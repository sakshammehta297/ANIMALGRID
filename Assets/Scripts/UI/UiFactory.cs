using UnityEngine;
using UnityEngine.UI;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Shared code-UI builders so every screen uses the same primitives.
    /// </summary>
    public static class UiFactory
    {
        public static GameObject MakePanel(Transform parent, Color color)
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.transform as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        public static Text MakeText(Transform parent, string content, Vector2 pos, Vector2 size,
            int fontSize, Color color, bool useDisplayFont = false)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var text = go.GetComponent<Text>();
            // Body/Display both fall back to the built-in font until a custom
            // Resources/Fonts/*.ttf is dropped in, so this is a no-op today and
            // an automatic upgrade later. Pass useDisplayFont: true for
            // titles/headers once a Display font exists.
            text.font = useDisplayFont ? UiFonts.Display : UiFonts.Body;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;

            // Auto-shrink instead of spilling past the button/panel edge —
            // matters a lot more now that buttons have a busy wood-grain
            // background art instead of a flat color, where overflowing text
            // is much more noticeable. fontSize is the ceiling; it only
            // shrinks for longer labels ("Daily Challenge") or narrower boxes.
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Min(14, fontSize);
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        /// <summary>
        /// Points an Image at real chrome art (Resources/Art/UI/&lt;artKey&gt;) when
        /// available, otherwise falls back to the procedural 9-sliced
        /// RoundedSquare tinted with `fallbackColor`. Always sets Sliced so
        /// corners stay correct at any element size — for real art, set a
        /// Border in the Sprite Editor on import, or tune
        /// image.pixelsPerUnitMultiplier afterwards if corners look too big/small.
        ///
        /// When `artKey` is null/empty, this tries `defaultKey` first — so once
        /// that art exists (e.g. Resources/Art/UI/button_wood.png), every
        /// caller that didn't ask for something specific picks it up
        /// automatically with no further code changes anywhere. Pass an
        /// explicit artKey (e.g. "button_gold") to opt out of that default.
        /// </summary>
        private static void ApplyChrome(Image img, string artKey, Color fallbackColor, string defaultKey = "button_wood")
        {
            string key = string.IsNullOrEmpty(artKey) ? defaultKey : artKey;
            var art = ArtLoader.GetUi(key);
            if (art != null)
            {
                img.sprite = art;
                img.color = Color.white;
            }
            else
            {
                img.sprite = UiSprites.RoundedSquare;
                img.color = fallbackColor;
            }
            img.type = Image.Type.Sliced;
        }

        /// <summary>
        /// Adds a soft drop-shadow behind an element so it reads with depth even
        /// with procedural art. Cheap and safe to skip (withShadow: false) for
        /// tiny/inline elements where it would be visual noise.
        /// </summary>
        private static void AddShadow(Transform parent, Vector2 inset)
        {
            var shadowGo = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            shadowGo.transform.SetParent(parent, false);
            shadowGo.transform.SetAsFirstSibling();
            var rect = shadowGo.transform as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(-inset.x, -inset.y);
            rect.offsetMax = new Vector2(inset.x, 0f);
            var img = shadowGo.GetComponent<Image>();
            img.sprite = UiSprites.Shadow;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
        }

        /// <summary>
        /// Standard pill/rect button: art-ready background, drop shadow, press
        /// feedback. Pass artKey (e.g. "button_wood") once real chrome art
        /// exists at Resources/Art/UI/button_wood — until then it falls back to
        /// the procedural rounded rect tinted with `background`.
        /// </summary>
        public static Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size,
            Color background, Color textColor, string artKey = null, bool withShadow = true)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            if (withShadow) AddShadow(go.transform, new Vector2(6f, 12f));

            var img = go.GetComponent<Image>();
            ApplyChrome(img, artKey, background);

            MakeText(go.transform, label, Vector2.zero, size, 56, textColor);
            go.AddComponent<UiButtonFeedback>();
            return go.GetComponent<Button>();
        }

        /// <summary>
        /// Icon-on-top, label-below tile button — the pattern used for a bottom
        /// nav row (Collection / Achievements / Daily Challenge / Shop, etc.).
        /// The icon is optional: if Resources/Art/Icons/icon_&lt;iconKey&gt; isn't
        /// present yet, the tile just shows the centered label, so screens can be
        /// wired up with this today and pick up icons automatically once art
        /// lands — no code changes needed later.
        /// </summary>
        public static Button MakeIconButton(Transform parent, string iconKey, string label,
            Vector2 pos, Vector2 size, Color background, Color textColor, string artKey = null)
        {
            var go = new GameObject("Button_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            AddShadow(go.transform, new Vector2(4f, 10f));

            var img = go.GetComponent<Image>();
            ApplyChrome(img, artKey, background);

            var icon = ArtLoader.GetIcon(iconKey);
            float labelY;
            if (icon != null)
            {
                var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(go.transform, false);
                var iRect = iconGo.transform as RectTransform;
                iRect.anchorMin = iRect.anchorMax = new Vector2(0.5f, 0.5f);
                iRect.anchoredPosition = new Vector2(0f, size.y * 0.16f);
                float iconSize = Mathf.Min(size.x, size.y) * 0.42f;
                iRect.sizeDelta = new Vector2(iconSize, iconSize);
                var iImg = iconGo.GetComponent<Image>();
                iImg.sprite = icon;
                iImg.preserveAspect = true;
                iImg.raycastTarget = false;
                labelY = -size.y * 0.28f;
            }
            else
            {
                labelY = 0f; // no icon art yet: center the label in the tile
            }

            MakeText(go.transform, label, new Vector2(0f, labelY), new Vector2(size.x - 16f, 60f), 30, textColor);
            go.AddComponent<UiButtonFeedback>();
            return go.GetComponent<Button>();
        }

        /// <summary>
        /// Full-art image with a safe "nothing yet" fallback: returns null and
        /// adds no GameObject at all if Resources/Art/&lt;artName&gt; doesn't exist,
        /// so callers can just skip/placeholder cleanly (mirrors the mascot
        /// pattern already used in GameplayBootstrap.BuildHomeOverlay).
        /// </summary>
        public static Image MakeArtImage(Transform parent, string artName, Vector2 pos, Vector2 size)
        {
            var sprite = ArtLoader.Get(artName);
            if (sprite == null) return null;

            var go = new GameObject("Art_" + artName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>
        /// The unlock progress bar: rounded track + gold fill + label underneath.
        /// </summary>
        public static void MakeUnlockBar(Transform parent, Vector2 anchor, Vector2 pos,
            float fraction, string label, Color labelColor, string trackArtKey = null, string fillArtKey = null)
        {
            var root = new GameObject("UnlockBar", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var r = root.transform as RectTransform;
            r.anchorMin = r.anchorMax = anchor;
            r.anchoredPosition = pos;
            r.sizeDelta = new Vector2(560f, 70f);

            var bgGo = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(root.transform, false);
            var bgRect = bgGo.transform as RectTransform;
            bgRect.anchorMin = bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = new Vector2(0f, 12f);
            bgRect.sizeDelta = new Vector2(560f, 34f);
            var bg = bgGo.GetComponent<Image>();
            ApplyChrome(bg, trackArtKey, new Color(0.25f, 0.2f, 0.15f, 0.35f), "bar_track");
            bg.raycastTarget = false;

            // Fill needs Image.Type.Filled (for the horizontal wipe), which can't
            // be combined with Sliced on the same Image, so it's set up directly
            // here instead of going through ApplyChrome.
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(bgGo.transform, false);
            var fRect = fillGo.transform as RectTransform;
            fRect.anchorMin = new Vector2(0f, 0.5f);
            fRect.anchorMax = new Vector2(0f, 0.5f);
            fRect.pivot = new Vector2(0f, 0.5f);
            fRect.anchoredPosition = new Vector2(6f, 0f);
            fRect.sizeDelta = new Vector2(548f, 22f);
            var fill = fillGo.GetComponent<Image>();
            string fillKey = string.IsNullOrEmpty(fillArtKey) ? "bar_fill" : fillArtKey;
            var fillArt = ArtLoader.GetUi(fillKey);
            if (fillArt != null)
            {
                fill.sprite = fillArt;
                fill.color = Color.white;
            }
            else
            {
                fill.sprite = UiSprites.RoundedSquare;
                fill.color = new Color(0.95f, 0.75f, 0.2f);
            }
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = Mathf.Clamp01(fraction);
            fill.raycastTarget = false;

            MakeText(root.transform, label, new Vector2(0f, -22f), new Vector2(760f, 40f), 30, labelColor);
        }
    }
}