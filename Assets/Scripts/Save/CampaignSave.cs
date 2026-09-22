using UnityEngine;
using AnimalGrid.Core;

namespace AnimalGrid.Save
{
    /// <summary>
    /// Offline-first campaign storage (spec 33): CampaignState as JSON + tutorial flag.
    /// One-time silent migration wipes the pre-worlds save keys.
    /// Includes checksum validation to detect tampering.
    /// </summary>
    public static class CampaignSave
    {
        private const string KeyState = "campaignStateV1";
        private const string KeyChecksum = "campaignStateChecksumV1";
        private const string KeyTutorial = "tutorialDoneV1";
        private const string KeyMigrated = "migratedV1";

        public static CampaignState Load()
        {
            MigrateOnce();
            string json = PlayerPrefs.GetString(KeyState, "");
            string storedChecksum = PlayerPrefs.GetString(KeyChecksum, "");
            
            if (string.IsNullOrEmpty(json)) return CampaignState.Fresh();
            
            // Validate checksum to detect tampering
            string computedChecksum = ComputeChecksum(json);
            if (!string.IsNullOrEmpty(storedChecksum) && computedChecksum != storedChecksum)
            {
                Debug.LogWarning($"[CampaignSave] Checksum mismatch detected. Save file may be corrupted or tampered. Resetting to fresh state.");
                return CampaignState.Fresh();
            }
            
            try
            {
                var state = JsonUtility.FromJson<CampaignState>(json);
                if (state == null)
                {
                    Debug.LogWarning($"[CampaignSave] Failed to parse save data, returning fresh state. JSON length: {json.Length}");
                    return CampaignState.Fresh();
                }
                return state;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CampaignSave] Exception while loading save: {ex.Message}");
                return CampaignState.Fresh();
            }
        }

        public static void Save(CampaignState state)
        {
            if (state == null)
            {
                Debug.LogError("[CampaignSave] Attempted to save null state");
                return;
            }
            try
            {
                string json = JsonUtility.ToJson(state);
                string checksum = ComputeChecksum(json);
                PlayerPrefs.SetString(KeyState, json);
                PlayerPrefs.SetString(KeyChecksum, checksum);
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CampaignSave] Exception while saving: {ex.Message}");
            }
        }

        private static string ComputeChecksum(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;
            
            // Simple but effective checksum using CRC32-like algorithm
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                char c = data[i];
                crc ^= c;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                        crc = (crc >> 1) ^ 0xEDB88320;
                    else
                        crc >>= 1;
                }
            }
            return (~crc).ToString("X8");
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