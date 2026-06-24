using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(ColorlessCardPool))]
	public sealed class TechInnovation : SanaeCardModel
	{
		public TechInnovation() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

			foreach (CardModel card in ToolBox.GetPile(Owner, PileType.Hand)?.Cards.ToList() ?? [])
			{
				if (!card.IsUpgradable || card.DeckVersion?.IsUpgradable != true)
				{
					continue;
				}

				ToolBox.UpgradeCard(card);
				ToolBox.UpgradeCard(card.DeckVersion);
			}

			BreakCombatRendering();
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}

		private static void BreakCombatRendering()
		{
			NCombatRoom? room = NCombatRoom.Instance;
			if (room == null)
			{
				return;
			}

			TechInnovationRenderGlitch? glitch = room.GetNodeOrNull<TechInnovationRenderGlitch>(nameof(TechInnovationRenderGlitch));
			if (glitch == null)
			{
				glitch = new TechInnovationRenderGlitch
				{
					Name = nameof(TechInnovationRenderGlitch)
				};
				room.AddChild(glitch);
			}

			glitch.Activate(room);
		}
	}

	internal sealed partial class TechInnovationRenderGlitch : Node
	{
		private readonly Dictionary<CanvasItem, bool> _hiddenTextNodes = [];
		private NCombatRoom? _room;
		private bool _cachedRelicInventoryVisible = true;
		private double _elapsed;

		public void Activate(NCombatRoom room)
		{
			_room = room;
			_elapsed = 0.0;
			if (NRun.Instance?.GlobalUi != null)
			{
				_cachedRelicInventoryVisible = NRun.Instance.GlobalUi.RelicInventory.Visible;
			}

			SetProcess(true);
			HideTextNodes(room);
			HideGlobalUiTextAndRelics();
			ApplyCreatureFlicker();
		}

		public override void _Process(double delta)
		{
			if (_room == null || !GodotObject.IsInstanceValid(_room))
			{
				QueueFree();
				return;
			}

			_elapsed += delta;
			HideTextNodes(_room);
			HideGlobalUiTextAndRelics();
			ApplyCreatureFlicker();
		}

		public override void _ExitTree()
		{
			base._ExitTree();
			RestoreVisuals();
		}

		private void HideGlobalUiTextAndRelics()
		{
			if (NRun.Instance?.GlobalUi == null)
			{
				return;
			}

			HideTextNodes(NRun.Instance.GlobalUi.TopBar);
			NRun.Instance.GlobalUi.RelicInventory.Visible = false;
		}

		private void HideTextNodes(Node root)
		{
			foreach (Node child in root.GetChildren())
			{
				HideTextNodes(child);
			}

			if (root is not CanvasItem canvasItem || !IsTextNode(root))
			{
				return;
			}

			if (!_hiddenTextNodes.ContainsKey(canvasItem))
			{
				_hiddenTextNodes.Add(canvasItem, canvasItem.Visible);
			}

			canvasItem.Visible = false;
		}

		private void ApplyCreatureFlicker()
		{
			if (_room == null)
			{
				return;
			}

			int index = 0;
			foreach (var creatureNode in _room.CreatureNodes)
			{
				float phase = (float)(_elapsed * 7.0 + index * 0.9);
				float alpha = Mathf.Sin(phase) > 0.2f ? 1.0f : 0.15f;
				Color modulate = creatureNode.Visuals.Modulate;
				modulate.A = alpha;
				creatureNode.Visuals.Modulate = modulate;
				index++;
			}
		}

		private void RestoreVisuals()
		{
			foreach ((CanvasItem canvasItem, bool wasVisible) in _hiddenTextNodes)
			{
				if (GodotObject.IsInstanceValid(canvasItem))
				{
					canvasItem.Visible = wasVisible;
				}
			}

			if (_room != null && GodotObject.IsInstanceValid(_room))
			{
				foreach (var creatureNode in _room.CreatureNodes)
				{
					Color modulate = creatureNode.Visuals.Modulate;
					modulate.A = 1.0f;
					creatureNode.Visuals.Modulate = modulate;
				}
			}

			if (NRun.Instance?.GlobalUi != null)
			{
				NRun.Instance.GlobalUi.RelicInventory.Visible = _cachedRelicInventoryVisible;
			}
		}

		private static bool IsTextNode(Node node)
		{
			string typeName = node.GetType().Name;
			return node is Label
				|| node is RichTextLabel
				|| typeName.Contains("MegaLabel")
				|| typeName.Contains("MegaRichTextLabel");
		}
	}
}
