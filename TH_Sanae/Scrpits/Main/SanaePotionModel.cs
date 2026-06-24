using BaseLib.Abstracts;
using BaseLib.Patches.Content;

namespace TH_Sanae.Scripts.Main
{
	public abstract class SanaePotionModel : CustomPotionModel
	{
		protected SanaePotionModel(bool autoAdd = true)
		{
			if (autoAdd)
			{
				CustomContentDictionary.AddModel(GetType());
			}
		}
	}
}
