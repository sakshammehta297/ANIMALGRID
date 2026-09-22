using UnityEngine;
using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// ScriptableObject definition for world configuration.
    /// Replaces hardcoded WorldConfig with data-driven design.
    /// Create instances via Assets > Create > Animal Grid > World Config
    /// </summary>
    [CreateAssetMenu(fileName = "NewWorldConfig", menuName = "Animal Grid/World Config", order = 1)]
    public class WorldConfigSO : ScriptableObject
    {
        [Header("World Identity")]
        [Tooltip("Unique identifier for this world (e.g., 'forest', 'sky', 'sea')")]
        public string worldId = "forest";
        
        [Tooltip("Display name shown to players")]
        public string displayName = "Forest";
        
        [Tooltip("Theme used for visual styling")]
        public string theme = "forest";
        
        [Header("Animals")]
        [Tooltip("List of animal IDs in unlock order (10 animals recommended)")]
        public List<string> animalIds = new List<string>();
        
        [Header("Difficulty Settings")]
        [Tooltip("Base grid size for levels in this world")]
        public int baseGridSize = 5;
        
        [Tooltip("Maximum grid size for boss levels")]
        public int maxGridSize = 7;
        
        [Tooltip("Points required to unlock each animal")]
        public int pointsPerAnimal = 10;
        
        /// <summary>
        /// Get the number of animals in this world.
        /// </summary>
        public int AnimalCount => animalIds.Count;
        
        /// <summary>
        /// Check if an animal index is valid.
        /// </summary>
        public bool IsValidAnimalIndex(int index)
        {
            return index >= 0 && index < animalIds.Count;
        }
        
        /// <summary>
        /// Get display name for an animal ID.
        /// </summary>
        public static string GetAnimalDisplayName(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            return char.ToUpper(id[0]) + id.Substring(1);
        }
    }
}
