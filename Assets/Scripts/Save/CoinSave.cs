using UnityEngine;

namespace AnimalGrid.Save
{
    /// <summary>
    /// Local, offline-first coin currency (same PlayerPrefs pattern as
    /// CampaignSave/SettingsSave). Coins are earned on level clear
    /// (see GameplayController.Fx.cs Celebrate()) and spent in the Shop or
    /// the in-level "out of hints" / "out of lives" prompts (see
    /// ShopCatalog.cs for prices).
    /// </summary>
    public static class CoinSave
    {
        private const string KeyBalance = "coinBalanceV1";
        private const int StartingBalance = 60; // small welcome stash so the shop isn't empty on first run

        public static int Balance
        {
            get { return PlayerPrefs.GetInt(KeyBalance, StartingBalance); }
            private set { PlayerPrefs.SetInt(KeyBalance, Mathf.Max(0, value)); PlayerPrefs.Save(); }
        }

        public static void Add(int amount)
        {
            if (amount <= 0) return;
            Balance += amount;
        }

        /// <summary>
        /// Deducts `amount` and returns true if affordable; otherwise leaves
        /// the balance untouched and returns false.
        /// </summary>
        public static bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (Balance < amount) return false;
            Balance -= amount;
            return true;
        }
    }
}
