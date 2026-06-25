using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class SnakeTalisman : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";

		private bool _hasTriggeredFirstTurn;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

		public override RelicRarity Rarity => RelicRarity.Uncommon;

		public bool HasTriggeredFirstTurn
		{
			get => _hasTriggeredFirstTurn;
			set
			{
				AssertMutable();
				_hasTriggeredFirstTurn = value;
			}
		}

		public override async Task BeforeCombatStart()
		{
			HasTriggeredFirstTurn = false;
			Flash();
			await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 8, Owner.Creature, null);
		}

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner)
			{
				return;
			}

			if (!HasTriggeredFirstTurn)
			{
				HasTriggeredFirstTurn = true;
				return;
			}

			Flash();
			await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, 4, Owner.Creature, null);
		}
	}
}
