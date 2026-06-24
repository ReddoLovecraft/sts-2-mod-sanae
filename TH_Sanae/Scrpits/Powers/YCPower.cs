using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
public sealed class YCPower : CustomPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
    //public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/YCP32.png";
    //public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/YCP64.png";
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => cardsTip;
    public List<IHoverTip> cardsTip=new List<IHoverTip>();
    public List<YCCardModel> cards=new List<YCCardModel>();
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        cardsTip = new List<IHoverTip>();
        cards = new List<YCCardModel>();
    }
    public void SetCardAndHoverTip(IHoverTip tip,YCCardModel yccm)
    {
        this.cardsTip.Clear();
        this.cardsTip.Add(tip);
        this.cards.Clear();
        YCCardModel card = (YCCardModel)yccm.CreateDupe();
        card.YC_count=yccm.YC_count;
        card.NotYC=true;
        this.cards.Add(card);
        ((StringVar)base.DynamicVars["Card"]).StringValue = card.Title;
    }
    protected override IEnumerable<DynamicVar> CanonicalVars => [(new StringVar("Card"))];
	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player)
        {
            return;
        }
        if(Amount>1)
        {
            await PowerCmd.Decrement(this);
        }
        else
        {
            await CardCmd.AutoPlay(choiceContext, cards[0], null);
            await PowerCmd.Decrement(this);
        }
    }

}
}


