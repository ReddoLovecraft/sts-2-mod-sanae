using BaseLib.Abstracts;
using Godot;

namespace TH_Sanae.Scripts.Main
{
	public class SanaeCardPool : CustomCardPoolModel
	{
		public override string Title => "TH_Sanae";

		public override Color ShaderColor => new Color("45d72cff");
		public override Color DeckEntryCardColor => new Color("5dce00ff");
		public override string? BigEnergyIconPath => "res://TH_Sanae/ArtWorks/Character/card_orb.png";
		public override string? TextEnergyIconPath => "res://TH_Sanae/ArtWorks/Character/cost_orb.png";
		public override bool IsColorless => false;

	}
}
