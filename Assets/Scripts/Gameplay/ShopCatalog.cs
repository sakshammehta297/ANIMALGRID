using AnimalGrid.Save;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Single source of truth for shop prices/quantities, so the Home Shop
    /// screen (GameplayBootstrap.BuildShopOverlay) and the in-level
    /// "out of hints" / "out of lives" prompts (GameplayController.Overlays.cs)
    /// never disagree with each other. Tune the economy by changing the
    /// constants here — nothing else needs to change.
    /// </summary>
    public static class ShopCatalog
    {
        public const int HintPackHints = 3;
        public const int HintPackCost = 40;

        public const int HintValuePackHints = 8;
        public const int HintValuePackCost = 90;

        public const int ExtraLifeCost = 30;

        public static bool TryBuyHintPack()
        {
            if (!CoinSave.TrySpend(HintPackCost)) return false;
            HintBankSave.Add(HintPackHints);
            return true;
        }

        public static bool TryBuyHintValuePack()
        {
            if (!CoinSave.TrySpend(HintValuePackCost)) return false;
            HintBankSave.Add(HintValuePackHints);
            return true;
        }

        public static bool TryBuyExtraLife()
        {
            return CoinSave.TrySpend(ExtraLifeCost);
        }
    }
}
