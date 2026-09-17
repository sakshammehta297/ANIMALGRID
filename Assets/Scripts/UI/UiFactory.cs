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
            int fontSize, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var text = go.GetComponent<Text>();
            text.font = UiFonts.Default;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            return text;
        }

        public static Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size,
            Color background, Color textColor)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = UiSprites.RoundedSquare;
            img.color = background;
            MakeText(go.transform, label, Vector2.zero, size, 56, textColor);
            return go.GetComponent<Button>();
        }

        /// <summary>
        /// The unlock progress bar: rounded track + gold fill + label underneath.
        /// </summary>
        public static void MakeUnlockBar(Transform parent, Vector2 anchor, Vector2 pos,
            float fraction, string label, Color labelColor)
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
            bg.sprite = UiSprites.RoundedSquare;
            bg.color = new Color(0.25f, 0.2f, 0.15f, 0.35f);
            bg.raycastTarget = false;

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(bgGo.transform, false);
            var fRect = fillGo.transform as RectTransform;
            fRect.anchorMin = new Vector2(0f, 0.5f);
            fRect.anchorMax = new Vector2(0f, 0.5f);
            fRect.pivot = new Vector2(0f, 0.5f);
            fRect.anchoredPosition = new Vector2(6f, 0f);
            fRect.sizeDelta = new Vector2(548f, 22f);
            var fill = fillGo.GetComponent<Image>();
            fill.sprite = UiSprites.RoundedSquare;
            fill.color = new Color(0.95f, 0.75f, 0.2f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = Mathf.Clamp01(fraction);
            fill.raycastTarget = false;

            MakeText(root.transform, label, new Vector2(0f, -22f), new Vector2(760f, 40f), 30, labelColor);
        }
    }
}