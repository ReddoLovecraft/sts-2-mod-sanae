using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace TH_Sanae.Scrpits.Cards
{
    public class CardModifier 
    {
        [CustomEnum("DRAW")]
        [KeywordProperties(AutoKeywordPosition.After)]
        public static CardKeyword DrawKeyword;

        [CustomEnum("MIRACLE")]
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword MiracleKeyword;

        [CustomEnum("WIND_STEP")]
        [KeywordProperties(AutoKeywordPosition.After)]
        public static CardKeyword WindStepKeyword;
    }
}
