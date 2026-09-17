using System.Collections.Generic;

namespace AnimalGrid.Core
{
    /// <summary>
    /// The animal roster used by levels. Ids map to Resources/Art/animal_<id>.
    /// Pure C# - no Unity dependencies!
    /// </summary>
    public static class AnimalRoster
    {
        private static readonly string[] BaseIds =
        {
            "cat", "panda", "fox", "rabbit", "frog"
        };

        public static List<string> IdsFor(int count)
        {
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                list.Add(BaseIds[i % BaseIds.Length]);
            }
            return list;
        }
    }
}