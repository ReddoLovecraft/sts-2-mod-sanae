using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace TH_Sanae.Scripts.Main
{
	internal sealed class YCPreviewCardHoverTip : CardHoverTip, IHoverTip
	{
		private readonly string _id;

		public YCPreviewCardHoverTip(CardModel card, string idSuffix) : base(card)
		{
			_id = $"{card.Id}+{card.CurrentUpgradeLevel}:{idSuffix}";
		}

		string IHoverTip.Id => _id;

		bool IHoverTip.IsInstanced => true;
	}
}
