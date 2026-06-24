using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Patches
{
	[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard))]
	public static class SanaeHandCardGreenGlowUpdatePatch
	{
		private static readonly Color _greenGlow = new Color("45d72cff");

		public static void Postfix(NHandCardHolder __instance)
		{
			NCard? cardNode = __instance.CardNode;
			CardModel? cardModel = cardNode?.Model;
			if (cardNode == null || cardModel == null)
			{
				return;
			}

			if (!MegaCrit.Sts2.Core.Combat.CombatManager.Instance.IsInProgress)
			{
				return;
			}

			PlayerCombatState? playerCombatState = cardModel.Owner?.PlayerCombatState;
			bool inPlayPhase = playerCombatState != null && playerCombatState.Phase == PlayerTurnPhase.Play;
			if (!inPlayPhase)
			{
				return;
			}

			bool shouldGlowRed = cardModel.ShouldGlowRed;
			bool shouldGlowGold = cardModel.CanPlay() && cardModel.ShouldGlowGold;
			bool shouldGlowGreen = cardModel is SanaeCardModel sanaeCard && sanaeCard.ShouldGlowGreen;

			if (cardModel.CanPlay() || shouldGlowRed || shouldGlowGold || shouldGlowGreen)
			{
				cardNode.CardHighlight.AnimShow();
				cardNode.CardHighlight.Modulate = NCardHighlight.playableColor;
				if (shouldGlowRed)
				{
					cardNode.CardHighlight.Modulate = NCardHighlight.red;
				}
				else if (shouldGlowGold)
				{
					cardNode.CardHighlight.Modulate = NCardHighlight.gold;
				}
				else if (shouldGlowGreen)
				{
					cardNode.CardHighlight.Modulate = _greenGlow;
				}
			}
			else
			{
				cardNode.CardHighlight.AnimHide();
			}
		}
	}

	[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.Flash))]
	public static class SanaeHandCardGreenGlowFlashPatch
	{
		private static readonly Color _greenGlow = new Color("45d72cff");

		private static readonly AccessTools.FieldRef<NHandCardHolder, Control> _flash =
			AccessTools.FieldRefAccess<NHandCardHolder, Control>("_flash");

		private static readonly AccessTools.FieldRef<NHandCardHolder, Tween?> _flashTween =
			AccessTools.FieldRefAccess<NHandCardHolder, Tween?>("_flashTween");

		public static bool Prefix(NHandCardHolder __instance)
		{
			Control flash = _flash(__instance);
			if (!GodotObject.IsInstanceValid(flash))
			{
				return false;
			}

			NCard? cardNode = __instance.CardNode;
			CardModel? cardModel = cardNode?.Model;

			PlayerCombatState? playerCombatState = cardModel?.Owner?.PlayerCombatState;
			bool inPlayPhase = playerCombatState != null && playerCombatState.Phase == PlayerTurnPhase.Play;

			bool shouldGlowRed = inPlayPhase && cardModel != null && cardModel.ShouldGlowRed;
			bool shouldGlowGold = inPlayPhase && cardModel != null && cardModel.CanPlay() && cardModel.ShouldGlowGold;
			bool shouldGlowGreen = inPlayPhase && cardModel is SanaeCardModel sanaeCard && sanaeCard.ShouldGlowGreen;

			flash.Scale = Vector2.One;
			flash.Modulate = NCardHighlight.playableColor;
			if (shouldGlowGold)
			{
				flash.Modulate = NCardHighlight.gold;
			}
			else if (shouldGlowRed)
			{
				flash.Modulate = NCardHighlight.red;
			}
			else if (shouldGlowGreen)
			{
				flash.Modulate = _greenGlow;
			}

			_flashTween(__instance)?.Kill();
			Tween tween = __instance.CreateTween();
			_flashTween(__instance) = tween;
			tween.TweenProperty(flash, "modulate:a", 0.6, 0.15);
			tween.TweenProperty(flash, "modulate:a", 0, 0.3);
			return false;
		}
	}
}
