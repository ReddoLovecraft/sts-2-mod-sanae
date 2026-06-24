using System;
using System.Collections.Generic;
using System.Linq;
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
			CardModel referenceCard = CreateReferenceCard(card);

			ApplyVarIfPresent(card.DynamicVars, referenceCard.DynamicVars, "Cards", result);
			ApplyVarIfPresent(card.DynamicVars, referenceCard.DynamicVars, "Damage", result);
			ApplyVarIfPresent(card.DynamicVars, referenceCard.DynamicVars, "Block", result);

			foreach (KeyValuePair<string, DynamicVar> pair in card.DynamicVars.Where(static pair => pair.Key.EndsWith("Power", StringComparison.Ordinal)))
			{
				if (referenceCard.DynamicVars.TryGetValue(pair.Key, out DynamicVar? referenceVar) && referenceVar != null)
				{
					pair.Value.BaseValue = ConvertByDrawResult(referenceVar.BaseValue, result);
				}
			}

			card.DynamicVars.RecalculateForUpgradeOrEnchant();
		}

		private static async Task ApplyMiracleKeyword(PlayerChoiceContext choiceContext, CardModel card)
		{
			if (card.EnergyCost.Canonical <= 0)
			{
				return;
			}

			if (card.Owner.RunState.Rng.CombatEnergyCosts.NextFloat() < 0.2f)
			{
				await PlayerCmd.GainEnergy(card.EnergyCost.Canonical, card.Owner);
			}
		}

		private static void ApplyVarIfPresent(DynamicVarSet currentVars, DynamicVarSet referenceVars, string key, DrawResultType result)
		{
			if (currentVars.TryGetValue(key, out DynamicVar? currentVar) && currentVar != null
				&& referenceVars.TryGetValue(key, out DynamicVar? referenceVar) && referenceVar != null)
			{
				currentVar.BaseValue = ConvertByDrawResult(referenceVar.BaseValue, result);
			}
		}

		private static decimal ConvertByDrawResult(decimal baseValue, DrawResultType result)
		{
			return result switch
			{
				DrawResultType.GreatLuck => baseValue * 2m,
				DrawResultType.Luck => baseValue,
				DrawResultType.BadLuck => decimal.Floor(baseValue * 0.5m),
				DrawResultType.GreatCurse => 0m,
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
			List<IHoverTip> tips = __result.ToList();
			if (!__instance.Keywords.Contains(CardModifier.DrawKeyword))
			{
				__result = IHoverTip.RemoveDupes(tips).ToList();
				return;
			}

			string drawTipId = HoverTipFactory.FromKeyword(CardModifier.DrawKeyword).Id;
			int drawTipIndex = tips.FindIndex(static tip => !string.IsNullOrEmpty(tip.Id));
			drawTipIndex = tips.FindIndex(tip => tip.Id == drawTipId);
			if (drawTipIndex < 0)
			{
				return;
			}

			tips[drawTipIndex] = CreateDrawHoverTip(__instance);
			__result = IHoverTip.RemoveDupes(tips).ToList();
		}

		private static IHoverTip CreateDrawHoverTip(CardModel card)
		{
			LocString title = ToolBox.L10NStatic(GetDrawTitleKey(card), "card_keywords");
			LocString description = ToolBox.L10NStatic("TH_SANAE-DRAW.description", "card_keywords");
			return new HoverTip(title, description);
		}

		private static string GetDrawTitleKey(CardModel card)
		{
			if (!HasBeenDrawn(card))
			{
				return "TH_SANAE-DRAW.title";
			}

			return CardKeywordAfterDrawPatch.InferDrawResult(card) switch
			{
				DrawResultType.GreatLuck => "TH_SANAE-DRAW_GREAT_LUCK.title",
				DrawResultType.Luck => "TH_SANAE-DRAW_LUCK.title",
				DrawResultType.BadLuck => "TH_SANAE-DRAW_BAD_LUCK.title",
				DrawResultType.GreatCurse => "TH_SANAE-DRAW_GREAT_CURSE.title",
				_ => "TH_SANAE-DRAW.title"
			};
		}

		private static bool HasBeenDrawn(CardModel card)
		{
			return CombatManager.Instance.History.Entries.OfType<CardDrawnEntry>().Any(entry => entry.Card == card);
		}
	}
}
