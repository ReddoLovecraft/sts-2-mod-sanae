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
	private const uint ChewingWineRightClickChoiceId = 4000000002u;
	private const uint ItemPRightClickChoiceId = 4000000003u;
	private const uint HinaNingyouRightClickChoiceId = 4000000004u;

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

	public static Task DoChewingWineLocalAndSync(Player player, ChewingWine relic, PlayerChoiceContext context)
	{
		if (relic.Charges <= 0 || player.Creature.CombatState == null || player.Creature.CombatState.CurrentSide != player.Creature.Side)
		{
			return Task.CompletedTask;
		}

		if (RunManager.Instance.NetService.Type.IsMultiplayer())
		{
			PlayerChoiceMessage message = new()
			{
				choiceId = ChewingWineRightClickChoiceId,
				result = PlayerChoiceResult.FromIndexes([1]).ToNetData()
			};
			RunManager.Instance.NetService.SendMessage(message);
		}

		return ApplyChewingWineImmediate(player, relic, context);
	}

	public static Task DoItemPLocalAndSync(Player player, ItemP relic, PlayerChoiceContext context)
	{
		if (relic.Count <= 0 || player.Creature.CombatState == null || player.Creature.CombatState.CurrentSide != player.Creature.Side)
		{
			return Task.CompletedTask;
		}

		if (RunManager.Instance.NetService.Type.IsMultiplayer())
		{
			PlayerChoiceMessage message = new()
			{
				choiceId = ItemPRightClickChoiceId,
				result = PlayerChoiceResult.FromIndexes([1]).ToNetData()
			};
			RunManager.Instance.NetService.SendMessage(message);
		}

		return ApplyItemPImmediate(player, relic, context);
	}

	public static Task DoHinaNingyouLocalAndSync(Player player, HinaNingyou relic, PlayerChoiceContext context)
	{
		if (RunManager.Instance.NetService.Type.IsMultiplayer())
		{
			PlayerChoiceMessage message = new()
			{
				choiceId = HinaNingyouRightClickChoiceId,
				result = PlayerChoiceResult.FromIndexes([1]).ToNetData()
			};
			RunManager.Instance.NetService.SendMessage(message);
		}

		return ApplyHinaNingyouImmediate(player, relic, context);
	}

	private static async void OnPlayerChoiceReceived(Player player, uint choiceId, NetPlayerChoiceResult result)
	{
		if (choiceId == ChewingWineRightClickChoiceId)
		{
			if (result.type != PlayerChoiceType.Index)
			{
				return;
			}

			if (player.GetRelic<ChewingWine>() is not ChewingWine relic)
			{
				return;
			}

			await ApplyChewingWineImmediate(player, relic, new ThrowingPlayerChoiceContext());
			return;
		}

		if (choiceId == ItemPRightClickChoiceId)
		{
			if (result.type != PlayerChoiceType.Index)
			{
				return;
			}

			if (player.GetRelic<ItemP>() is not ItemP relic)
			{
				return;
			}

			await ApplyItemPImmediate(player, relic, new ThrowingPlayerChoiceContext());
			return;
		}

		if (choiceId == HinaNingyouRightClickChoiceId)
		{
			if (result.type != PlayerChoiceType.Index)
			{
				return;
			}

			if (player.GetRelic<HinaNingyou>() is not HinaNingyou relic)
			{
				return;
			}

			await ApplyHinaNingyouImmediate(player, relic, new ThrowingPlayerChoiceContext());
			return;
		}

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

	private static async Task ApplyChewingWineImmediate(Player player, ChewingWine relic, PlayerChoiceContext context)
	{
		if (relic.Charges <= 0 || player.Creature.CombatState == null)
		{
			return;
		}

		relic.Flash();
		await PlayerCmd.GainEnergy(3, player);
		relic.Charges -= 1;
		relic.RefreshStatus(inCombat: true);
	}

	private static async Task ApplyItemPImmediate(Player player, ItemP relic, PlayerChoiceContext context)
	{
		if (relic.Count <= 0 || player.Creature.CombatState == null)
		{
			return;
		}

		relic.Flash();
		await CreatureCmd.Damage(context, player.Creature.CombatState.HittableEnemies, 10, ValueProp.Unpowered, player.Creature, null);
		relic.Count -= 1;
		relic.RefreshStatus(inCombat: true);
	}

	private static async Task ApplyHinaNingyouImmediate(Player player, HinaNingyou relic, PlayerChoiceContext context)
	{
		relic.Flash();
		var removableDeckCards = PileType.Deck.GetPile(player)
			.Cards
			.Where(card => card.Type == CardType.Curse || card.Type == CardType.Status)
			.ToList();
		if (removableDeckCards.Count > 0)
		{
			await CardPileCmd.RemoveFromDeck(removableDeckCards);
		}

		var cardsToExhaust = player.PlayerCombatState?.AllCards
			.Where(card => card.Pile?.Type != PileType.Exhaust && (card.Type == CardType.Curse || card.Type == CardType.Status))
			.ToList() ?? [];
		foreach (CardModel card in cardsToExhaust)
		{
			if (card.Pile?.Type == PileType.Exhaust)
			{
				continue;
			}

			await CardCmd.Exhaust(context, card);
		}

		await RelicCmd.Replace(relic, ModelDb.Relic<GoneHinaNingyou>().ToMutable());
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
