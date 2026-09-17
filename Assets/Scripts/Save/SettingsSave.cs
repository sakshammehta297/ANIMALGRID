using UnityEngine;

namespace AnimalGrid.Save
{
    /// <summary>
    /// Local settings storage (spec 33): all toggles persist between sessions.
    /// </summary>
    public static class SettingsSave
    {
        public static bool SfxOn
        {
            get { return PlayerPrefs.GetInt("sfxOn", 1) == 1; }
            set { PlayerPrefs.SetInt("sfxOn", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool MusicOn
        {
            get { return PlayerPrefs.GetInt("musicOn", 1) == 1; }
            set { PlayerPrefs.SetInt("musicOn", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool VibrationOn
        {
            get { return PlayerPrefs.GetInt("vibrationOn", 1) == 1; }
            set { PlayerPrefs.SetInt("vibrationOn", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool AnimatedHome
        {
            get { return PlayerPrefs.GetInt("animatedHome", 1) == 1; }
            set { PlayerPrefs.SetInt("animatedHome", value ? 1 : 0); PlayerPrefs.Save(); }
        }
    }
}