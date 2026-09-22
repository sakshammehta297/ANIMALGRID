using UnityEngine;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Loads optional art from Resources/Art. Returns null when art is missing,
    /// so the game always falls back to code-drawn visuals (see UiSprites).
    ///
    /// Folder convention (create these under Assets/Resources as art arrives —
    /// Resources.Load only ever sees files under a folder literally named
    /// "Resources"):
    ///   Resources/Art/&lt;name&gt;.png            full backgrounds        -> Get(name)
    ///   Resources/Art/animal_&lt;id&gt;.png        animal stickers        -> GetAnimal(id)
    ///   Resources/Art/UI/&lt;name&gt;.png          button/panel chrome    -> GetUi(name)
    ///   Resources/Art/Icons/icon_&lt;id&gt;.png     small nav/tile icons   -> GetIcon(id)
    ///   Resources/Fonts/Display.ttf, Body.ttf   custom fonts           -> UiFonts.Display / .Body
    ///
    /// Import UI chrome sprites as Sprite (2D and UI) and set a Border in the
    /// Sprite Editor so 9-slicing looks right at any button size; icons and
    /// animal stickers can stay border-free.
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

        /// <summary>Button/panel background chrome, e.g. GetUi("button_wood") -> Art/UI/button_wood.</summary>
        public static Sprite GetUi(string name)
        {
            return Get("UI/" + name);
        }

        /// <summary>Small tile/nav icon, e.g. GetIcon("shop") -> Art/Icons/icon_shop.</summary>
        public static Sprite GetIcon(string id)
        {
            return Get("Icons/icon_" + id);
        }

        /// <summary>Custom TTF/OTF font, e.g. GetFont("Display") -> Fonts/Display.</summary>
        public static Font GetFont(string name)
        {
            return Resources.Load<Font>("Fonts/" + name);
        }
    }
}