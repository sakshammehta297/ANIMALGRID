using UnityEngine;

namespace AnimalGrid.Core
{
    /// <summary>
    /// ScriptableObject definition for difficulty curve configuration.
    /// Controls how puzzle difficulty scales across levels.
    /// Create instances via Assets > Create > Animal Grid > Difficulty Config
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Animal Grid/Difficulty Config", order = 3)]
    public class DifficultyConfigSO : ScriptableObject
    {
        [Header("Base Settings")]
        [Tooltip("Starting grid size for first level")]
        public int baseGridSize = 5;
        
        [Tooltip("Maximum grid size allowed")]
        public int maxGridSize = 9;
        
        [Tooltip("Grid size increase interval (levels per size increase)")]
        public int levelsPerSizeIncrease = 5;
        
        [Header("Boss Level Settings")]
        [Tooltip("Grid size for boss levels")]
        public int bossGridSize = 6;
        
        [Tooltip("Levels between boss fights")]
        public int levelsBetweenBosses = 10;
        
        [Header("Generation Parameters")]
        [Tooltip("Maximum generation attempts before giving up")]
        public int maxGenerationAttempts = 2000;
        
        [Tooltip("Maximum repair iterations to fix multiple solutions")]
        public int maxRepairIterations = 100;
        
        [Header("Hint System")]
        [Tooltip("Starting hints per level")]
        public int startingHints = 3;
        
        [Tooltip("Maximum hints a player can hold")]
        public int maxHints = 10;
        
        [Tooltip("Cost in coins to buy additional hints")]
        public int hintPurchaseCost = 50;
        
        [Header("Scoring")]
        [Tooltip("Points for correct placement")]
        public int pointsPerCorrectPlacement = 10;
        
        [Tooltip("Bonus for completing without mistakes")]
        public int perfectBonus = 50;
        
        [Tooltip("Time bonus per second remaining")]
        public float timeBonusPerSecond = 0.5f;
        
        /// <summary>
        /// Calculate grid size for a given level number.
        /// </summary>
        public int GetGridSizeForLevel(int levelNumber, bool isBoss = false)
        {
            if (isBoss) return Mathf.Min(bossGridSize, maxGridSize);
            
            int sizeIncrease = (levelNumber - 1) / levelsPerSizeIncrease;
            int gridSize = baseGridSize + sizeIncrease;
            return Mathf.Min(gridSize, maxGridSize);
        }
        
        /// <summary>
        /// Check if a level is a boss level.
        /// </summary>
        public bool IsBossLevel(int levelNumber)
        {
            return levelNumber > 0 && levelNumber % levelsBetweenBosses == 0;
        }
        
        /// <summary>
        /// Calculate score for a completed level.
        /// </summary>
        public int CalculateScore(int placements, int mistakes, float timeRemaining, bool isPerfect)
        {
            int score = placements * pointsPerCorrectPlacement;
            if (isPerfect) score += perfectBonus;
            score += Mathf.RoundToInt(timeRemaining * timeBonusPerSecond);
            return Mathf.Max(0, score);
        }
    }
}
