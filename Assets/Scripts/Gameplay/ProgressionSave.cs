using UnityEngine;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Local, offline-first progress storage (spec 33).
    /// </summary>
    public static class ProgressionSave
    {
        private const string KeyLevel = "currentLevel";
        private const string KeyTutorial = "tutorialDone";

        public static int CurrentLevel
        {
            get { return PlayerPrefs.GetInt(KeyLevel, 1); }
            set { PlayerPrefs.SetInt(KeyLevel, value); PlayerPrefs.Save(); }
        }

        public static bool TutorialDone
        {
            get { return PlayerPrefs.GetInt(KeyTutorial, 0) == 1; }
            set { PlayerPrefs.SetInt(KeyTutorial, value ? 1 : 0); PlayerPrefs.Save(); }
        }
    }
}