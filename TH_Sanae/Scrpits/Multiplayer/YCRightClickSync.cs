using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Models;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Multiplayer;

public static class YCRightClickSync
{
	private const uint YCRightClickChoiceId = 4000000001u;

	private static PlayerChoiceSynchronizer? _lastSynchronizer;

	public static void EnsureSubscribed()
	{
		PlayerChoiceSynchronizer synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
		if (synchronizer == null)
		{
			return;
		}

		if (ReferenceEquals(_lastSynchronizer, synchronizer))
		{
			return;
		}

		if (_lastSynchronizer != null)
		{
			_lastSynchronizer.PlayerChoiceReceived -= OnPlayerChoiceReceived;
		}

		_lastSynchronizer = synchronizer;
		synchronizer.PlayerChoiceReceived += OnPlayerChoiceReceived;
	}

	public static Task DoLocalAndSync(Player player, YCCardModel card, PlayerChoiceContext context, int hpLoss)
	{
		if (hpLoss <= 0 || card.NotYC)
		{
			return Task.CompletedTask;
		}

		if (RunManager.Instance.NetService.Type.IsMultiplayer())
		{
			uint cardId = NetCombatCardDb.Instance.GetCardId(card);
			PlayerChoiceMessage message = new()
			{
				choiceId = YCRightClickChoiceId,
				result = PlayerChoiceResult.FromIndexes([(int)cardId, hpLoss]).ToNetData()
			};
			RunManager.Instance.NetService.SendMessage(message);
		}

		return ApplyImmediate(player, card, context, hpLoss);
	}

	private static async void OnPlayerChoiceReceived(Player player, uint choiceId, NetPlayerChoiceResult result)
	{
		if (choiceId != YCRightClickChoiceId)
		{
			return;
		}

		if (result.type != PlayerChoiceType.Index || result.indexes == null || result.indexes.Count < 2)
		{
			return;
		}

		int cardIndex = result.indexes[0];
		int hpLoss = result.indexes[1];
		if (cardIndex < 0 || hpLoss <= 0)
		{
			return;
		}

		if (!NetCombatCardDb.Instance.TryGetCard((uint)cardIndex, out CardModel? card) || card is not YCCardModel ycCard)
		{
			return;
		}

		if (!ReferenceEquals(ycCard.Owner, player))
		{
			return;
		}

		await ApplyImmediate(player, ycCard, new ThrowingPlayerChoiceContext(), hpLoss);
	}

	private static async Task ApplyImmediate(Player player, YCCardModel card, PlayerChoiceContext context, int hpLoss)
	{
		if (hpLoss <= 0 || card.NotYC)
		{
			return;
		}

		await CreatureCmd.Damage(
			context,
			player.Creature,
			new DamageVar(hpLoss, ValueProp.Unpowered | ValueProp.Unblockable),
			card);
		ToolBox.UpgradeCard(card);
		card.NotYC = true;
	}
}

[HarmonyPatch(typeof(RunManager))]
[HarmonyPatch("InitializeShared")]
public static class YCRightClickSyncRunManagerPatch
{
	public static void Postfix()
	{
		YCRightClickSync.EnsureSubscribed();
	}
}
