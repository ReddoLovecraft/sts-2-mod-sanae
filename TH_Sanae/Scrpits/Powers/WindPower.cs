using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
public sealed class WindPower : SanaePowerModel
{
	private bool _applyingAyaFollowBonus;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (power != this || amount <= 0m || _applyingAyaFollowBonus)
		{
			return;
		}

		AyaFollow? relic = Owner.Player?.GetRelic<AyaFollow>();
		if (relic == null)
		{
			return;
		}

		_applyingAyaFollowBonus = true;
		try
		{
			relic.Flash();
			await PowerCmd.ModifyAmount(choiceContext, this, amount, null, cardSource);
		}
		finally
		{
			_applyingAyaFollowBonus = false;
		}
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner.Player)
		{
			return;
		}

		int currentAmount = base.Amount;
		Color color = new Color("FFFFFF80");
		double num2 = ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2 : 0.3);
		NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(color, 0.8 + (double)Mathf.Min(8, Amount) * num2));
		SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");
		NRun.Instance?.GlobalUi.AddChildSafely(NSmokyVignetteVfx.Create(color, color));

		var livingEnemies = base.CombatState.HittableEnemies.ToList();
		if (livingEnemies.Count > 0)
		{
			Flash();
			await CreatureCmd.Damage(choiceContext, livingEnemies, currentAmount, ValueProp.Unpowered, base.Owner, null);
		}

		if (base.Owner.HasPower<MasterWindPower>() && base.Owner.HasPower<WindStatePower>())
		{
			return;
		}

		int decayAmount = currentAmount / 2;
		if (base.Owner.HasPower<WindGodLakePower>())
		{
			int lakeAmount = base.Owner.GetPowerAmount<WindGodLakePower>();
			decimal decayMultiplier = 1m - (lakeAmount * 0.1m);
			if (decayMultiplier < 0m)
			{
				decayMultiplier = 0m;
			}

			decayAmount = (int)(decayAmount * decayMultiplier);
		}

		int nextAmount = currentAmount - decayAmount;
		if (nextAmount <= 0)
		{
			await PowerCmd.ModifyAmount(choiceContext, this, -currentAmount, null, null);
			return;
		}

		await PowerCmd.ModifyAmount(choiceContext, this, nextAmount - currentAmount, null, null);
	}
}
}
