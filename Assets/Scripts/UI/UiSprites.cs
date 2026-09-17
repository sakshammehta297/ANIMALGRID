using UnityEngine;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Runtime-generated sprites so we need no art assets for shapes.
    /// </summary>
    public static class UiSprites
    {
        private static Sprite _white;
        private static Sprite _circle;
        private static Sprite _ring;
        private static Sprite _rounded;

        public static Sprite White
        {
            get
            {
                if (_white == null)
                {
                    var tex = new Texture2D(4, 4);
                    var pixels = new Color32[16];
                    for (int i = 0; i < 16; i++) pixels[i] = new Color32(255, 255, 255, 255);
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    _white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
                    _white.name = "UiSprites_White";
                }
                return _white;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (_circle == null)
                {
                    int size = 64;
                    var tex = new Texture2D(size, size);
                    float center = (size - 1) / 2f;
                    float radius = size / 2f - 1f;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                            tex.SetPixel(x, y, d <= radius ? Color.white : Color.clear);
                        }
                    }
                    tex.Apply();
                    _circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                    _circle.name = "UiSprites_Circle";
                }
                return _circle;
            }
        }

        public static Sprite Ring
        {
            get
            {
                if (_ring == null)
                {
                    int size = 96;
                    var tex = new Texture2D(size, size);
                    float center = (size - 1) / 2f;
                    float outer = size / 2f - 1f;
                    float inner = outer - 8f;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                            tex.SetPixel(x, y, (d <= outer && d >= inner) ? Color.white : Color.clear);
                        }
                    }
                    tex.Apply();
                    _ring = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                    _ring.name = "UiSprites_Ring";
                }
                return _ring;
            }
        }

        public static Sprite RoundedSquare
        {
            get
            {
                if (_rounded == null)
                {
                    int size = 128;
                    float radius = 24f;
                    float half = size / 2f;
                    float inner = half - radius;
                    var tex = new Texture2D(size, size);
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = Mathf.Max(Mathf.Abs(x + 0.5f - half) - inner, 0f);
                            float dy = Mathf.Max(Mathf.Abs(y + 0.5f - half) - inner, 0f);
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            tex.SetPixel(x, y, d <= radius ? Color.white : Color.clear);
                        }
                    }
                    tex.Apply();
                    _rounded = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                    _rounded.name = "UiSprites_RoundedSquare";
                }
                return _rounded;
            }
        }
    }

    /// <summary>
    /// Built-in font access so UI text works with zero imports.
    /// </summary>
    public static class UiFonts
    {
        private static Font _default;

        public static Font Default
        {
            get
            {
                if (_default == null)
                {
                    _default = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_default == null) _default = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return _default;
            }
        }
    }
}