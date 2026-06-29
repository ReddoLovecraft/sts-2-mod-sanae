using BaseLib.Config;

namespace TH_Sanae.Scripts.Main;

[ConfigHoverTipsByDefault]
public sealed class SanaeModConfig : SimpleModConfig
{
	[ConfigSection("Cards")]
	[ConfigHoverTip]
	public static bool NsfwCardArt { get; set; } = false;

	[ConfigSection("Ancients")]
	[ConfigHoverTip]
	public static bool TwoGodProtection { get; set; } = false;
}
