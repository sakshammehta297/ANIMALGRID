using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// The visual representation of one board cell: rounded tile,
    /// white rounded X bars, and animal sticker (or fallback circle).
    /// Animations here are plain coroutines (no external tween package needed):
    /// a staggered pop-in when the board loads, a punchy pop when an animal or
    /// X is placed, a quick shrink when one is removed, and a celebratory
    /// bounce (PlayWinBounce) used on solved cells when a level completes.
    /// </summary>
    public class CellView : MonoBehaviour
    {
        public Image background;
        public int row;
        public int column;

        private GameObject xRoot;
        private GameObject animalRoot;
        private RectTransform cellRect;

        private void Awake()
        {
            cellRect = GetComponent<RectTransform>();
            if (background == null) background = GetComponent<Image>();
        }

        public void Setup(int row, int column, float size, Color regionColor)
        {
            this.row = row;
            this.column = column;

            if (cellRect == null) cellRect = GetComponent<RectTransform>();
            cellRect.sizeDelta = new Vector2(size, size);
            cellRect.localScale = Vector3.one;

            if (background == null) background = GetComponent<Image>();
            background.sprite = UiSprites.RoundedSquare;
            background.color = regionColor;

            // Staggered pop-in as the board loads, sweeping diagonally across the grid.
            float delay = (row + column) * 0.025f;
            StartCoroutine(PopIn(cellRect, delay, 0.3f));
        }

        /// <summary>
        /// Resets all visual state so this cell can be reused from the pool.
        /// Stops running animations, hides children, resets scale.
        /// </summary>
        public void ResetForPool()
        {
            StopAllCoroutines();

            if (cellRect != null)
                cellRect.localScale = Vector3.one;

            if (xRoot != null)
            {
                xRoot.transform.localScale = Vector3.one;
                xRoot.SetActive(false);
            }

            if (animalRoot != null)
            {
                animalRoot.transform.localScale = Vector3.one;
                animalRoot.SetActive(false);
            }

            if (background != null)
                background.color = Color.white;

            row = 0;
            column = 0;
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
                xRoot.SetActive(false);
                MakeBar(45f);
                MakeBar(-45f);
            }
            if (xRoot == null) return;

            if (visible)
            {
                bool wasHidden = !xRoot.activeSelf;
                xRoot.SetActive(true);
                if (wasHidden) StartCoroutine(PopIn(xRoot.transform as RectTransform, 0f, 0.16f));
            }
            else if (xRoot.activeSelf)
            {
                StartCoroutine(ShrinkOutThenDeactivate(xRoot, 0.1f));
            }
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
                animalRoot.SetActive(false);
                var rect = animalRoot.transform as RectTransform;
                float parentSize = (transform as RectTransform).sizeDelta.x;
                rect.sizeDelta = new Vector2(parentSize * 0.92f, parentSize * 0.92f);
                animalRoot.GetComponent<Image>().raycastTarget = false;
            }
            if (animalRoot == null) return;

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
                bool wasHidden = !animalRoot.activeSelf;
                animalRoot.SetActive(true);
                if (wasHidden) StartCoroutine(PopIn(animalRoot.transform as RectTransform, 0f, 0.22f));
            }
            else if (animalRoot.activeSelf)
            {
                StartCoroutine(ShrinkOutThenDeactivate(animalRoot, 0.12f));
            }
        }

        /// <summary>
        /// Small celebratory bounce for a solved cell's animal (falls back to
        /// the whole tile if no animal is showing). Scales up and settles back
        /// down without disappearing, unlike PopIn — call with a small
        /// per-cell `delay` from the caller to sweep across the solved cells.
        /// </summary>
        public void PlayWinBounce(float delay = 0f)
        {
            var target = animalRoot != null ? (RectTransform)animalRoot.transform : cellRect;
            StartCoroutine(Bounce(target, delay));
        }

        // ---------- Shared coroutine animations ----------

        private static IEnumerator PopIn(RectTransform target, float delay, float duration)
        {
            target.localScale = Vector3.zero;
            if (delay > 0f) yield return new WaitForSeconds(delay);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Ease.OutBack(Mathf.Clamp01(t / duration));
                target.localScale = Vector3.one * Mathf.Max(k, 0f);
                yield return null;
            }
            target.localScale = Vector3.one;
        }

        private static IEnumerator ShrinkOutThenDeactivate(GameObject go, float duration)
        {
            var target = go.transform as RectTransform;
            Vector3 start = target.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                target.localScale = Vector3.Lerp(start, Vector3.zero, t / duration);
                yield return null;
            }
            target.localScale = Vector3.zero;
            go.SetActive(false);
        }

        private static IEnumerator Bounce(RectTransform target, float delay, float duration = 0.32f, float peak = 1.25f)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                float k = Mathf.Sin(p * Mathf.PI);
                target.localScale = Vector3.one * (1f + (peak - 1f) * Mathf.Max(k, 0f));
                yield return null;
            }
            target.localScale = Vector3.one;
        }
    }
}