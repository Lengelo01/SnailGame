using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FrogProjectile : MonoBehaviour
{
    public Sprite[] frames;
    [Min(1)] public float framesPerSecond = 12;
    [Min(0.01f)] public float hitRadius = 0.12f;
    [Min(0.1f)] public float lifetime = 6;
    private Vector2 direction;
    private float speed;
    private float age;
    private Transform owner;
    private GameOverManager manager;
    private Sprite portrait;
    private SpriteRenderer visual;
    private bool reflected;
    private bool piercesShell;

    public void Launch(Vector2 travel, float travelSpeed, Transform source, GameOverManager gameOver, Sprite frog, bool unblockable = false)
    {
        direction = travel.normalized;
        reflected = false;
        piercesShell = unblockable;
        age = 0;
        speed = travelSpeed;
        owner = source;
        manager = gameOver;
        portrait = frog;
        visual = GetComponent<SpriteRenderer>();
        visual.flipX = direction.x < 0;
        if (piercesShell)
        {
            visual.color = new Color(1f, 0.45f, 0.35f, 1f);
            transform.localScale *= 1.2f;
        }
    }

    private void Update()
    {
        if (visual == null || Time.deltaTime == 0) return;
        age += Time.deltaTime;
        if (age >= lifetime) { Destroy(gameObject); return; }
        if (frames != null && frames.Length > 0)
            visual.sprite = frames[(int)(age * framesPerSecond) % frames.Length];
    }

    private void FixedUpdate()
    {
        if (visual == null) return;
        float distance = speed * Time.fixedDeltaTime;
        // Sweep the entire step so fast shots cannot skip thin walls or the snail.
        var hits = Physics2D.CircleCastAll(transform.position, hitRadius, direction, distance);
        foreach (var hit in hits)
        {
            var target = hit.collider;
            if (target == null) continue;
            if (!reflected && owner != null && target.transform.IsChildOf(owner)) continue;
            var player = target.GetComponentInParent<PlayerMovement>();
            if (player != null)
            {
                // Returned shots are friendly; ignore the shell's remaining overlap.
                if (reflected) continue;
                if (player.IsInShell && !piercesShell)
                {
                    transform.position = hit.centroid;
                    direction = -direction;
                    reflected = true;
                    age = 0;
                    visual.flipX = direction.x < 0;
                    return;
                }
                if (manager != null) manager.PlayHazardDefeat(player.transform, portrait,
                    piercesShell ? DefeatCause.ArmoredFrog : DefeatCause.Frog);
                Destroy(gameObject);
                return;
            }
            var frog = target.GetComponentInParent<FrogShooter>();
            if (reflected && frog != null)
            {
                frog.TakeReflectedHit();
                Destroy(gameObject);
                return;
            }
            if (target.isTrigger) continue;
            Destroy(gameObject);
            return;
        }
        transform.position += (Vector3)(direction * distance);
    }
}
