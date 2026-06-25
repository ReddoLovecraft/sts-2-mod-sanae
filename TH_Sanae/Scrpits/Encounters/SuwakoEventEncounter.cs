using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TH_Sanae.Scripts.Monsters;

namespace TH_Sanae.Scripts.Encounters;

public sealed class SuwakoEventEncounter : CustomEncounterModel
{
	public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<SuwakoBoss>()];

	public override bool IsValidForAct(ActModel act) => false;

	public override bool IsWeak => false;

	public SuwakoEventEncounter() : base(RoomType.Monster)
	{
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
	[
		(ModelDb.Monster<SuwakoBoss>().ToMutable(), null)
	];
}
