using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
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

	internal interface IResolvedDrawResultCard
	{
		DrawResultType? ResolvedDrawResult { get; set; }
	}

	[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardDrawn))]
	public static class CardKeywordAfterDrawPatch
	{
		private sealed class DrawResultHolder
		{
			public DrawResultType Result;
		}

		private static ConditionalWeakTable<CardModel, DrawResultHolder> DrawResults = new();
		private static readonly Dictionary<uint, DrawResultType> MultiplayerDrawResults = new();

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
			StoreResolvedDrawResult(card, result);
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
			RefreshDrawKeywordCardVisuals(card);
		}

		private static void RefreshDrawKeywordCardVisuals(CardModel card)
		{
			PileType? pileType = card.Pile?.Type;
			NCard? nCard = pileType != null ? NCard.FindOnTable(card, pileType) : null;
			if (nCard == null && TryGetTrackedCardId(ResolveTrackedCard(card), out uint trackedId))
			{
				NCombatUi? ui = NCombatRoom.Instance?.Ui;
				if (ui != null)
				{
					nCard = FindCardNodeByTrackedId(ui, trackedId);
				}
			}

			if (nCard == null)
			{
				return;
			}

			PileType displayingPile = nCard.DisplayingPile;
			CardModel? model = nCard.Model;
			MegaCrit.Sts2.Core.Entities.UI.ModelVisibility visibility = nCard.Visibility;
			nCard.Model = null;
			nCard.Visibility = visibility;
			nCard.Model = model ?? card;
			nCard.UpdateVisuals(displayingPile, CardPreviewMode.Normal);
		}

		private static NCard? FindCardNodeByTrackedId(NCombatUi ui, uint trackedId)
		{
			foreach (NHandCardHolder holder in ui.Hand.ActiveHolders)
			{
				NCard? node = holder.CardNode;
				CardModel? model = node?.Model;
				if (model != null && TryGetTrackedCardId(ResolveTrackedCard(model), out uint id) && id == trackedId)
				{
					return node;
				}
			}

			foreach (NCard node in ui.PlayContainer.GetChildren().OfType<NCard>())
			{
				CardModel? model = node.Model;
				if (model != null && TryGetTrackedCardId(ResolveTrackedCard(model), out uint id) && id == trackedId)
				{
					return node;
				}
			}

			foreach (NCard node in ui.PlayQueue.GetChildren().OfType<NCard>())
			{
				CardModel? model = node.Model;
				if (model != null && TryGetTrackedCardId(ResolveTrackedCard(model), out uint id) && id == trackedId)
				{
					return node;
				}
			}

			return null;
		}

		private static async Task ApplyMiracleKeyword(PlayerChoiceContext choiceContext, CardModel card)
		{
			await MiracleHelper.TryTriggerMiracle(choiceContext, card);
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
			return TryGetStoredDrawResult(card, out DrawResultType result) ? result : InferDrawResult(card);
		}

		internal static bool HasStoredDrawResult(CardModel card)
		{
			return TryGetStoredDrawResult(card, out _);
		}

		internal static void ClearStoredDrawResults()
		{
			DrawResults = new ConditionalWeakTable<CardModel, DrawResultHolder>();
			MultiplayerDrawResults.Clear();
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

		internal static void StoreResolvedDrawResult(CardModel card, DrawResultType result)
		{
			CardModel resolvedCard = ResolveTrackedCard(card);
			StoreResolvedDrawResultOnCard(card, result);
			if (!ReferenceEquals(resolvedCard, card))
			{
				StoreResolvedDrawResultOnCard(resolvedCard, result);
			}

			DrawResults.Remove(resolvedCard);
			DrawResults.Add(resolvedCard, new DrawResultHolder { Result = result });

			if (TryGetTrackedCardId(card, out uint cardId) || TryGetTrackedCardId(resolvedCard, out cardId))
			{
				MultiplayerDrawResults[cardId] = result;
			}
		}

		private static bool TryGetStoredDrawResult(CardModel card, out DrawResultType result)
		{
			CardModel resolvedCard = ResolveTrackedCard(card);
			if (TryGetStoredDrawResultFromCard(card, out result) || (!ReferenceEquals(resolvedCard, card) && TryGetStoredDrawResultFromCard(resolvedCard, out result)))
			{
				return true;
			}

			if (DrawResults.TryGetValue(resolvedCard, out DrawResultHolder? holder))
			{
				result = holder.Result;
				return true;
			}

			if ((TryGetTrackedCardId(card, out uint cardId) || TryGetTrackedCardId(resolvedCard, out cardId))
				&& MultiplayerDrawResults.TryGetValue(cardId, out result))
			{
				return true;
			}

			result = default;
			return false;
		}

		private static void StoreResolvedDrawResultOnCard(CardModel card, DrawResultType result)
		{
			if (card is IResolvedDrawResultCard resolvedDrawResultCard)
			{
				resolvedDrawResultCard.ResolvedDrawResult = result;
			}
		}

		private static bool TryGetStoredDrawResultFromCard(CardModel card, out DrawResultType result)
		{
			if (card is IResolvedDrawResultCard { ResolvedDrawResult: DrawResultType resolvedResult })
			{
				result = resolvedResult;
				return true;
			}

			result = default;
			return false;
		}

		internal static CardModel ResolveTrackedCard(CardModel card)
		{
			HashSet<CardModel> visited = new(ReferenceEqualityComparer.Instance);
			CardModel current = card;
			while (current.CloneOf != null && visited.Add(current))
			{
				current = current.CloneOf;
			}

			return current;
		}

		internal static bool TryGetTrackedCardId(CardModel card, out uint cardId)
		{
			if (card.IsInCombat && NetCombatCardDb.Instance.TryGetCardId(card, out cardId))
			{
				return true;
			}

			cardId = 0;
			return false;
		}

		private sealed class ReferenceEqualityComparer : IEqualityComparer<CardModel>
		{
			internal static readonly ReferenceEqualityComparer Instance = new();

			public bool Equals(CardModel? x, CardModel? y)
			{
				return ReferenceEquals(x, y);
			}

			public int GetHashCode(CardModel obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}
	}

	[HarmonyPatch(typeof(NetCombatCardDb), nameof(NetCombatCardDb.StartCombat))]
	public static class DrawKeywordNetCombatCardDbStartPatch
	{
		public static void Prefix()
		{
			CardKeywordAfterDrawPatch.ClearStoredDrawResults();
		}
	}

	[HarmonyPatch(typeof(NetCombatCardDb), "OnCombatEnded")]
	public static class DrawKeywordNetCombatCardDbEndPatch
	{
		public static void Prefix()
		{
			CardKeywordAfterDrawPatch.ClearStoredDrawResults();
		}
	}

	[HarmonyPatch(typeof(NetCombatCardDb), nameof(NetCombatCardDb.ClearCardsForTesting))]
	public static class DrawKeywordNetCombatCardDbClearPatch
	{
		public static void Prefix()
		{
			CardKeywordAfterDrawPatch.ClearStoredDrawResults();
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
	[HarmonyPriority(Priority.Last)]
	public static class DrawKeywordDescriptionPatch
	{
		public static void Postfix(CardModel __instance, PileType pileType, MegaCrit.Sts2.Core.Entities.Creatures.Creature? target, ref string __result)
		{
			__result = DrawKeywordTextHelper.ReplaceDrawKeywordLine(__instance, __result, pileType);
		}
	}

	[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForUpgradePreview))]
	[HarmonyPriority(Priority.Last)]
	public static class DrawKeywordUpgradePreviewDescriptionPatch
	{
		public static void Postfix(CardModel __instance, ref string __result)
		{
			__result = DrawKeywordTextHelper.ReplaceDrawKeywordLine(__instance, __result, PileType.None);
		}
	}

	[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
	[HarmonyPriority(Priority.Last)]
	public static class DrawKeywordNCardUpdateVisualsPatch
	{
		public static void Postfix(NCard __instance, PileType pileType, CardPreviewMode previewMode)
		{
			if (__instance.Visibility != ModelVisibility.Visible)
			{
				return;
			}

			CardModel? model = __instance.Model;
			if (model == null || !model.Keywords.Contains(CardModifier.DrawKeyword))
			{
				return;
			}

			MegaRichTextLabel label = __instance.GetNode<MegaRichTextLabel>("%DescriptionLabel");
			string current = label.Text;
			string replaced = DrawKeywordTextHelper.ReplaceDrawKeywordLine(model, current, pileType);
			if (!string.Equals(current, replaced, StringComparison.Ordinal))
			{
				label.SetTextAutoSize(replaced);
			}
		}
	}

	internal static class DrawKeywordTextHelper
	{
		internal static string ReplaceDrawKeywordLine(CardModel card, string description, PileType displayingPile)
		{
			if (!card.Keywords.Contains(CardModifier.DrawKeyword) || !ShouldReplaceDrawKeywordLine(card, displayingPile))
			{
				return description;
			}

			string replaced = description.Replace(GetDefaultDrawKeywordText(), GetResolvedDrawKeywordText(card), StringComparison.Ordinal);
			return replaced.Replace(GetDefaultDrawKeywordTag(), GetResolvedDrawKeywordTag(card), StringComparison.Ordinal);
		}

		private static string GetResolvedDrawKeywordText(CardModel card)
		{
			LocString period = ToolBox.L10NStatic("PERIOD", "card_keywords");
			string suffix = period.Exists() ? period.GetRawText() : "。";
			return $"{GetResolvedDrawKeywordTag(card)}{suffix}";
		}

		private static string GetResolvedDrawKeywordTag(CardModel card)
		{
			(string color, string key) = CardKeywordAfterDrawPatch.GetResolvedDrawResult(card) switch
			{
				DrawResultType.GreatLuck => ("gold", "TH_SANAE-DRAW_GREAT_LUCK.title"),
				DrawResultType.Luck => ("green", "TH_SANAE-DRAW_LUCK.title"),
				DrawResultType.BadLuck => ("red", "TH_SANAE-DRAW_BAD_LUCK.title"),
				DrawResultType.GreatCurse => ("purple", "TH_SANAE-DRAW_GREAT_CURSE.title"),
				_ => ("gold", "TH_SANAE-DRAW.title")
			};
			string title = ToolBox.L10NStatic(key, "card_keywords").GetFormattedText();
			return $"[{color}]{title}[/{color}]";
		}

		private static string GetDefaultDrawKeywordText()
		{
			LocString period = ToolBox.L10NStatic("PERIOD", "card_keywords");
			string suffix = period.Exists() ? period.GetRawText() : "。";
			return $"{GetDefaultDrawKeywordTag()}{suffix}";
		}

		private static string GetDefaultDrawKeywordTag()
		{
			return $"[gold]{ToolBox.L10NStatic("TH_SANAE-DRAW.title", "card_keywords").GetFormattedText()}[/gold]";
		}

		private static bool HasBeenDrawn(CardModel card)
		{
			return CombatManager.Instance.History.Entries
				.OfType<CardDrawnEntry>()
				.Any(entry => IsSameTrackedCard(entry.Card, card));
		}

		private static bool HasResolvedDrawKeyword(CardModel card)
		{
			return CardKeywordAfterDrawPatch.HasStoredDrawResult(card) || HasBeenDrawn(card);
		}

		private static bool ShouldReplaceDrawKeywordLine(CardModel card, PileType displayingPile)
		{
			if (HasResolvedDrawKeyword(card))
			{
				return true;
			}

			if (!card.IsMutable || card.CombatState == null)
			{
				return false;
			}

			return displayingPile is PileType.Hand or PileType.Play;
		}

		private static bool IsSameTrackedCard(CardModel left, CardModel right)
		{
			if (ReferenceEquals(left, right))
			{
				return true;
			}

			CardModel resolvedLeft = CardKeywordAfterDrawPatch.ResolveTrackedCard(left);
			CardModel resolvedRight = CardKeywordAfterDrawPatch.ResolveTrackedCard(right);
			if (ReferenceEquals(resolvedLeft, resolvedRight))
			{
				return true;
			}

			return CardKeywordAfterDrawPatch.TryGetTrackedCardId(resolvedLeft, out uint leftId)
				&& CardKeywordAfterDrawPatch.TryGetTrackedCardId(resolvedRight, out uint rightId)
				&& leftId == rightId;
		}
	}
}
