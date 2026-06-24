using BaseLib.Abstracts;

namespace TH_Sanae.Scripts.Main
{
	public abstract class SanaePowerModel : CustomPowerModel
	{
		public virtual void Trigger()
		{
			Flash();
		}
	}
	
}
