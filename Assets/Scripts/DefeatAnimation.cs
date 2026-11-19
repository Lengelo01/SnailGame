using System.Collections;
using UnityEngine;

// Runs with unscaled time while the game is frozen. Only visual stand-ins move;
// real actors, collisions, map bounds and the camera target remain untouched.
public static class DefeatAnimation
{
    public static IEnumerator PlayHazard(SpriteRenderer victim, bool fire)
    {
        var root = new GameObject(fire ? "Fire defeat cinematic" : "Slime defeat cinematic");
        var snail = Copy(victim, root.transform, fire ? "Charred snail" : "Sticky snail");
        snail.sortingOrder += 20;
        Vector3 origin = snail.transform.position;
        Vector3 scale = snail.transform.localScale;
        float size = Mathf.Max(victim.bounds.size.x, victim.bounds.size.y, 0.3f);
        victim.enabled = false;
        Sprite pixel = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            Vector2.one * 0.5f, Texture2D.whiteTexture.width);
        var particles = new SpriteRenderer[24];
        for (int i = 0; i < particles.Length; i++)
        {
            var part = new GameObject(fire ? "Ember or smoke" : "Slime splash and drip").AddComponent<SpriteRenderer>();
            part.transform.SetParent(root.transform);
            part.sprite = pixel;
            part.sortingLayerID = snail.sortingLayerID;
            part.sortingOrder = snail.sortingOrder + 2;
            particles[i] = part;
        }
        var puddle = new GameObject(fire ? "Ash pile" : "Slime puddle").AddComponent<SpriteRenderer>();
        puddle.transform.SetParent(root.transform);
        puddle.sprite = pixel;
        puddle.sortingLayerID = snail.sortingLayerID;
        puddle.sortingOrder = snail.sortingOrder - 1;
        puddle.transform.position = origin + Vector3.down * size * 0.3f;
        float elapsed = 0;
        try
        {
            while (elapsed < 1.8f)
            {
                elapsed += Time.unscaledDeltaTime;
                float settle = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.2f, 1.3f, elapsed));
                float fade = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.35f, 1.8f, elapsed));
                float impact = Mathf.Sin(Mathf.Clamp01(elapsed / 0.2f) * Mathf.PI);
                float struggle = Mathf.Sin(elapsed * (fire ? 75 : 40)) * (1 - settle);
                snail.transform.position = origin + Vector3.right * (struggle * size * 0.045f)
                    + Vector3.down * size * settle * (fire ? 0.2f : 0.14f);
                snail.transform.localScale = Vector3.Scale(scale, new Vector3(
                    1 + impact * 0.2f + settle * 0.18f,
                    1 - impact * 0.22f - settle * (fire ? 0.75f : 0.4f), 1));
                Color tint = fire
                    ? Color.Lerp(new Color(1f,0.45f,0.12f), new Color(0.13f,0.1f,0.09f), Mathf.Clamp01(elapsed / 0.65f))
                    : Color.Lerp(victim.color, new Color(0.38f,0.85f,0.08f), Mathf.Clamp01(elapsed / 0.3f));
                tint.a = fade;
                snail.color = tint;
                puddle.transform.localScale = new Vector3(size * Mathf.Lerp(0.2f,1.05f,settle),size * 0.13f * settle,1);
                puddle.color = fire ? new Color(0.17f,0.13f,0.1f,fade) : new Color(0.28f,0.65f,0.04f,fade * 0.9f);
                for (int i = 0; i < particles.Length; i++)
                {
                    var part = particles[i];
                    if (fire)
                    {
                        bool smoke = i >= 12;
                        float t = Mathf.Clamp01((elapsed - (i % 12) * 0.035f - (smoke ? 0.35f : 0)) / (smoke ? 1.1f : 0.65f));
                        part.enabled = t > 0 && t < 1;
                        float side = ((i * 7) % 11 - 5) / 5f;
                        part.transform.position = origin + new Vector3(side * size * (0.1f + t * 0.5f),
                            size * (smoke ? t * 1.2f : Mathf.Sin(t * Mathf.PI) * 0.7f),0);
                        float width = size * (smoke ? Mathf.Lerp(0.07f,0.23f,t) : 0.055f * (1-t));
                        part.transform.localScale = new Vector3(width,width,1);
                        part.color = smoke ? new Color(0.4f,0.39f,0.36f,(1-t)*0.7f*fade)
                            : new Color(1, i%2==0 ? 0.72f : 0.3f,0.05f,(1-t)*fade);
                    }
                    else
                    {
                        bool drip = i >= 12;
                        float t = Mathf.Clamp01((elapsed - (drip ? 0.12f + (i%4)*0.05f : 0)) / (drip ? 1.3f : 0.55f));
                        float side = ((i * 7) % 11 - 5) / 5f;
                        float angle = i * Mathf.PI * 2 / 12;
                        part.enabled = t > 0 && (drip || t < 1);
                        part.transform.position = drip
                            ? origin + new Vector3(side*size*0.36f, size*(0.22f - t*0.48f - (i%3)*0.08f),0)
                            : origin + new Vector3(Mathf.Cos(angle)*size*t*0.8f,Mathf.Sin(angle)*size*t*0.65f - size*t*t*0.35f,0);
                        part.transform.localScale = drip
                            ? new Vector3(size*0.075f,size*(0.08f+Mathf.Sin(t*Mathf.PI)*0.28f),1)
                            : Vector3.one * size * 0.09f * (1-t);
                        part.color = i%3==0 ? new Color(0.78f,1,0.22f,fade*0.9f) : new Color(0.32f,0.73f,0.03f,fade*0.85f);
                    }
                }
                yield return null;
            }
        }
        finally
        {
            Object.Destroy(root);
            Object.Destroy(pixel);
        }
    }

    public static IEnumerator PlaySpider(SpriteRenderer victim, SpriteRenderer attacker)
    {
        var root = new GameObject("Spider defeat cinematic");
        SpriteRenderer snail = Copy(victim, root.transform, "Snail caught in web");
        SpriteRenderer spider = Copy(attacker, root.transform, "Spider pounce");
        bool attackerWasEnabled = attacker.enabled;
        victim.enabled = false;
        attacker.enabled = false;
        Vector3 origin = snail.transform.position;
        Vector3 start = spider.transform.position;
        Vector3 scale = snail.transform.localScale;
        Vector3 spiderScale = spider.transform.localScale;
        Vector3 direction = origin - start;
        direction.z = 0;
        direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.down;
        float size = Mathf.Max(victim.bounds.size.x, victim.bounds.size.y, 0.3f);
        Sprite pixel = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            Vector2.one * 0.5f, Texture2D.whiteTexture.width);
        var threads = new SpriteRenderer[8];
        for (int i = 0; i < threads.Length; i++)
        {
            var thread = new GameObject("Silk strand").AddComponent<SpriteRenderer>();
            thread.transform.SetParent(root.transform);
            thread.sprite = pixel;
            thread.sortingLayerID = victim.sortingLayerID;
            thread.sortingOrder = Mathf.Max(victim.sortingOrder, attacker.sortingOrder) + 5;
            thread.enabled = false;
            threads[i] = thread;
        }
        float elapsed = 0;
        try
        {
            while (elapsed < 1.65f)
            {
                elapsed += Time.unscaledDeltaTime;
                float crouch = Mathf.Sin(Mathf.Clamp01(elapsed / 0.18f) * Mathf.PI);
                float pounce = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.18f, 0.38f, elapsed));
                float retreat = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.65f, 1.15f, elapsed));
                spider.transform.position = Vector3.Lerp(Vector3.Lerp(start, origin - direction * size * 0.25f, pounce), start, retreat);
                spider.transform.localScale = Vector3.Scale(spiderScale, new Vector3(1 + crouch * 0.18f, 1 - crouch * 0.2f, 1));
                float caught = Mathf.InverseLerp(0.38f, 1.4f, elapsed);
                float fade = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.65f, 1, caught));
                snail.transform.position = origin + Vector3.right * (Mathf.Sin(elapsed * 65) * size * 0.035f * (1 - caught));
                snail.transform.localScale = scale * Mathf.Lerp(1, 0.6f, caught);
                Color tint = Color.Lerp(victim.color, new Color(0.61f, 0.5f, 0.77f), Mathf.Clamp01(caught * 3));
                tint.a = fade;
                snail.color = tint;
                for (int i = 0; i < threads.Length; i++)
                {
                    float wrap = Mathf.Clamp01((elapsed - 0.4f - i * 0.045f) / 0.18f);
                    var thread = threads[i];
                    thread.enabled = wrap > 0;
                    thread.transform.position = snail.transform.position + Vector3.up * ((i / 2 - 1.5f) * size * 0.14f);
                    thread.transform.rotation = Quaternion.Euler(0, 0, i % 2 == 0 ? 28 : -28);
                    thread.transform.localScale = new Vector3(size * 0.85f * wrap * Mathf.Lerp(1, 0.6f, caught), size * 0.035f, 1);
                    thread.color = new Color(0.87f, 0.94f, 0.91f, fade * 0.85f);
                }
                yield return null;
            }
        }
        finally
        {
            if (attacker != null) attacker.enabled = attackerWasEnabled;
            Object.Destroy(root);
            Object.Destroy(pixel);
        }
    }

    public static IEnumerator PlaySpike(SpriteRenderer victim)
    {
        var root = new GameObject("Spike defeat cinematic");
        SpriteRenderer snail = Copy(victim, root.transform, "Snail spike recoil");
        victim.enabled = false;
        Vector3 origin = snail.transform.position;
        Vector3 scale = snail.transform.localScale;
        Quaternion rotation = snail.transform.rotation;
        float size = Mathf.Max(victim.bounds.size.x, victim.bounds.size.y, 0.3f);
        float elapsed = 0f;
        try
        {
            while (elapsed < 1.25f)
            {
                elapsed += Time.unscaledDeltaTime;
                float flight = Mathf.Clamp01((elapsed - 0.12f) / 0.85f);
                float impact = Mathf.Sin(Mathf.Clamp01(elapsed / 0.12f) * Mathf.PI);
                snail.transform.position = origin + Vector3.up * (Mathf.Sin(flight * Mathf.PI) * size * 0.8f)
                    + Vector3.right * (Mathf.Sin(elapsed * 95f) * size * 0.035f * (1f - flight));
                snail.transform.rotation = rotation * Quaternion.Euler(0, 0, -165f * Mathf.SmoothStep(0, 1, flight));
                snail.transform.localScale = Vector3.Scale(scale,
                    new Vector3(1f + impact * 0.25f, 1f - impact * 0.3f, 1f));
                Color color = Color.Lerp(new Color(1f, 0.35f, 0.28f), new Color(0.65f, 0.78f, 0.73f), flight);
                color.a = 1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.55f, 1f, flight));
                snail.color = color;
                yield return null;
            }
        }
        finally { Object.Destroy(root); }
    }

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
