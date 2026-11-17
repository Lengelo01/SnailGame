using System.Collections;
using UnityEngine;

public static class CabbageEatingAnimation
{
    // The meal runs in unscaled time while enemies and traps are frozen.
    public static IEnumerator Play(PlayerMovement player, SpriteRenderer cabbage)
    {
        var snail = player.GetComponentInChildren<SpriteRenderer>();
        if (snail == null || cabbage == null) yield break;
        var animator = player.GetComponent<Animator>();
        var cabbageAnimator = cabbage.GetComponent<Animator>();
        bool movementEnabled = player.enabled;
        bool animatorEnabled = animator != null && animator.enabled;
        bool cabbageAnimatorEnabled = cabbageAnimator != null && cabbageAnimator.enabled;
        float previousTimeScale = Time.timeScale;
        Vector3 snailPosition = snail.transform.localPosition;
        Vector3 snailScale = snail.transform.localScale;
        Vector3 cabbageScale = cabbage.transform.localScale;
        Color cabbageColor = cabbage.color;
        Vector3 toward = cabbage.transform.position - snail.transform.position;
        toward.z = 0;
        toward = toward.sqrMagnitude > 0.0001f ? toward.normalized : Vector3.up;
        float size = Mathf.Max(snail.bounds.size.x, snail.bounds.size.y, 0.25f);
        var root = new GameObject("Cabbage eating cinematic");
        Sprite pixel = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            Vector2.one * 0.5f, Texture2D.whiteTexture.width);
        var crumbs = new SpriteRenderer[8];
        for (int i = 0; i < crumbs.Length; i++)
        {
            var crumb = new GameObject("Cabbage leaf crumb").AddComponent<SpriteRenderer>();
            crumb.transform.SetParent(root.transform);
            crumb.sprite = pixel;
            crumb.sortingLayerID = snail.sortingLayerID;
            crumb.sortingOrder = Mathf.Max(snail.sortingOrder, cabbage.sortingOrder) + 4;
            crumb.enabled = false;
            crumbs[i] = crumb;
        }
        player.enabled = false;
        if (animator != null)
        {
            animator.SetFloat("MoveX", toward.x);
            animator.SetFloat("MoveY", toward.y);
            animator.SetBool("IsMoving", true);
            animator.Update(0f);
            animator.enabled = false;
        }
        if (cabbageAnimator != null) cabbageAnimator.enabled = false;
        Time.timeScale = 0f;
        Vector3 worldOrigin = snail.transform.position;
        Vector3 foodPosition = cabbage.transform.position;
        float elapsed = 0f;
        try
        {
            while (elapsed < 2.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float meal = Mathf.Clamp01((elapsed - 0.2f) / 1.55f);
                float bite = meal < 1f ? Mathf.Max(0, Mathf.Sin(meal * Mathf.PI * 8)) : 0;
                float approach = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / 0.2f));
                snail.transform.position = worldOrigin + toward * size * (0.12f * approach + 0.07f * bite);
                float happy = Mathf.Sin(Mathf.InverseLerp(1.8f, 2.2f, elapsed) * Mathf.PI);
                snail.transform.localScale = Vector3.Scale(snailScale,
                    new Vector3(1 + bite * 0.1f - happy * 0.06f, 1 - bite * 0.08f + happy * 0.12f, 1));
                cabbage.transform.localScale = cabbageScale * Mathf.Lerp(1, 0.08f, meal);
                Color tint = cabbageColor;
                tint.a *= 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.85f, 1, meal));
                cabbage.color = tint;
                float burst = (meal * 4) % 1f;
                for (int i = 0; i < crumbs.Length; i++)
                {
                    var crumb = crumbs[i];
                    crumb.enabled = meal > 0 && meal < 1;
                    float angle = i * Mathf.PI * 2 / crumbs.Length;
                    var radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                    crumb.transform.position = foodPosition + radial * size * burst * 0.65f + Vector3.down * size * burst * burst * 0.25f;
                    crumb.transform.rotation = Quaternion.Euler(0, 0, i * 45 + burst * 180);
                    crumb.transform.localScale = Vector3.one * size * 0.065f * (1 - burst);
                    crumb.color = i % 2 == 0 ? new Color(0.67f, 0.9f, 0.32f, 1 - burst) : new Color(0.29f, 0.62f, 0.18f, 1 - burst);
                }
                yield return null;
            }
        }
        finally
        {
            if (snail != null) { snail.transform.localPosition = snailPosition; snail.transform.localScale = snailScale; }
            if (cabbage != null) { cabbage.transform.localScale = cabbageScale; cabbage.color = cabbageColor; }
            if (player != null) player.enabled = movementEnabled;
            if (animator != null) animator.enabled = animatorEnabled;
            if (cabbageAnimator != null) cabbageAnimator.enabled = cabbageAnimatorEnabled;
            Time.timeScale = previousTimeScale;
            Object.Destroy(root);
            Object.Destroy(pixel);
        }
    }
}
