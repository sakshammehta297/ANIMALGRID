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
    }
}