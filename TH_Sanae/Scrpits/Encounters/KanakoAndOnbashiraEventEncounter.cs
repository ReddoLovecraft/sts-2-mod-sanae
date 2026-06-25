using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TH_Sanae.Scripts.Monsters;

namespace TH_Sanae.Scripts.Encounters;

public sealed class KanakoAndOnbashiraEventEncounter : CustomEncounterModel
{
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
		(ModelDb.Monster<OnbashiraMinion>().ToMutable(), null),
		(ModelDb.Monster<OnbashiraMinion>().ToMutable(), null),
		(ModelDb.Monster<OnbashiraMinion>().ToMutable(), null),
		(ModelDb.Monster<OnbashiraMinion>().ToMutable(), null),
		(ModelDb.Monster<KanakoBoss>().ToMutable(), null)
	];
}
