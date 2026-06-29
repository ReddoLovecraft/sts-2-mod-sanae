using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scrpits.Cards;

namespace TH_Sanae.Scripts.Main
{
	public class SanaeCharacter : PlaceholderCharacterModel
	{
		public override Color NameColor => new Color("5faa06ff");
		public override Color EnergyLabelOutlineColor => new Color("8dcf04ff");
		public override Color DialogueColor => new Color("49c600ff");
		public override Color MapDrawingColor => new Color("39db00ff");
		public override Color RemoteTargetingLineColor => new Color("00bd16ff");
		public override Color RemoteTargetingLineOutline => new Color("3b9000ff");
		public override CharacterGender Gender => CharacterGender.Feminine;
		public override int StartingHp => 70;
		public override string CustomVisualPath => "res://TH_Sanae/ArtWorks/Character/sanae.tscn";
		public override string CustomTrailPath => "res://TH_Sanae/ArtWorks/VFX/SanaeCardTrail.tscn";
		public override string CustomIconTexturePath => "res://TH_Sanae/ArtWorks/Character/sanae_icon.png";
		public override string CustomIconPath => "res://TH_Sanae/ArtWorks/Character/sanae_icon.tscn";
		public override string CustomEnergyCounterPath => "res://TH_Sanae/ArtWorks/Character/sanae_energy_counter.tscn";
		// // 篝火休息动画。
		public override string CustomRestSiteAnimPath => "res://TH_Sanae/ArtWorks/Character/sanaerest.tscn";
		// // 商店人物动画。
		public override string CustomMerchantAnimPath => "res://TH_Sanae/ArtWorks/Character/sanae_merchant.tscn";
		public override string CustomArmPointingTexturePath => "res://TH_Sanae/ArtWorks/Character/multiplayer_hand_sanae_point.png";
		public override string CustomArmRockTexturePath => "res://TH_Sanae/ArtWorks/Character/multiplayer_hand_sanae_rock.png";
		public override string CustomArmPaperTexturePath => "res://TH_Sanae/ArtWorks/Character/multiplayer_hand_sanae_paper.png";
		public override string CustomArmScissorsTexturePath => "res://TH_Sanae/ArtWorks/Character/multiplayer_hand_sanae_scissors.png";
		public override string CustomCharacterSelectBg => "res://TH_Sanae/ArtWorks/Character/Sanae_bg.tscn";
		public override string CustomCharacterSelectIconPath => "res://TH_Sanae/ArtWorks/Character/char_select_sanae.png";
		public override string CustomCharacterSelectLockedIconPath => "res://TH_Sanae/ArtWorks/Character/char_select_sanae_locked.png";
		public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/silent_transition_mat.tres";
		public override string CustomMapMarkerPath => "res://TH_Sanae/ArtWorks/Character/map_marker_sanae.png";
		// 攻击音效
		public override string CustomAttackSfx => SanaeInit.ToModSfxPath("TH_Sanae/ArtWorks/SFX/attack.wav");
		// // 施法音效
		public override string CustomCastSfx => SanaeInit.ToModSfxPath("TH_Sanae/ArtWorks/SFX/cast.wav");
		// // 死亡音效
		public override string CustomDeathSfx => SanaeInit.ToModSfxPath("TH_Sanae/ArtWorks/SFX/die.ogg");
		public override string CharacterSelectSfx  => SanaeInit.ToModSfxPath("TH_Sanae/ArtWorks/SFX/characterselect.ogg");
		public override string CharacterTransitionSfx => SanaeInit.ToModSfxPath("TH_Sanae/ArtWorks/SFX/transition.wav");
		public override CardPoolModel CardPool => ModelDb.CardPool<SanaeCardPool>();
		public override RelicPoolModel RelicPool => ModelDb.RelicPool<SanaeRelicPool>();
		public override PotionPoolModel PotionPool => ModelDb.PotionPool<SanaePotionPool>();



		// 初始卡组
		public override IEnumerable<CardModel> StartingDeck => [
			ModelDb.Card<Strike>(),
			ModelDb.Card<Strike>(),
			ModelDb.Card<Strike>(),
			ModelDb.Card<Strike>(),
			ModelDb.Card<Defend>(),
			ModelDb.Card<Defend>(),
			ModelDb.Card<Defend>(),
			ModelDb.Card<Defend>(),
			ModelDb.Card<GrayMiracle>(),
			ModelDb.Card<MoriyaProtection>()
	];

		// 初始遗物
		public override IReadOnlyList<RelicModel> StartingRelics => [
			ModelDb.Relic<FakeFaith>()
	];

		// 攻击建筑师的攻击特效列表
		public override List<string> GetArchitectAttackVfx() => [
		"vfx/vfx_attack_slash",
		"vfx_starry_impact",
		"vfx/vfx_giant_horizontal_slash",
		"vfx/vfx_attack_slash",
		"vfx_starry_impact",
		"vfx/vfx_giant_horizontal_slash",
		"vfx/vfx_attack_slash",
		"vfx_starry_impact",
		"vfx/vfx_giant_horizontal_slash",
		"vfx/vfx_attack_slash"
		];
		public override CreatureAnimator GenerateAnimator(MegaSprite controller)
		{
			AnimState animState = new AnimState("Idle", isLooping: true);
			AnimState animState2 = new AnimState("Cast");
			AnimState animState3 = new AnimState("Attack");
			AnimState animState4 = new AnimState("Hit");
			AnimState state = new AnimState("die");
			AnimState animState5 = new AnimState("relaxed_loop", isLooping: true);
			AnimState animState6 = new AnimState("Spell");
			AnimState animState7 = new AnimState("AttackS");
			AnimState animState8 = new AnimState("throw");
			animState2.NextState = animState;
			animState3.NextState = animState;
			animState4.NextState = animState;
			animState6.NextState = animState;
			animState7.NextState = animState;
			animState8.NextState = animState;
			animState5.AddBranch("Idle", animState);
			CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
			creatureAnimator.AddAnyState("Idle", animState);
			creatureAnimator.AddAnyState("Dead", state);
			creatureAnimator.AddAnyState("Hit", animState4);
			creatureAnimator.AddAnyState("Attack", animState3);
			creatureAnimator.AddAnyState("Cast", animState2);
			creatureAnimator.AddAnyState("Spell", animState6);
			creatureAnimator.AddAnyState("AttakS", animState7);
			creatureAnimator.AddAnyState("Throw", animState8);
			creatureAnimator.AddAnyState("relaxed_loop", animState5);		
			return creatureAnimator;
		}
	}
}
/*
神奈子
先古之民遗物（2，3幕都可出现）

衔尾蛇纹章
每当你的抽牌堆洗牌时，为你的所有卡牌添加重放1。

注连绳圈


乾神祝福

天行健，君子以自强不息。

神之粥
在你的回合开始时，将一张丰穰之米洗入你的抽牌堆顶部。

迷你御柱
将你的牌组中的打击全部替换为乾神招来-御柱，同时将其的耗能减少1点。

山废精酿
拾起时，用随机药水填满你的药水栏位。
每当你使用药水时，将你的最大生命提高5点。

神灵金锭
你不会受到负面效果影响。
(Goety Revelation)

高达模型
你的卡牌奖励选项增加2个。
你的卡牌奖励选项将被升级。

核聚变炉
在你的回合开始时，消耗一张牌，获得一点能量，然后给予所有敌人6层点燃。

献给神山的供物
将要杀死敌人时，饶恕该敌人。（可以规避复活等效果，比如把千足虫三段中的一段直接送走）
被饶恕的敌人会留下随机的奖励。
“献给大山的，便是属于我的！”
啊啊，乍一听是多么自私的说法啊。

*/
