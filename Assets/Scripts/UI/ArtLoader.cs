using UnityEngine;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Loads optional art from Resources/Art. Returns null when art is missing,
    /// so the game always falls back to code-drawn visuals.
    /// </summary>
    public static class ArtLoader
    {
        public static Sprite Get(string name)
        {
            return Resources.Load<Sprite>("Art/" + name);
        }

        public static Sprite GetAnimal(string id)
        {
            return Get("animal_" + id);
        }
    }
}