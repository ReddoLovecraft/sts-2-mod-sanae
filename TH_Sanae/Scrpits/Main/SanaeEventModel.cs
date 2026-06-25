using BaseLib.Abstracts;
using BaseLib.Patches.Content;

namespace TH_Sanae.Scripts.Main
{
	public abstract class SanaeEventModel : CustomEventModel
	{
		protected SanaeEventModel(bool autoAdd = true)
		{
			if (autoAdd)
			{
				CustomContentDictionary.AddModel(GetType());
			}
		}
	}
}
