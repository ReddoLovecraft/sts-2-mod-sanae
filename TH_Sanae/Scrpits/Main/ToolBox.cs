using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scrpits.Cards;
using TH_Sanae.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
namespace TH_Sanae.Scripts.Main
{
	public static class ToolBox
	{
        private static HashSet<string>? _windRelatedTipIds;

        private static List<Type>? _sanaeSpellCardTypes;
        private static List<Type>? _kanakoCardTypes;
        private static readonly HashSet<string> _kanakoCardTypeNames =
        [
            nameof(ArmyGodForm),
            nameof(BoilingBlood),
            nameof(ExtendOnbashria),
            nameof(FaithCall),
            nameof(GodPowerLight),
            nameof(KanakoOnbashiraSummon),
            nameof(KanakoSummonOnbashira),
            nameof(KanakoSummonRush),
            nameof(KanakoSummonWind),
            nameof(MartialTraining),
            nameof(TechInnovation),
            nameof(WindGodVirtue)
        ];
        public static void playWindSfx(float specialNum,Color? color = null)
        {
            Color actualColor = color ?? new Color("FFFFFF80");
			double num2 = ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2 : 0.3);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(actualColor, 0.8 + specialNum * num2));
			SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");
			NRun.Instance?.GlobalUi.AddChildSafely(NSmokyVignetteVfx.Create(actualColor, actualColor));
        }
        public static void PlayMiracleVfx(Player? player)
        {
            if (player?.Creature == null)
            {
                return;
            }

            PlayMiracleVfx(player.Creature,StsColors.gold);
        }
        public static void PlayMiracleVfx(Creature? target,Color borderTint,bool hasColor=false)
        {
            if (target == null)
            {
                return;
            }
            if(!hasColor)
                borderTint = StsColors.gold;
            borderTint.A = 0.28f;
            Color highlightTint = new Color(1f, 0.6f, 0.2f, 0.18f);
            SfxCmd.Play(SanaeInit.ToModSfxPath("TH_Sanae/ArtWorks/SFX/miracle.wav"));
            NRun.Instance?.GlobalUi.AddChildSafely(NSmokyVignetteVfx.Create(borderTint, highlightTint));
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NMiracleVfx.Create(target));
        }
        public static bool IsDevotee(Creature creature)
        {
            return creature.HasPower<BeliefPower>() && creature.GetPower<BeliefPower>().Amount >= 10;
        }
        public static bool IsWindControl(Creature owner,int line)
        {
            if(!owner.HasPower<WindPower>())
                return false;
            if(owner.GetPower<WindPower>().Amount<line)
                return false;
            return true;
        }
        public static bool IsPiety(Creature owner,int line)
        {
            if(!owner.HasPower<BeliefPower>())
                return false;
            if(owner.GetPower<BeliefPower>().Amount<line)
                return false;
            return true;
        }
        public static async Task Persuasion(Creature player,Creature target)
        {
            if(target==null||!target.HasPower<BeliefPower>())
            return;
            if(target.GetPowerAmount<BeliefPower>() * 2 >= target.CurrentHp)
            {
                AbstractRoom currentRoom = player.CombatState.RunState.CurrentRoom;
                //投降逻辑
                SfxCmd.Play("event:/sfx/enemy/enemy_attacks/gremlin_merc/fat_gremlin_escape");
                Tools.Talk("[green]起信仰了，润了润了！[/green]",target);
                await CreatureCmd.Escape(target);
		        if (currentRoom is CombatRoom combatRoom)
		        { 
                    Rng rng = player.Player.RunState.Rng.CombatCardGeneration;
                    int randomNumber = rng.NextInt(1, 10);
                    int goldAmt=25;
                    if(randomNumber<=6)
                    {
                        if(randomNumber<=2)
                        {
                            combatRoom.AddExtraReward(player.Player, new RelicReward(player.Player));
                        }
                        if(combatRoom.RoomType==RoomType.Monster)
                        goldAmt+=rng.NextInt(10,20);
                        else if(combatRoom.RoomType==RoomType.Elite)
                        goldAmt+=rng.NextInt(30,50);
                        else if(combatRoom.RoomType==RoomType.Boss)
                        goldAmt+=rng.NextInt(50,100);
                    }
                    else
                    {
                        if(randomNumber<=8)
                        {
                            combatRoom.AddExtraReward(player.Player, new RelicReward(player.Player));
                        }
                        combatRoom.AddExtraReward(player.Player, new CardReward(CardCreationOptions.ForRoom(player.Player, combatRoom.RoomType), 3, player.Player));
                    }
                    combatRoom.AddExtraReward(player.Player, new GoldReward(goldAmt,player.Player));
                }
            }
        }
        public static async Task StopWind(PlayerChoiceContext context,Creature owner)
        {
            if(owner==null)
                return;
            if(owner.HasPower<WindPower>())
                await PowerCmd.Remove<WindPower>(owner);
            if(owner.HasPower<WindStatePower>())
                await PowerCmd.Remove<WindStatePower>(owner);
        }
        public static async Task SummonWind(PlayerChoiceContext context,Creature summoner)
        {
            if(summoner==null)
                return;
            if(summoner.HasPower<WindStatePower>())
            {
                await DoubleWind(context,summoner);
            }
            else
            {
                await PowerCmd.Apply<WindStatePower>(context,summoner,1,null,null);
            }
        }
        public static async Task DoubleWind(PlayerChoiceContext context,Creature owner)
        {
           if(owner==null||!owner.HasPower<WindPower>())
                return;
           int amt=owner.GetPower<WindPower>().Amount;
           await PowerCmd.Apply<WindPower>(context,owner,amt,null,null);
        }

        public static async Task ReturnBerserkWindFromDiscardToHand(Player player)
        {
            CardPile? discardPile = player.Piles.FirstOrDefault(pile => pile.Type == PileType.Discard);
            if (discardPile == null || discardPile.IsEmpty)
            {
                return;
            }

            foreach (BerserkWind card in discardPile.Cards.OfType<BerserkWind>().ToList())
            {
                await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Random, card, false);
            }
        }

        public static IReadOnlyList<CardModel> GetAllCombatCards(Player player)
        {
            return player.Piles.SelectMany(pile => pile.Cards).ToList();
        }

        public static bool IsWindRelatedCard(CardModel card)
        {
            return card.HoverTips.Any(tip => GetWindRelatedTipIds().Contains(tip.Id));
        }

        public static CardModel? CreateRandomSanaeSpellCard(Player player)
        {
            List<Type> spellCardTypes = GetSanaeSpellCardTypes();
            if (spellCardTypes.Count == 0)
            {
                return null;
            }

            int index = player.RunState.Rng.CombatCardGeneration.NextInt(spellCardTypes.Count);
            Type cardType = spellCardTypes[index];
            if (player.Creature.CombatState == null)
            {
                return null;
            }

            CardModel? canonicalCard = GetCanonicalCard(cardType);
            if (canonicalCard == null)
            {
                return null;
            }

            return player.Creature.CombatState.CreateCard(canonicalCard, player);
        }

        public static CardModel? CreateRandomKanakoCard(Player player, bool upgraded = false)
        {
            List<Type> kanakoCardTypes = GetKanakoCardTypes();
            if (kanakoCardTypes.Count == 0)
            {
                return null;
            }

            Type cardType = kanakoCardTypes[player.RunState.Rng.CombatCardGeneration.NextInt(kanakoCardTypes.Count)];
            if (player.Creature.CombatState == null)
            {
                return null;
            }

            CardModel? canonicalCard = GetCanonicalCard(cardType);
            if (canonicalCard == null)
            {
                return null;
            }

            CardModel card = player.Creature.CombatState.CreateCard(canonicalCard, player);
            if (upgraded)
            {
                while (card.CurrentUpgradeLevel < card.MaxUpgradeLevel)
                {
                    UpgradeCard(card);
                }
            }
            return card;
        }

        public static bool HasAlwaysGoodLuck(Creature creature)
        {
            return creature.HasPower<AlwaysGoodLuckPower>();
        }

        public static bool RollMiracle(Creature creature)
        {
            if (HasAlwaysGoodLuck(creature))
            {
                return true;
            }

            return creature.Player?.RunState.Rng.CombatCardGeneration.NextInt(1, 101) <= 20;
        }

        public static string RollOmikuji(Creature creature)
        {
            if (HasAlwaysGoodLuck(creature))
            {
                return "大吉";
            }

            int num = creature.Player?.RunState.Rng.CombatCardGeneration.NextInt(1, 102) ?? 101;
            if (num <= 20)
            {
                return "大吉";
            }
            if (num <= 50)
            {
                return "吉";
            }
            if (num <= 80)
            {
                return "凶";
            }
            return "大凶";
        }

        public static string ApplyOmikuji(CardModel card)
        {
            if (!card.IsMutable || card.Owner == null)
            {
                return "吉";
            }

            string result = RollOmikuji(card.Owner.Creature);
            ApplyOmikujiResult(card, result);
            return result;
        }

        public static void ApplyOmikujiResult(CardModel card, string result)
        {
            if (!card.IsMutable)
            {
                return;
            }

            if (card.EnergyCost.CostsX || card.EnergyCost.Canonical < 0)
            {
                return;
            }

            int currentCost = card.EnergyCost.GetWithModifiers(CostModifiers.All);
            switch (result)
            {
                case "大吉":
                    card.EnergyCost.SetUntilPlayed(0);
                    break;
                case "吉":
                    if (currentCost > 0)
                    {
                        card.EnergyCost.AddUntilPlayed(-1);
                    }
                    break;
                case "凶":
                    card.EnergyCost.AddUntilPlayed(1);
                    break;
                case "大凶":
                    if (currentCost > 0)
                    {
                        card.EnergyCost.AddUntilPlayed(currentCost);
                    }
                    break;
            }

            RefreshCardVisuals(card);
        }

        public static void RefreshCardVisuals(CardModel card)
        {
            PileType? pileType = card.Pile?.Type;
            if (pileType == null)
            {
                return;
            }

            NCard? nCard = NCard.FindOnTable(card, pileType);
            if (nCard == null)
            {
                return;
            }

            MegaCrit.Sts2.Core.Entities.UI.ModelVisibility visibility = nCard.Visibility;
            nCard.Model = null;
            nCard.Visibility = visibility;
            nCard.Model = card;
            nCard.UpdateVisuals(pileType.Value, CardPreviewMode.Normal);
        }

        public static CardPile? GetPile(Player player, PileType pileType)
        {
            return player.Piles.FirstOrDefault(pile => pile.Type == pileType);
        }

        public static void UpgradeCard(CardModel card, CardPreviewStyle style = CardPreviewStyle.None)
        {
            UpgradeCards([card], style);
        }

        public static void UpgradeCards(IEnumerable<CardModel> cards, CardPreviewStyle style = CardPreviewStyle.None)
        {
            List<CardModel> upgradableCards = cards
                .Where(card => card.IsUpgradable)
                .Distinct()
                .ToList();
            if (upgradableCards.Count == 0)
            {
                return;
            }

            CardCmd.Upgrade(upgradableCards, style);
            foreach (CardModel card in upgradableCards.Where(card => card.Pile != null))
            {
                RefreshCardVisuals(card);
            }
            PlaySmithUpgradeVfx(upgradableCards);
        }

        public static async Task UpgradeCardsInHand(Player player)
        {
            CardPile? hand = GetPile(player, PileType.Hand);
            if (hand == null)
            {
                return;
            }

            UpgradeCards(hand.Cards.ToList());
        }

        private static HashSet<string> GetWindRelatedTipIds()
        {
            _windRelatedTipIds ??=
            [
                HoverTipFactory.FromPower<WindPower>().Id,
                Tools.GetStaticKeyword("WindState").Id,
                Tools.GetStaticKeyword("StopWind").Id,
                Tools.GetStaticKeyword("WindSummon").Id,
                Tools.GetStaticKeyword("WindControl").Id
            ];

            return _windRelatedTipIds;
        }

        private static CardModel? GetCanonicalCard(Type cardType)
        {
            if (!typeof(CardModel).IsAssignableFrom(cardType))
            {
                return null;
            }

            return ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
        }

        private static void PlaySmithUpgradeVfx(IReadOnlyCollection<CardModel> cards)
        {
            if (cards.Count == 0 || !LocalContext.IsMine(cards.First()))
            {
                return;
            }

            List<CardModel> visibleCards = cards
                .Where(card => card.Pile != null)
                .ToList();
            if (visibleCards.Count == 0)
            {
                return;
            }

            List<CardModel> handCards = visibleCards
                .Where(card => card.Pile!.Type == PileType.Hand)
                .ToList();
            List<CardModel> otherCards = visibleCards
                .Where(card => card.Pile!.Type != PileType.Hand)
                .ToList();

            bool playedSfx = false;
            foreach (CardModel handCard in handCards)
            {
                var nCard = NCombatRoom.Instance?.Ui.Hand.GetCard(handCard);
                if (nCard == null)
                {
                    otherCards.Add(handCard);
                    continue;
                }

                NCardSmithVfx? vfx = NCardSmithVfx.Create(nCard, playSfx: !playedSfx);
                if (vfx == null)
                {
                    continue;
                }

                NRun.Instance?.GlobalUi.AboveTopBarVfxContainer.AddChildSafely(vfx);
                playedSfx = true;
            }

            if (otherCards.Count == 0)
            {
                return;
            }

            NCardSmithVfx? previewVfx = NCardSmithVfx.Create(otherCards, playSfx: !playedSfx);
            if (previewVfx == null)
            {
                return;
            }

            NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(previewVfx);
        }

        private static List<Type> GetSanaeSpellCardTypes()
        {
            _sanaeSpellCardTypes ??= typeof(SanaeCharacter).Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract
                    && typeof(YCCardModel).IsAssignableFrom(type)
                    && type.Namespace == "TH_Sanae.Scrpits.Cards")
                .OrderBy(type => type.Name)
                .ToList();

            return _sanaeSpellCardTypes;
        }

        private static List<Type> GetKanakoCardTypes()
        {
            _kanakoCardTypes ??= typeof(SanaeCharacter).Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract
                    && typeof(SanaeCardModel).IsAssignableFrom(type)
                    && type.Namespace == "TH_Sanae.Scrpits.Cards"
                    && _kanakoCardTypeNames.Contains(type.Name))
                .OrderBy(type => type.Name)
                .ToList();

            return _kanakoCardTypes;
        }

        public static int GetDebuffTotalCount(Creature target) 
        {
            int result = 0;
            foreach(PowerModel debuff in target.Powers) 
            {
                if(debuff.Type==PowerType.Debuff) 
                {
                    if (debuff.Amount > 0)
                        result += debuff.Amount;
                    else
                        result++;
                }
            }
            return result;

        }
        public static int GetDebuffKind(Creature target)
        {
            int result = 0;
            foreach (PowerModel debuff in target.Powers)
            {
                if (debuff.Type == PowerType.Debuff)
                {
                        result++;
                }
            }
            return result;
        }
             public static LocString L10NStatic(string entry,string targetTable="static_hover_tips")
            {
                return new LocString(targetTable, entry);
            }
            public static LocString GetCustomText(string targetTable,string entry,string postfix)
            {
                string text = StringHelper.Slugify(entry);
                LocString res = L10NStatic(text + postfix, targetTable);
                if (!res.Exists())
                {
                    res = L10NStatic(entry + postfix, targetTable);
                }
                return res;
            }
           
	}
}
