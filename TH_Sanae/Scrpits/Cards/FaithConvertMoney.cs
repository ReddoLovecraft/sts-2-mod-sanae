using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class FaithConvertMoney : SanaeCardModel
	{
		private const string CoinFlipTexturePath = "res://TH_Sanae/ArtWorks/VFX/coin_flip_anim.png";

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>()];

		public FaithConvertMoney() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int totalFaith = 0;
			foreach (var enemy in (CombatState?.HittableEnemies ?? []).ToList())
			{
				if (!enemy.HasPower<BeliefPower>())
				{
					continue;
				}
				int faith = enemy.GetPowerAmount<BeliefPower>();
				totalFaith += faith;
				PlayTreasureGoldVfx(enemy, faith);
				await PowerCmd.Remove<BeliefPower>(enemy);
			}

			if (totalFaith > 0)
			{
				await PlayerCmd.GainGold(totalFaith, Owner);
			}
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
		}

		private static void PlayTreasureGoldVfx(Creature target, int amount)
		{
			if (amount <= 0 || TestMode.IsOn || NCombatRoom.Instance == null || target.IsDead)
			{
				return;
			}

			Node? container = NCombatRoom.Instance.CombatVfxContainer;
			if (container == null)
			{
				return;
			}

			var creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
			if (creatureNode == null)
			{
				return;
			}

			Texture2D tex = ResourceLoader.Load<Texture2D>(CoinFlipTexturePath);
			if (tex == null)
			{
				return;
			}

			var particles = new GpuParticles2D
			{
				Emitting = false,
				Amount = System.Math.Max(1, amount),
				Texture = tex,
				Lifetime = 2.5f,
				OneShot = true,
				SpeedScale = 1.8f,
				Explosiveness = 0.5f,
				FixedFps = 60
			};

			particles.Material = new CanvasItemMaterial
			{
				ParticlesAnimation = true,
				ParticlesAnimHFrames = 4,
				ParticlesAnimVFrames = 3,
				ParticlesAnimLoop = false
			};

			var alphaCurve = new Curve();
			alphaCurve.AddPoint(new Vector2(0f, 0f));
			alphaCurve.AddPoint(new Vector2(0.210821f, 0.94636f));
			alphaCurve.AddPoint(new Vector2(0.636194f, 0.823755f));
			alphaCurve.AddPoint(new Vector2(1f, 0f));

			var colorGradient = new Gradient();
			colorGradient.AddPoint(0f, new Color(0.342581f, 0.342581f, 0.342581f, 1f));
			colorGradient.AddPoint(0.595745f, Colors.White);

			var process = new ParticleProcessMaterial
			{
				LifetimeRandomness = 0.2f,
				ParticleFlagDisableZ = true,
				EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere,
				EmissionSphereRadius = 150f,
				AngleMin = -180f,
				AngleMax = 180f,
				Direction = new Vector3(0f, -1f, 0f),
				Spread = 40f,
				InitialVelocityMin = 150f,
				InitialVelocityMax = 600f,
				AngularVelocityMin = -720f,
				AngularVelocityMax = 720f,
				Gravity = new Vector3(0f, 800f, 0f),
				ScaleMin = 0.8f,
				ScaleMax = 1.2f,
				ColorRamp = new GradientTexture1D
				{
					Gradient = colorGradient,
					Width = 64
				},
				AlphaCurve = new CurveTexture
				{
					Curve = alphaCurve,
					Width = 128
				},
				HueVariationMin = -0.01f,
				HueVariationMax = 0.01f,
				AnimSpeedMax = 0.56f,
				AnimOffsetMax = 1f
			};

			particles.ProcessMaterial = process;
			Vector2 pos = creatureNode.Hitbox.GlobalPosition + creatureNode.Hitbox.Size * 0.5f;
			pos.Y -= creatureNode.Hitbox.Size.Y * 0.18f;
			particles.GlobalPosition = pos;
			container.AddChildSafely(particles);
			particles.Emitting = true;

			TaskHelper.RunSafely(FreeAfter(particles));
		}

		private static async Task FreeAfter(GpuParticles2D particles)
		{
			await Cmd.Wait((float)particles.Lifetime + 0.1f);
			if (GodotObject.IsInstanceValid(particles))
			{
				particles.QueueFree();
			}
		}
	}
}

