using UnityEngine;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Runtime-generated sprites so the game always has usable, anti-aliased art
    /// even before real art exists. RoundedSquare and Shadow carry 9-slice border
    /// data, so once real chrome textures land in Resources/Art/UI (see
    /// ArtLoader.GetUi) buttons/panels swap over via UiFactory with no code
    /// changes anywhere else, and keep scaling correctly at every button size.
    /// </summary>
    public static class UiSprites
    {
        private static Sprite _white;
        private static Sprite _circle;
        private static Sprite _ring;
        private static Sprite _rounded;
        private static Sprite _shadow;

        private const int RoundedSize = 128;
        private const float RoundedRadius = 24f;

        public static Sprite White
        {
            get
            {
                if (_white == null)
                {
                    var tex = NewTex(4);
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
                    var tex = NewTex(size);
                    float center = (size - 1) / 2f;
                    float radius = size / 2f - 1f;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, SoftEdge(d, radius)));
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
                    var tex = NewTex(size);
                    float center = (size - 1) / 2f;
                    float outer = size / 2f - 1f;
                    float inner = outer - 8f;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                            float a = Mathf.Min(SoftEdge(d, outer), 1f - SoftEdge(d, inner));
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(a)));
                        }
                    }
                    tex.Apply();
                    _ring = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                    _ring.name = "UiSprites_Ring";
                }
                return _ring;
            }
        }

        /// <summary>
        /// Soft-edged rounded square with 9-slice border data baked in. This is
        /// the automatic fallback background for every UiFactory button/panel
        /// until real art (Resources/Art/UI/&lt;key&gt;) is supplied — swapping in
        /// real art later needs zero code changes.
        /// </summary>
        public static Sprite RoundedSquare
        {
            get
            {
                if (_rounded == null)
                {
                    int size = RoundedSize;
                    float radius = RoundedRadius;
                    float half = size / 2f;
                    float inner = half - radius;
                    var tex = NewTex(size);
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = Mathf.Max(Mathf.Abs(x + 0.5f - half) - inner, 0f);
                            float dy = Mathf.Max(Mathf.Abs(y + 0.5f - half) - inner, 0f);
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            tex.SetPixel(x, y, new Color(1f, 1f, 1f, SoftEdge(d, radius)));
                        }
                    }
                    tex.Apply();
                    var border = new Vector4(radius, radius, radius, radius);
                    _rounded = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                        size, 0, SpriteMeshType.FullRect, border);
                    _rounded.name = "UiSprites_RoundedSquare";
                }
                return _rounded;
            }
        }

        /// <summary>
        /// Soft blurred rounded rect used as a drop shadow behind buttons/panels,
        /// so every UI element reads with a bit of depth even before real art
        /// exists. Feather is wide (28px) instead of the 1px AA band used
        /// elsewhere, which reads as a soft blur without a real gaussian pass.
        /// </summary>
        public static Sprite Shadow
        {
            get
            {
                if (_shadow == null)
                {
                    int size = 160;
                    float radius = 28f;
                    float feather = 28f;
                    float half = size / 2f;
                    float inner = half - radius;
                    var tex = NewTex(size);
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = Mathf.Max(Mathf.Abs(x + 0.5f - half) - inner, 0f);
                            float dy = Mathf.Max(Mathf.Abs(y + 0.5f - half) - inner, 0f);
                            float d = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                            float a = Mathf.Clamp01(1f - d / feather);
                            tex.SetPixel(x, y, new Color(0f, 0f, 0f, a * 0.35f));
                        }
                    }
                    tex.Apply();
                    float b = radius + feather;
                    _shadow = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                        size, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
                    _shadow.name = "UiSprites_Shadow";
                }
                return _shadow;
            }
        }

        private static Texture2D NewTex(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        /// <summary>1px anti-aliased edge: opaque well inside the radius, clear well outside.</summary>
        private static float SoftEdge(float distance, float radius)
        {
            return Mathf.Clamp01(radius - distance + 0.5f);
        }
    }

    /// <summary>
    /// Font access with a graceful upgrade path: drop TTF/OTF files into
    /// Resources/Fonts/Display.ttf (chunky/logo-style, for titles) and
    /// Resources/Fonts/Body.ttf (rounded/readable, for buttons and body text)
    /// and every screen picks them up automatically next run. Until then,
    /// everything falls back to Unity's built-in font so the game always
    /// compiles and runs with zero setup.
    /// </summary>
    public static class UiFonts
    {
        private static Font _default;
        private static Font _display;
        private static Font _body;
        private static bool _displayChecked;
        private static bool _bodyChecked;

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

        /// <summary>Title/logo-style font, if Resources/Fonts/Display.ttf has been dropped in.</summary>
        public static Font Display
        {
            get
            {
                if (!_displayChecked)
                {
                    _display = ArtLoader.GetFont("Display");
                    _displayChecked = true;
                }
                return _display != null ? _display : Default;
            }
        }

        /// <summary>Button/body font, if Resources/Fonts/Body.ttf has been dropped in.</summary>
        public static Font Body
        {
            get
            {
                if (!_bodyChecked)
                {
                    _body = ArtLoader.GetFont("Body");
                    _bodyChecked = true;
                }
                return _body != null ? _body : Default;
            }
        }
    }
}