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

        /// <summary>
        /// Standard text. Best-fit is always on: fontSize is the ceiling and
        /// minSize (default 14) the floor. Pass minSize/maxSize to override.
        /// </summary>
        public static Text MakeText(Transform parent, string content, Vector2 pos, Vector2 size,
            int fontSize, Color color, bool useDisplayFont = false,
            int minSize = -1, int maxSize = -1)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var text = go.GetComponent<Text>();
            text.font = useDisplayFont ? UiFonts.Display : UiFonts.Body;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minSize > 0 ? minSize : Mathf.Min(14, fontSize);
            text.resizeTextMaxSize = maxSize > 0 ? maxSize : fontSize;
            return text;
        }

        /// <summary>
        /// Points an Image at real chrome art (Resources/Art/UI/&lt;artKey&gt;) when
        /// available, otherwise falls back to the procedural 9-sliced
        /// RoundedSquare tinted with `fallbackColor`. Always sets Sliced so
        /// corners stay correct at any element size.
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
        /// with procedural art.
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
        /// feedback. Label defaults to font size 56 with best-fit; override
        /// with labelFontSize.
        /// </summary>
        public static Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size,
            Color background, Color textColor, string artKey = null, bool withShadow = true,
            int labelFontSize = 56)
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
            MakeText(go.transform, label, Vector2.zero, size, labelFontSize, textColor);
            go.AddComponent<UiButtonFeedback>();
            return go.GetComponent<Button>();
        }

        /// <summary>
        /// Icon-on-top, label-below tile button. Label defaults to font size 30
        /// (min 14); override with labelFontSize / labelMinSize.
        /// </summary>
        public static Button MakeIconButton(Transform parent, string iconKey, string label,
            Vector2 pos, Vector2 size, Color background, Color textColor, string artKey = null,
            int labelFontSize = 30, int labelMinSize = -1)
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
            MakeText(go.transform, label, new Vector2(0f, labelY), new Vector2(size.x - 16f, 60f),
                labelFontSize, textColor, false, labelMinSize);
            go.AddComponent<UiButtonFeedback>();
            return go.GetComponent<Button>();
        }

        /// <summary>
        /// Full-art image with a safe "nothing yet" fallback: returns null and
        /// adds no GameObject at all if Resources/Art/&lt;artName&gt; doesn't exist.
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
        /// Returns the label Text so callers can style it further (e.g. Outline).
        /// </summary>
        public static Text MakeUnlockBar(Transform parent, Vector2 anchor, Vector2 pos,
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
            // be combined with Sliced on the same Image, so it's set up directly here.
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

            return MakeText(root.transform, label, new Vector2(0f, -22f), new Vector2(760f, 40f), 30, labelColor);
        }
    }
}