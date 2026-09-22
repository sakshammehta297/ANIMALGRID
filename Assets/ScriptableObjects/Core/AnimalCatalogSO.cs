using UnityEngine;
using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// ScriptableObject definition for animal catalog.
    /// Centralized data for all animals in the game.
    /// Create instances via Assets > Create > Animal Grid > Animal Catalog
    /// </summary>
    [CreateAssetMenu(fileName = "AnimalCatalog", menuName = "Animal Grid/Animal Catalog", order = 2)]
    public class AnimalCatalogSO : ScriptableObject
    {
        [Header("Catalog Settings")]
        [Tooltip("All animals available in the game")]
        public List<AnimalEntry> animals = new List<AnimalEntry>();
        
        /// <summary>
        /// Entry for a single animal in the catalog.
        /// </summary>
        [System.Serializable]
        public class AnimalEntry
        {
            [Tooltip("Unique identifier (e.g., 'cat', 'dog')")]
            public string id;
            
            [Tooltip("Display name shown to players (e.g., 'Cat', 'Dog')")]
            public string displayName;
            
            [Tooltip("Associated color name for puzzle matching")]
            public string colorName;
            
            [Tooltip("Optional sprite reference for this animal")]
            public Sprite sprite;
            
            [Tooltip("Optional audio clip for this animal's sound")]
            public AudioClip soundClip;
            
            [Tooltip("Rarity or unlock tier (0 = common, higher = rarer)")]
            public int rarity;
            
            [Tooltip("Short description or fun fact about this animal")]
            [TextArea(2, 4)]
            public string description;
        }
        
        /// <summary>
        /// Get an animal definition by ID.
        /// Returns null if not found.
        /// </summary>
        public AnimalDefinition GetAnimalById(string id)
        {
            foreach (var entry in animals)
            {
                if (entry.id == id)
                {
                    return new AnimalDefinition(entry.id, entry.displayName, entry.colorName);
                }
            }
            return null;
        }
        
        /// <summary>
        /// Get all animal IDs in the catalog.
        /// </summary>
        public List<string> GetAllAnimalIds()
        {
            var ids = new List<string>(animals.Count);
            foreach (var entry in animals)
            {
                ids.Add(entry.id);
            }
            return ids;
        }
        
        /// <summary>
        /// Check if an animal ID exists in the catalog.
        /// </summary>
        public bool HasAnimal(string id)
        {
            foreach (var entry in animals)
            {
                if (entry.id == id) return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get count of animals in catalog.
        /// </summary>
        public int Count => animals.Count;
    }
}
