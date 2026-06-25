using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TH_Sanae.Scripts.Monsters;

namespace TH_Sanae.Scripts.Encounters;

public sealed class KanakoAndOnbashiraEventEncounter : CustomEncounterModel
{
	public const string KanakoSlot = "kanako";

	public static IReadOnlyList<string> OnbashiraSlots =>
	[
		"left_top_1",
		"left_bottom_1",
		"right_top_1",
		"right_bottom_1",
		"left_top_2",
		"left_bottom_2",
		"right_top_2",
		"right_bottom_2"
	];

	public override IReadOnlyList<string> Slots =>
	[
		.. OnbashiraSlots,
		KanakoSlot
	];

	public override IEnumerable<MonsterModel> AllPossibleMonsters =>
	[
		ModelDb.Monster<KanakoBoss>(),
		ModelDb.Monster<OnbashiraMinion>()
	];

	public override bool IsValidForAct(ActModel act) => false;

	public override bool IsWeak => false;

	public KanakoAndOnbashiraEventEncounter() : base(RoomType.Monster)
	{
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
	[
		(ModelDb.Monster<OnbashiraMinion>().ToMutable(), OnbashiraSlots[0]),
		(ModelDb.Monster<OnbashiraMinion>().ToMutable(), OnbashiraSlots[1]),
		(ModelDb.Monster<OnbashiraMinion>().ToMutable(), OnbashiraSlots[2]),
		(ModelDb.Monster<OnbashiraMinion>().ToMutable(), OnbashiraSlots[3]),
		(ModelDb.Monster<KanakoBoss>().ToMutable(), KanakoSlot)
	];
}
