using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;
namespace TH_Sanae.Scrpits.Cards
{
[Pool(typeof(SanaeCardPool))]
public class GrayMiracle : YCCardModel
{
	public override int YC_count
	{
		get => 1;
		set { }
	}
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move),new CardsVar(2)];
	protected override IEnumerable<IHoverTip> ExtraHoverTips 
	{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				List<IHoverTip> tips = new List<IHoverTip>(UpgradeExtraHoverTips)
				{
					Tools.GetStaticKeyword("Spellcard"),
					HoverTipFactory.FromKeyword(CardModifier.MiracleKeyword),
					HoverTipFactory.FromPower<BeliefPower>(),
					Tools.GetStaticKeyword("Chant")
				};
				return tips;
			}
		}
	public GrayMiracle() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{

       if(!NotYC && await QueueSingleChantWithPreview(choiceContext, YC_count, $"yc-{CurrentUpgradeLevel}", cardPlay.Target))
	   {
		 return;
	   }
	   else
	   {
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue) .FromCard(this) .WithHitFx("vfx/vfx_starry_impact").Targeting(cardPlay.Target).Execute(choiceContext);
		await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature,this);
		NotYC=false;
	   }
	}
	protected override void firstUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3); 
		DynamicVars.Cards.UpgradeValueBy(2);
	}

	protected override void secondUpgrade()
	{
		this.AddKeyword(CardKeyword.Innate);
		DynamicVars.Damage.UpgradeValueBy(2); 		
	}
}

}
