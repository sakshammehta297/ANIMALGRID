using System;
using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// What happened after clearing a level (drives popups & world flow).
    /// </summary>
    [Serializable]
    public class ClearResult
    {
        public List<int> newlyUnlocked = new List<int>(); // animal indices in the current world
        public bool worldCompleted;
        public bool gameCompleted;
        public int nextWorld;
        public int nextLevel;
    }

    /// <summary>
    /// The whole campaign: which world, which level, points per world, completion.
    /// Serializable so the save system can store it as JSON.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    [Serializable]
    public class CampaignState
    {
        public int worldIndex;
        public int levelInWorld = 1;
        public List<int> worldPoints = new List<int> { 0, 0, 0 };
        public bool gameCompleted;

        public static CampaignState Fresh()
        {
            return new CampaignState();
        }

        public int UnlockedInWorld(int world)
        {
            return UnlockProgression.UnlockedCount(worldPoints[world]);
        }

        public ClearResult RecordLevelClear(bool isBoss)
        {
            var result = new ClearResult();
            result.nextWorld = worldIndex;
            result.nextLevel = levelInWorld;
            if (gameCompleted) return result;

            int before = UnlockedInWorld(worldIndex);
            worldPoints[worldIndex] += UnlockProgression.PointsForLevel(isBoss);
            if (worldPoints[worldIndex] > UnlockProgression.MaxPoints)
            {
                worldPoints[worldIndex] = UnlockProgression.MaxPoints;
            }
            int after = UnlockedInWorld(worldIndex);
            for (int i = before; i < after; i++)
            {
                result.newlyUnlocked.Add(i);
            }

            bool worldDone = after >= Worlds.Get(worldIndex).animalIds.Length;
            if (worldDone)
            {
                result.worldCompleted = true;
                if (worldIndex >= Worlds.Count - 1)
                {
                    gameCompleted = true;
                    result.gameCompleted = true;
                }
                else
                {
                    worldIndex++;
                    levelInWorld = 1;
                    result.nextWorld = worldIndex;
                    result.nextLevel = 1;
                }
            }
            else
            {
                levelInWorld = Math.Min(levelInWorld + 1, ProgressionConfig.MaxLevelInWorld);
                result.nextLevel = levelInWorld;
            }
            return result;
        }
    }
}