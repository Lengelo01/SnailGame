using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class FlameTrap : MonoBehaviour
{
    [Min(0.1f)] public float burningTime = 3.5f;
    [Min(0.1f)] public float restTime = 2.5f;
    [Min(0.01f)] public float ignitionTime = 0.4f;
    [Min(0.01f)] public float extinguishTime = 0.4f;
    public GameOverManager gameOverManager;
    public Sprite[] frames;
    public Rect[] damageRects;
    public bool IsDangerous { get; private set; }
    private Animator animator;
    private SpriteRenderer flame;
    private BoxCollider2D hitbox;
    private Coroutine cycle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        flame = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<BoxCollider2D>();
        hitbox.isTrigger = true;
        if (gameOverManager == null) gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    private void OnEnable() { cycle = StartCoroutine(Cycle()); }

    private IEnumerator Cycle()
    {
        while (true)
        {
            IsDangerous = false;
            flame.enabled = false;
            hitbox.enabled = false;
            yield return new WaitForSeconds(restTime);

            flame.enabled = true;
            IsDangerous = true;
            animator.Play("Ignite", 0, 0);
            animator.Update(0);
            UpdateHitbox();
            yield return new WaitForSeconds(ignitionTime);
            animator.Play("Burn", 0, 0);
            yield return new WaitForSeconds(burningTime);
            animator.Play("Extinguish", 0, 0);
            yield return new WaitForSeconds(extinguishTime);
        }
    }

    private void LateUpdate() { UpdateHitbox(); }

    private void UpdateHitbox()
    {
        hitbox.enabled = false;
        if (!IsDangerous || !flame.enabled || frames == null || damageRects == null) return;
        for (int i = 0; i < frames.Length && i < damageRects.Length; i++)
        {
            if (frames[i] != flame.sprite) continue;
            Rect rect = damageRects[i];
            hitbox.offset = rect.center;
            hitbox.size = rect.size;
            hitbox.enabled = rect.width > 0 && rect.height > 0;
            break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) { TryBurn(other); }
    private void OnTriggerStay2D(Collider2D other) { TryBurn(other); }
    private void TryBurn(Collider2D other)
    {
        if (!IsDangerous || !hitbox.enabled || gameOverManager == null) return;
        var player = other.GetComponentInParent<PlayerMovement>();
        if (player == null || !player.CompareTag("Player") || player.IsInShell) return;
        gameOverManager.PlayHazardDefeat(player.transform, flame.sprite, DefeatCause.Fire);
    }

    private void OnDisable()
    {
        if (cycle != null) StopCoroutine(cycle);
        IsDangerous = false;
        if (hitbox != null) hitbox.enabled = false;
        if (flame != null) flame.enabled = false;
    }
}
