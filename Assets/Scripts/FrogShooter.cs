using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FrogShooter : MonoBehaviour
{
    public bool shootLeft = true;
    [Tooltip("Armored mini-boss: immune to returned spit; its shots bypass the shell.")]
    public bool isArmoredBoss = false;
    [Min(0.1f)] public float idleTime = 2.4f;
    [Min(1)] public float framesPerSecond = 10;
    [Min(0.1f)] public float projectileSpeed = 4;
    public Vector2 mouthOffset = new Vector2(0.57f, -0.03f);
    public Sprite[] idleFrames;
    public Sprite[] spitFrames;
    public FrogProjectile projectilePrefab;
    public GameOverManager gameOverManager;
    private SpriteRenderer body;
    public int RemainingHits { get; private set; } = 2;
    public bool IsDead { get; private set; }
    private Coroutine damageFlash;
    private Color normalColor;

    private void Awake()
    {
        body = GetComponent<SpriteRenderer>();
        normalColor = body.color;
        if (gameOverManager == null) gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    private void OnEnable() { if (!IsDead) StartCoroutine(Cycle()); }

    private void OnCollisionEnter2D(Collision2D collision) { TouchPlayer(collision.collider); }
    private void OnCollisionStay2D(Collision2D collision) { TouchPlayer(collision.collider); }
    private void OnTriggerEnter2D(Collider2D other) { TouchPlayer(other); }
    private void OnTriggerStay2D(Collider2D other) { TouchPlayer(other); }

    private void TouchPlayer(Collider2D other)
    {
        if (IsDead || gameOverManager == null) return;
        var player = other.GetComponentInParent<PlayerMovement>();
        if (player == null || !player.CompareTag("Player")) return;
        // The shell reflects spit, but direct contact with a living frog is lethal.
        gameOverManager.PlayHazardDefeat(player.transform, body.sprite,
            isArmoredBoss ? DefeatCause.ArmoredFrog : DefeatCause.Frog);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        damageFlash = null;
        if (body != null) body.color = normalColor;
    }

    public void TakeReflectedHit()
    {
        if (IsDead || isArmoredBoss) return;
        RemainingHits--;
        if (RemainingHits <= 0)
        {
            IsDead = true;
            StopAllCoroutines();
            foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
            StartCoroutine(Die());
        }
        else
        {
            if (damageFlash != null) StopCoroutine(damageFlash);
            damageFlash = StartCoroutine(FlashDamage());
        }
    }

    private IEnumerator FlashDamage()
    {
        body.color = new Color(1f, 0.18f, 0.18f, normalColor.a);
        yield return new WaitForSeconds(0.25f);
        body.color = normalColor;
        damageFlash = null;
    }

    private IEnumerator Die()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0;
        while (elapsed < 0.4f)
        {
            float t = elapsed / 0.4f;
            body.color = new Color(1f, 0.18f, 0.18f, (1f - t) * normalColor.a);
            transform.localScale = originalScale * Mathf.Lerp(1f, 0.15f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    private IEnumerator Cycle()
    {
        while (true)
        {
            float elapsed = 0;
            while (elapsed < idleTime)
            {
                body.flipX = !shootLeft;
                if (idleFrames != null && idleFrames.Length > 0)
                    body.sprite = idleFrames[(int)(elapsed * framesPerSecond) % idleFrames.Length];
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (spitFrames == null || spitFrames.Length == 0) { yield return null; continue; }
            for (int i = 0; i < spitFrames.Length; i++)
            {
                body.flipX = !shootLeft;
                body.sprite = spitFrames[i];
                if (i == 4) Fire();
                yield return new WaitForSeconds(1f / framesPerSecond);
            }
        }
    }

    public void Fire()
    {
        if (IsDead || projectilePrefab == null || (gameOverManager != null && gameOverManager.IsGameOver)) return;
        Vector2 direction = shootLeft ? Vector2.left : Vector2.right;
        Vector3 mouth = transform.TransformPoint(new Vector3(mouthOffset.x * direction.x, mouthOffset.y, 0));
        var shot = Instantiate(projectilePrefab, mouth, Quaternion.identity);
        shot.Launch(direction, projectileSpeed, transform, gameOverManager, body.sprite, isArmoredBoss);
        GameSfx.Play(GameSfx.Cue.FrogSpit, mouth);
    }
}
