using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scrpits.Cards;
using TH_Sanae.Scripts.Main;
using BaseLib.Extensions;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Patches
{
	internal enum DrawResultType
	{
		GreatLuck,
		Luck,
		BadLuck,
		GreatCurse
	}

	[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardDrawn))]
	public static class CardKeywordAfterDrawPatch
	{
		private sealed class DrawResultHolder
		{
			public DrawResultType Result;
		}

		private static readonly ConditionalWeakTable<CardModel, DrawResultHolder> DrawResults = new();

		public static void Postfix(ref Task __result, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			__result = ApplyKeywordEffectsAsync(__result, choiceContext, card, fromHandDraw);
		}

		private static async Task ApplyKeywordEffectsAsync(Task originalTask, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			await originalTask;

			if (card.Owner == null || !card.IsMutable)
			{
				return;
			}

			if (card.Keywords.Contains(CardModifier.DrawKeyword))
			{
				ApplyDrawKeyword(card);
			}

			if (card.Keywords.Contains(CardModifier.MiracleKeyword))
			{
				await ApplyMiracleKeyword(choiceContext, card);
			}
		}

		private static void ApplyDrawKeyword(CardModel card)
		{
			DrawResultType result = RollDrawResult(card);
			DrawResults.Remove(card);
			DrawResults.Add(card, new DrawResultHolder { Result = result });
			CardModel referenceCard = CreateReferenceCard(card);

			ApplyVarIfPresent(card, card.DynamicVars, referenceCard.DynamicVars, "Cards", result);
			ApplyVarIfPresent(card, card.DynamicVars, referenceCard.DynamicVars, "Damage", result);
			ApplyVarIfPresent(card, card.DynamicVars, referenceCard.DynamicVars, "Block", result);

			foreach (KeyValuePair<string, DynamicVar> pair in card.DynamicVars.Where(static pair => pair.Key.EndsWith("Power", StringComparison.Ordinal)))
			{
				if (referenceCard.DynamicVars.TryGetValue(pair.Key, out DynamicVar? referenceVar) && referenceVar != null)
				{
					pair.Value.BaseValue = ConvertByDrawResult(card, referenceVar.BaseValue, result);
				}
			}

			card.DynamicVars.RecalculateForUpgradeOrEnchant();
		}

		private static async Task ApplyMiracleKeyword(PlayerChoiceContext choiceContext, CardModel card)
		{
			if (card.Owner.RunState.Rng.CombatEnergyCosts.NextFloat() < 0.2f||card.Owner.HasPower<AlwaysGoodLuckPower>())
			{
				ToolBox.PlayMiracleVfx(card.Owner);
				await PlayerCmd.GainEnergy(card.EnergyCost.Canonical, card.Owner);
			}
		}

		private static void ApplyVarIfPresent(CardModel card, DynamicVarSet currentVars, DynamicVarSet referenceVars, string key, DrawResultType result)
		{
			if (currentVars.TryGetValue(key, out DynamicVar? currentVar) && currentVar != null
				&& referenceVars.TryGetValue(key, out DynamicVar? referenceVar) && referenceVar != null)
			{
				currentVar.BaseValue = ConvertByDrawResult(card, referenceVar.BaseValue, result);
			}
		}

		private static decimal ConvertByDrawResult(CardModel card, decimal baseValue, DrawResultType result)
		{
			return result switch
			{
				DrawResultType.GreatLuck => baseValue * 2m,
				DrawResultType.Luck => baseValue,
				DrawResultType.BadLuck => decimal.Floor(baseValue * 0.5m),
				DrawResultType.GreatCurse => card is OmikujiBomb ? baseValue * 2m : 0m,
				_ => baseValue
			};
		}

		private static DrawResultType RollDrawResult(CardModel card)
		{
			return ToolBox.RollOmikuji(card.Owner.Creature) switch
			{
				"大吉" => DrawResultType.GreatLuck,
				"吉" => DrawResultType.Luck,
				"凶" => DrawResultType.BadLuck,
				_ => DrawResultType.GreatCurse
			};
		}

		internal static CardModel CreateReferenceCard(CardModel card)
		{
			CardModel referenceCard = ModelDb.GetById<CardModel>(card.Id).ToMutable();
			for (int level = 0; level < card.CurrentUpgradeLevel; level++)
			{
				referenceCard.UpgradeInternal();
				referenceCard.FinalizeUpgradeInternal();
			}
			return referenceCard;
		}

		internal static DrawResultType InferDrawResult(CardModel card)
		{
			CardModel referenceCard = CreateReferenceCard(card);
			DrawResultType? inferredResult = null;

			foreach ((DynamicVar currentVar, DynamicVar referenceVar) in GetTrackedVars(card, referenceCard))
			{
				if (referenceVar.BaseValue <= 0m)
				{
					continue;
				}

				DrawResultType? currentResult = GetResultFromValues(currentVar.BaseValue, referenceVar.BaseValue);
				if (currentResult == null)
				{
					continue;
				}

				if (inferredResult == null)
				{
					inferredResult = currentResult.Value;
				}
				else if (inferredResult != currentResult.Value)
				{
					return DrawResultType.Luck;
				}
			}

			return inferredResult ?? DrawResultType.Luck;
		}

		internal static DrawResultType GetResolvedDrawResult(CardModel card)
		{
			return DrawResults.TryGetValue(card, out DrawResultHolder? holder) ? holder.Result : InferDrawResult(card);
		}

		private static IEnumerable<(DynamicVar currentVar, DynamicVar referenceVar)> GetTrackedVars(CardModel card, CardModel referenceCard)
		{
			string[] directKeys = ["Cards", "Damage", "Block"];
			foreach (string key in directKeys)
			{
				if (card.DynamicVars.TryGetValue(key, out DynamicVar? currentVar) && currentVar != null
					&& referenceCard.DynamicVars.TryGetValue(key, out DynamicVar? referenceVar) && referenceVar != null)
				{
					yield return (currentVar, referenceVar);
				}
			}

			foreach (KeyValuePair<string, DynamicVar> pair in card.DynamicVars.Where(static pair => pair.Key.EndsWith("Power", StringComparison.Ordinal)))
			{
				if (referenceCard.DynamicVars.TryGetValue(pair.Key, out DynamicVar? referenceVar) && referenceVar != null)
				{
					yield return (pair.Value, referenceVar);
				}
			}
		}

		private static DrawResultType? GetResultFromValues(decimal currentValue, decimal referenceValue)
		{
			if (currentValue == referenceValue * 2m)
			{
				return DrawResultType.GreatLuck;
			}

			if (currentValue == referenceValue)
			{
				return DrawResultType.Luck;
			}

			if (currentValue == decimal.Floor(referenceValue * 0.5m))
			{
				return DrawResultType.BadLuck;
			}

			if (currentValue == 0m)
			{
				return DrawResultType.GreatCurse;
			}

			return null;
		}
	}

	[HarmonyPatch(typeof(CardModel), "get_HoverTips")]
	public static class DrawKeywordHoverTipPatch
	{
		public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
		{
			__result = IHoverTip.RemoveDupes(__result.ToList()).ToList();
		}
	}

	[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile), [typeof(PileType), typeof(MegaCrit.Sts2.Core.Entities.Creatures.Creature)])]
	public static class DrawKeywordDescriptionPatch
	{
		public static void Postfix(CardModel __instance, ref string __result)
		{
			__result = DrawKeywordTextHelper.ReplaceDrawKeywordLine(__instance, __result);
		}
	}

	[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForUpgradePreview))]
	public static class DrawKeywordUpgradePreviewDescriptionPatch
	{
		public static void Postfix(CardModel __instance, ref string __result)
		{
			__result = DrawKeywordTextHelper.ReplaceDrawKeywordLine(__instance, __result);
		}
	}

	internal static class DrawKeywordTextHelper
	{
		internal static string ReplaceDrawKeywordLine(CardModel card, string description)
		{
			if (!card.Keywords.Contains(CardModifier.DrawKeyword) || !HasBeenDrawn(card))
			{
				return description;
			}

			return description.Replace(GetDefaultDrawKeywordText(), GetResolvedDrawKeywordText(card), StringComparison.Ordinal);
		}

		private static string GetResolvedDrawKeywordText(CardModel card)
		{
			return CardKeywordAfterDrawPatch.GetResolvedDrawResult(card) switch
			{
				DrawResultType.GreatLuck => "[gold]抽签·大吉[/gold]。",
				DrawResultType.Luck => "[green]抽签·吉[/green]。",
				DrawResultType.BadLuck => "[red]抽签·凶[/red]。",
				DrawResultType.GreatCurse => "[purple]抽签·大凶[/purple]。",
				_ => GetDefaultDrawKeywordText()
			};
		}

		private static string GetDefaultDrawKeywordText()
		{
			LocString period = ToolBox.L10NStatic("PERIOD", "card_keywords");
			string suffix = period.Exists() ? period.GetRawText() : "。";
			return $"[gold]{ToolBox.L10NStatic("TH_SANAE-DRAW.title", "card_keywords").GetFormattedText()}[/gold]{suffix}";
		}

		private static bool HasBeenDrawn(CardModel card)
		{
			return CombatManager.Instance.History.Entries.OfType<CardDrawnEntry>().Any(entry => entry.Card == card);
		}
	}
}
