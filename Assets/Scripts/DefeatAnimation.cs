using System.Collections;
using UnityEngine;

// Runs with unscaled time while the game is frozen. Only visual stand-ins move;
// real actors, collisions, map bounds and the camera target remain untouched.
public static class DefeatAnimation
{
    public static IEnumerator Play(SpriteRenderer victim, SpriteRenderer attacker)
    {
        var root = new GameObject("Defeat cinematic");
        SpriteRenderer snail = Copy(victim, root.transform, "Snail tumble");
        SpriteRenderer bug = Copy(attacker, root.transform, "Ladybug strike");
        victim.enabled = false;
        attacker.enabled = false;
        Vector3 origin = snail.transform.position;
        Vector3 enemyOrigin = bug.transform.position;
        Vector3 snailScale = snail.transform.localScale;
        Vector3 bugScale = bug.transform.localScale;
        Vector3 away = origin - enemyOrigin;
        away.z = 0;
        away = away.sqrMagnitude > 0.0001f ? away.normalized : Vector3.right;
        float size = Mathf.Max(victim.bounds.size.x, victim.bounds.size.y, 0.3f);
        Sprite pixel = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width,
            Texture2D.whiteTexture.height), Vector2.one * 0.5f, Texture2D.whiteTexture.width);
        var sparks = new SpriteRenderer[12];
        for (int i = 0; i < sparks.Length; i++)
        {
            sparks[i] = new GameObject("Impact spark").AddComponent<SpriteRenderer>();
            sparks[i].transform.SetParent(root.transform);
            sparks[i].sprite = pixel;
            sparks[i].sortingLayerID = victim.sortingLayerID;
            sparks[i].sortingOrder = Mathf.Max(victim.sortingOrder, attacker.sortingOrder) + 4;
            sparks[i].enabled = false;
        }
        float time = 0;
        try
        {
            while (time < 1.35f)
            {
                time += Time.unscaledDeltaTime;
                if (time < 0.16f)
                {
                    float t = time / 0.16f;
                    bug.transform.position = enemyOrigin - away * (size * 0.12f * t);
                    bug.transform.localScale = Vector3.Scale(bugScale, new Vector3(1f + 0.18f * t, 1f - 0.2f * t, 1));
                }
                else
                {
                    float hit = Mathf.Clamp01((time - 0.16f) / 0.10f);
                    float recoil = Mathf.SmoothStep(0, 1, Mathf.Clamp01((time - 0.3f) / 0.28f));
                    Vector3 strike = Vector3.Lerp(enemyOrigin, origin - away * size * 0.24f, hit);
                    bug.transform.position = Vector3.Lerp(strike, enemyOrigin, recoil);
                    bug.transform.localScale = Vector3.Lerp(Vector3.Scale(bugScale, new Vector3(1.15f, 0.85f, 1)), bugScale, recoil);
                }

                if (time >= 0.23f)
                {
                    float t = Mathf.Clamp01((time - 0.23f) / 0.78f);
                    float flight = Mathf.SmoothStep(0f, 1f, t);
                    snail.transform.position = origin + away * (size * 0.65f * flight)
                        + Vector3.up * (Mathf.Sin(t * Mathf.PI) * size * 0.65f);
                    snail.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sign(away.x == 0 ? 1 : away.x) * -155f * flight);
                    float squash = Mathf.Sin(Mathf.Clamp01(t / 0.18f) * Mathf.PI);
                    snail.transform.localScale = Vector3.Scale(snailScale,
                        new Vector3(1f + 0.25f * squash, 1f - 0.35f * squash, 1f));
                    Color tint = Color.Lerp(new Color(1f, 0.45f, 0.35f), new Color(0.65f, 0.81f, 0.75f), Mathf.Clamp01(t * 3));
                    tint.a = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1f, t));
                    snail.color = tint;
                    float burst = Mathf.Clamp01((time - 0.23f) / 0.46f);
                    for (int i = 0; i < sparks.Length; i++)
                    {
                        SpriteRenderer spark = sparks[i];
                        spark.enabled = burst < 1f;
                        float angle = i * Mathf.PI * 2 / sparks.Length;
                        Vector3 radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                        spark.transform.position = origin + radial * size * Mathf.Lerp(0.1f, 0.85f, burst);
                        spark.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                        spark.transform.localScale = new Vector3(0.13f, 0.04f, 1) * size * (1f - burst);
                        spark.color = new Color(1f, i % 2 == 0 ? 0.85f : 0.42f, 0.38f, 1f - burst);
                    }
                }
                yield return null;
            }
        }
        finally
        {
            if (attacker != null) attacker.enabled = true;
            Object.Destroy(root);
            Object.Destroy(pixel);
        }
    }

    private static SpriteRenderer Copy(SpriteRenderer source, Transform parent, string name)
    {
        SpriteRenderer copy = new GameObject(name).AddComponent<SpriteRenderer>();
        copy.transform.SetParent(parent, false);
        copy.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        copy.transform.localScale = source.transform.lossyScale;
        copy.sprite = source.sprite;
        copy.sharedMaterial = source.sharedMaterial;
        copy.color = source.color;
        copy.flipX = source.flipX;
        copy.flipY = source.flipY;
        copy.sortingLayerID = source.sortingLayerID;
        copy.sortingOrder = source.sortingOrder + 2;
        return copy;
    }
}
