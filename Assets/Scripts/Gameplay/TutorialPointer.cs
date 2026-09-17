using UnityEngine;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// A pulsing ring that follows a target UI element ("tap THIS!").
    /// </summary>
    public class TutorialPointer : MonoBehaviour
    {
        public RectTransform target;
        private RectTransform rect;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            if (target == null) return;
            rect.position = target.position;
            float max = Mathf.Max(target.sizeDelta.x, target.sizeDelta.y) * 1.3f;
            rect.sizeDelta = new Vector2(max, max);
            float pulse = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 6f);
            rect.localScale = Vector3.one * pulse;
        }
    }
}