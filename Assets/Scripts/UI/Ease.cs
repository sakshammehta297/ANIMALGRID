namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Small dependency-free easing helpers used by CellView / GameplayController
    /// animations so spawn/placement/win effects feel consistent without needing
    /// an external tweening package (DOTween etc.). t is expected in [0,1].
    /// If a tween package is added later, these can be swapped for its built-in
    /// eases one call site at a time — nothing else needs to change.
    /// </summary>
    public static class Ease
    {
        /// <summary>Overshoots past 1 then settles back — the "pop" feel used for spawn/placement.</summary>
        public static float OutBack(float t, float overshoot = 1.70158f)
        {
            t -= 1f;
            return t * t * ((overshoot + 1f) * t + overshoot) + 1f;
        }

        public static float OutCubic(float t)
        {
            float f = t - 1f;
            return f * f * f + 1f;
        }

        public static float InOutSine(float t)
        {
            return -(UnityEngine.Mathf.Cos(UnityEngine.Mathf.PI * t) - 1f) / 2f;
        }
    }
}
