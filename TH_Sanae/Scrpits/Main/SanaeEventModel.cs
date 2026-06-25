using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace TH_Sanae.Scripts.Main
{
	public abstract class SanaeEventModel : CustomEventModel
	{
		protected virtual IReadOnlySet<int>? AllowedActs => null;

		protected virtual bool RequiresAllSanaeParty => false;

		protected virtual bool RequiresSanaeInParty => false;

		protected SanaeEventModel(bool autoAdd = true)
		{
			if (autoAdd)
			{
				CustomContentDictionary.AddModel(GetType());
			}
		}

		public override bool IsAllowed(IRunState runState)
		{
			if (!base.IsAllowed(runState))
			{
				return false;
			}

			if (AllowedActs != null)
			{
				if (!AllowedActs.Contains(runState.Act.Index))
				{
					return false;
				}
			}

			RunState? concreteRunState = runState as RunState;
			if (RequiresAllSanaeParty)
			{
				return concreteRunState != null && concreteRunState.Players.Count > 0 && concreteRunState.Players.All(IsSanae);
			}

			if (RequiresSanaeInParty)
			{
				return concreteRunState != null && concreteRunState.Players.Any(IsSanae);
			}

			return true;
		}

		private static bool IsSanae(Player player) => player.Character is SanaeCharacter;
	}
}
