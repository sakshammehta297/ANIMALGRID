using UnityEngine;
using UnityEngine.UI;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// The visual representation of one board cell: rounded tile,
    /// white rounded X bars, and animal sticker (or fallback circle).
    /// </summary>
    public class CellView : MonoBehaviour
    {
        public Image background;
        public int row;
        public int column;

        private GameObject xRoot;
        private GameObject animalRoot;

        public void Setup(int row, int column, float size, Color regionColor)
        {
            this.row = row;
            this.column = column;

            var rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);

            if (background == null) background = GetComponent<Image>();
            background.sprite = UiSprites.RoundedSquare;
            background.color = regionColor;
        }

        public void SetRegionColor(Color color)
        {
            background.color = color;
        }

        public void SetXVisible(bool visible)
        {
            if (visible && xRoot == null)
            {
                xRoot = new GameObject("XMark", typeof(RectTransform));
                xRoot.transform.SetParent(transform, false);
                MakeBar(45f);
                MakeBar(-45f);
            }
            if (xRoot != null) xRoot.SetActive(visible);
        }

        private void MakeBar(float angle)
        {
            var bar = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(xRoot.transform, false);
            var rect = bar.transform as RectTransform;
            float parentSize = (transform as RectTransform).sizeDelta.x;
            rect.sizeDelta = new Vector2(parentSize * 0.55f, parentSize * 0.12f);
            rect.localEulerAngles = new Vector3(0f, 0f, angle);
            var img = bar.GetComponent<Image>();
            img.sprite = UiSprites.RoundedSquare;
            img.color = new Color(1f, 1f, 1f, 0.92f);
            img.raycastTarget = false;
        }

        public void SetAnimalVisible(bool visible, Color fallbackColor)
        {
            if (visible && animalRoot == null)
            {
                animalRoot = new GameObject("Animal", typeof(RectTransform), typeof(Image));
                animalRoot.transform.SetParent(transform, false);
                var rect = animalRoot.transform as RectTransform;
                float parentSize = (transform as RectTransform).sizeDelta.x;
                rect.sizeDelta = new Vector2(parentSize * 0.92f, parentSize * 0.92f);
                animalRoot.GetComponent<Image>().raycastTarget = false;
            }

            if (animalRoot != null)
            {
                animalRoot.SetActive(visible);
                if (visible)
                {
                    var img = animalRoot.GetComponent<Image>();
                    var board = GetComponentInParent<BoardView>();
                    var sprite = board != null ? board.AnimalSpriteForCell(row, column) : null;
                    if (sprite != null)
                    {
                        img.sprite = sprite;
                        img.color = Color.white;
                    }
                    else
                    {
                        img.sprite = UiSprites.Circle;
                        img.color = fallbackColor;
                    }
                }
            }
        }
    }
}