using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace TH_Sanae.Scripts.Enchantments
{
public sealed class GhostCurse : CustomEnchantmentModel
{
	public override bool HasExtraCardText => true;
	protected override string? CustomIconPath => "res://TH_Sanae/ArtWorks/Enchants/gc.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

		protected override void OnEnchant()
	{
		base.Card.AddKeyword(CardKeyword.Exhaust);
	}
	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card == this.Card)
		{
			await Cmd.Wait(0.25f);
			await CardCmd.AutoPlay(choiceContext,card,null);
		}
	}
	
}
}