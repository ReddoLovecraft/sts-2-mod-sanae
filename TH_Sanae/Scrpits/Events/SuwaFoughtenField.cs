using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using TH_Sanae.Scripts.Encounters;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Events;

public sealed class SuwaFoughtenField : SanaeEventModel
{
	public override string? CustomInitialPortraitPath => "res://TH_Sanae/ArtWorks/Events/suwafield.png";
	protected override IReadOnlySet<int> AllowedActs => new HashSet<int> { 3 };
	protected override bool RequiresAllSanaeParty => true;
	public override bool IsShared => true;
	private enum SuwaRoute
	{
		None,
		HelpKanako,
		HelpSuwako,
		StopBoth
	}

	private enum PendingBattle
	{
		None,
		Kanako,
		Suwako
	}

	private SuwaRoute _route;
	private PendingBattle _pendingBattle;
	private bool _wonKanako;
	private bool _wonSuwako;

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return
		[
			new EventOption(this, HelpKanako, $"{Id.Entry}.pages.INITIAL.options.HELP_KANAKO"),
			new EventOption(this, HelpSuwako, $"{Id.Entry}.pages.INITIAL.options.HELP_SUWAKO"),
			new EventOption(this, StopBoth, $"{Id.Entry}.pages.INITIAL.options.STOP_BOTH")
		];
	}

	private Task HelpKanako()
	{
		_route = SuwaRoute.HelpKanako;
		SetEventState(
			L10NLookup($"{Id.Entry}.pages.HELP_KANAKO.description"),
			[
				new EventOption(this, FightSuwako, $"{Id.Entry}.pages.HELP_KANAKO.options.FIGHT")
			]);
		return Task.CompletedTask;
	}

	private Task HelpSuwako()
	{
		_route = SuwaRoute.HelpSuwako;
		SetEventState(
			L10NLookup($"{Id.Entry}.pages.HELP_SUWAKO.description"),
			[
				new EventOption(this, FightKanako, $"{Id.Entry}.pages.HELP_SUWAKO.options.FIGHT")
			]);
		return Task.CompletedTask;
	}

	private Task StopBoth()
	{
		_route = SuwaRoute.StopBoth;
		SetEventState(
			L10NLookup($"{Id.Entry}.pages.STOP_BOTH.description"),
			[
				new EventOption(this, FightKanako, $"{Id.Entry}.pages.STOP_BOTH.options.FIGHT")
			]);
		return Task.CompletedTask;
	}

	private Task FightKanako()
	{
		_pendingBattle = PendingBattle.Kanako;
		EnterCombatWithoutExitingEvent<KanakoAndOnbashiraEventEncounter>(Array.Empty<Reward>(), shouldResumeAfterCombat: true);
		return Task.CompletedTask;
	}

	private Task FightSuwako()
	{
		_pendingBattle = PendingBattle.Suwako;
		EnterCombatWithoutExitingEvent<SuwakoEventEncounter>(Array.Empty<Reward>(), shouldResumeAfterCombat: true);
		return Task.CompletedTask;
	}

	public override async Task Resume(AbstractRoom room)
	{
		if (Owner?.Creature == null || !Owner.Creature.IsAlive)
		{
			return;
		}

		switch (_pendingBattle)
		{
			case PendingBattle.Kanako:
				await ResumeAfterKanako();
				break;
			case PendingBattle.Suwako:
				await ResumeAfterSuwako();
				break;
		}
	}

	private async Task ResumeAfterKanako()
	{
		_pendingBattle = PendingBattle.None;
		_wonKanako = true;

		if (_route == SuwaRoute.StopBoth && !_wonSuwako)
		{
			SetEventState(
				L10NLookup($"{Id.Entry}.pages.STOP_BOTH.description"),
				[
					new EventOption(this, FightSuwako, $"{Id.Entry}.pages.STOP_BOTH.options.FIGHT")
				]);
			return;
		}

		await PlayerCmd.GainGold(200, Owner!);
		await RelicCmd.Obtain(ModelDb.Relic<BrokenPartKanako>().ToMutable(), Owner!);
		SetEventFinished(L10NLookup($"{Id.Entry}.pages.HELP_SUWAKO_WIN.description"));
	}

	private async Task ResumeAfterSuwako()
	{
		_pendingBattle = PendingBattle.None;
		_wonSuwako = true;

		if (_route == SuwaRoute.StopBoth && _wonKanako)
		{
			await PlayerCmd.GainGold(300, Owner!);
			await RelicCmd.Obtain(ModelDb.Relic<FullMoriyaShrine>().ToMutable(), Owner!);

			ChangedTimeLine? timeline = Owner!.GetRelic<ChangedTimeLine>();
			if (timeline != null)
			{
				timeline.AddCounter();
			}
			else
			{
				await RelicCmd.Obtain(ModelDb.Relic<ChangedTimeLine>().ToMutable(), Owner!);
			}

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.STOP_BOTH_WIN.description"));
			return;
		}

		await PlayerCmd.GainGold(100, Owner!);
		await RelicCmd.Obtain(ModelDb.Relic<BrokenPartSuwako>().ToMutable(), Owner!);
		SetEventFinished(L10NLookup($"{Id.Entry}.pages.HELP_KANAKO_WIN.description"));
	}
}
