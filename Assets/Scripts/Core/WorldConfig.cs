namespace AnimalGrid.Core
{
    /// <summary>
    /// One world: theme + its 10 discoverable animals (in unlock order).
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public class WorldConfig
    {
        public int index;
        public string id;
        public string name;
        public string theme;
        public string[] animalIds;
    }

    public static class Worlds
    {
        public static readonly WorldConfig[] All =
        {
            new WorldConfig
            {
                index = 0, id = "forest", name = "Forest", theme = "forest",
                animalIds = new[] { "dog", "cat", "fox", "rabbit", "frog", "panda", "bear", "deer", "owl", "hedgehog" }
            },
            new WorldConfig
            {
                index = 1, id = "sky", name = "Sky", theme = "sky",
                animalIds = new[] { "sparrow", "parrot", "eagle", "flamingo", "bat", "hummingbird", "peacock", "toucan", "duck", "dragonfly" }
            },
            new WorldConfig
            {
                index = 2, id = "sea", name = "Sea", theme = "sea",
                animalIds = new[] { "dolphin", "turtle", "crab", "octopus", "whale", "seahorse", "starfish", "jellyfish", "seal", "clownfish" }
            }
        };

        public static int Count
        {
            get { return All.Length; }
        }

        public static WorldConfig Get(int index)
        {
            if (index < 0) index = 0;
            if (index > All.Length - 1) index = All.Length - 1;
            return All[index];
        }

        /// <summary>"dog" -> "Dog" for display.</summary>
        public static string DisplayName(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            return char.ToUpper(id[0]) + id.Substring(1);
        }
    }
}