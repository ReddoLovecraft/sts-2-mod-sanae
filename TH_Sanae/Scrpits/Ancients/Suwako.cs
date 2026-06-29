using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Models;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Main.Ancients
{
public sealed class Suwako : SanaeAncientModel
{
	// // 选项按钮颜色
	public override Color ButtonColor => new(0.335f, 0.259f, 0.522f, 0.5f);
	// // 对话框颜色
	public override Color DialogueColor => new(0.335f, 0.259f, 0.522f, 0.5f);

	// // 自定义场景的路径
	 public override string? CustomScenePath => "res://TH_Sanae/ArtWorks/Ancients/suwako.tscn";
	// // 自定义地图图标和轮廓的路径
	public override string? CustomMapIconPath => "res://TH_Sanae/ArtWorks/Ancients/suwako.png";
	public override string? CustomMapIconOutlinePath => "res://TH_Sanae/ArtWorks/Ancients/suwako_outline.png";
	// // 历史记录图标路径
	public override string? CustomRunHistoryIconPath => "res://TH_Sanae/ArtWorks/Ancients/suwako_icon.png";
	public override string? CustomRunHistoryIconOutlinePath => "res://TH_Sanae/ArtWorks/Ancients/suwako_icon.png";

	public override bool IsValidForAct(ActModel act)
	{
		if (!base.IsValidForAct(act))
		{
			return false;
		}

		if (!SanaeModConfig.TwoGodProtection)
		{
			return true;
		}

		return act.ActNumber() == 2;
	}

	protected override OptionPools MakeOptionPools { get; } = new OptionPools(
		MakePool(
			AncientOption<Ichimegasa>(),
			AncientOption<TsuboShozoku>(),
			AncientOption<AncientWheel>()
		),
		MakePool(
			AncientOption<KunBless>(),
			AncientOption<RedFrogBomb>(),
			AncientOption<MarkedEvent>()
		),
		MakePool(
			AncientOption<CurseMaterial>(),
			AncientOption<BottleOfDoom>(),
			AncientOption<OldStationCard>(),
			AncientOption<Emerald>()
		)
	);
	

}

}
