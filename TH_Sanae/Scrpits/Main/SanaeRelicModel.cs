using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace TH_Sanae.Scripts.Main
{
	public abstract class SanaeRelicModel : CustomRelicModel
	{
		protected SanaeRelicModel(bool autoAdd = true)
		{
			if (autoAdd)
			{
				CustomContentDictionary.AddModel(GetType());
			}
		}
	}
}
