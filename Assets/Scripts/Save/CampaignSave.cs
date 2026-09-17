using UnityEngine;
using AnimalGrid.Core;

namespace AnimalGrid.Save
{
    /// <summary>
    /// Offline-first campaign storage (spec 33): CampaignState as JSON + tutorial flag.
    /// One-time silent migration wipes the pre-worlds save keys.
    /// </summary>
    public static class CampaignSave
    {
        private const string KeyState = "campaignStateV1";
        private const string KeyTutorial = "tutorialDoneV1";
        private const string KeyMigrated = "migratedV1";

        public static CampaignState Load()
        {
            MigrateOnce();
            string json = PlayerPrefs.GetString(KeyState, "");
            if (string.IsNullOrEmpty(json)) return CampaignState.Fresh();
            try
            {
                var state = JsonUtility.FromJson<CampaignState>(json);
                return state != null ? state : CampaignState.Fresh();
            }
            catch
            {
                return CampaignState.Fresh();
            }
        }

        public static void Save(CampaignState state)
        {
            PlayerPrefs.SetString(KeyState, JsonUtility.ToJson(state));
            PlayerPrefs.Save();
        }

        public static bool TutorialDone
        {
            get { return PlayerPrefs.GetInt(KeyTutorial, 0) == 1; }
            set { PlayerPrefs.SetInt(KeyTutorial, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        private static void MigrateOnce()
        {
            if (PlayerPrefs.GetInt(KeyMigrated, 0) == 1) return;
            PlayerPrefs.DeleteKey("currentLevel");
            PlayerPrefs.DeleteKey("tutorialDone");
            PlayerPrefs.SetInt(KeyMigrated, 1);
            PlayerPrefs.Save();
        }
    }
}