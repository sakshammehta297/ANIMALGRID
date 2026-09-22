using UnityEngine;

namespace AnimalGrid.Save
{
    /// <summary>
    /// Persistent bonus-hint stockpile, separate from the 3 free hints every
    /// level starts with (that counter, `hintsLeft`, lives on
    /// GameplayController and resets each level as before). Bonus hints are
    /// bought in the Shop (or the in-level "out of hints" prompt) with coins,
    /// then drawn down automatically once a level's free hints run out.
    /// </summary>
    public static class HintBankSave
    {
        private const string KeyBonusHints = "bonusHintsV1";

        public static int BonusHints
        {
            get { return PlayerPrefs.GetInt(KeyBonusHints, 0); }
            private set { PlayerPrefs.SetInt(KeyBonusHints, Mathf.Max(0, value)); PlayerPrefs.Save(); }
        }

        public static void Add(int amount)
        {
            if (amount <= 0) return;
            BonusHints += amount;
        }

        /// <summary>Consumes one bonus hint if any are banked; returns whether one was consumed.</summary>
        public static bool TryConsumeOne()
        {
            if (BonusHints <= 0) return false;
            BonusHints -= 1;
            return true;
        }
    }
}
