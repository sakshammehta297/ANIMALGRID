using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Lightweight press feedback for UiFactory buttons: scales down a touch on
    /// press and springs back on release/exit. Pure coroutine lerp, so it needs
    /// no external animation package and works today. Safe to leave in place
    /// after DOTween (or another tween library) is added later — swap the body
    /// of Lerp() for a DOTween call at that point if you want easing curves.
    /// Added automatically by UiFactory.MakeButton / MakeIconButton.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UiButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float PressedScale = 0.94f;
        private const float AnimTime = 0.09f;

        private RectTransform rect;
        private Vector3 baseScale;
        private Coroutine running;

        private void Awake()
        {
            rect = (RectTransform)transform;
            baseScale = rect.localScale;
        }

        public void OnPointerDown(PointerEventData eventData) => AnimateTo(baseScale * PressedScale);
        public void OnPointerUp(PointerEventData eventData) => AnimateTo(baseScale);
        public void OnPointerExit(PointerEventData eventData) => AnimateTo(baseScale);

        private void AnimateTo(Vector3 target)
        {
            if (!gameObject.activeInHierarchy) return;
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Lerp(target));
        }

        private IEnumerator Lerp(Vector3 target)
        {
            Vector3 start = rect.localScale;
            float t = 0f;
            while (t < AnimTime)
            {
                t += Time.unscaledDeltaTime;
                rect.localScale = Vector3.Lerp(start, target, t / AnimTime);
                yield return null;
            }
            rect.localScale = target;
        }
    }
}
