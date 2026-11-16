
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    public Sprite openDoorSprite;
    public float unlockDistance = 1.5f;

    [Header("Door passage animation")]
    public bool animatePassage = true;
    [Range(0f, 0.5f)] public float passageShrink = 0.22f;
    [Range(0f, 1f)] public float passageDarkness = 0.65f;
    [Range(0.05f, 0.4f)] public float hiddenCenter = 0.18f;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D doorCollider;
    private bool isUnlocked = false;
    private SpriteRenderer playerRenderer;
    private SpriteRenderer passageRenderer;
    private bool hidingPlayer;
    private bool originalForceRenderingOff;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerRenderer = playerObject.GetComponent<SpriteRenderer>();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (isUnlocked || player == null)
            return;

        PlayerKey playerKey = player.GetComponent<PlayerKey>();

        if (playerKey == null || !playerKey.hasKey)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance <= unlockDistance)
        {
            UnlockDoor();
        }
    }

    void UnlockDoor()
    {
        isUnlocked = true;

        spriteRenderer.sprite = openDoorSprite;

        doorCollider.enabled = false;
    }

    void LateUpdate()
    {
        if (!animatePassage || !isUnlocked || playerRenderer == null ||
            spriteRenderer == null || !playerRenderer.enabled || !player.gameObject.activeInHierarchy)
        {
            RestorePlayerVisual();
            return;
        }

        // Evaluate the actual position, so stopping, reversing or pausing also
        // stops/reverses the effect. Never move the player or change its collider.
        Bounds doorway = spriteRenderer.bounds;
        Vector2 offset = playerRenderer.bounds.center - doorway.center;
        float width = Mathf.Max(0.01f, doorway.extents.x);
        float height = Mathf.Max(0.01f, doorway.extents.y);
        float distance = new Vector2(offset.x / width, offset.y / height).magnitude;

        if (distance >= 1f)
        {
            RestorePlayerVisual();
            return;
        }

        if (passageRenderer == null)
        {
            GameObject visual = new GameObject("Door passage visual");
            visual.layer = player.gameObject.layer;
            visual.transform.SetParent(playerRenderer.transform, false);
            passageRenderer = visual.AddComponent<SpriteRenderer>();
        }

        if (!hidingPlayer)
        {
            originalForceRenderingOff = playerRenderer.forceRenderingOff;
            hidingPlayer = true;
            playerRenderer.forceRenderingOff = true;
        }

        float depth = 1f - Mathf.SmoothStep(0f, 1f,
            Mathf.InverseLerp(hiddenCenter, 1f, distance));

        // A separate visual follows every walking-animation frame. Scaling it
        // cannot shrink the player's physics body, change speed or move the camera.
        passageRenderer.sprite = playerRenderer.sprite;
        passageRenderer.sharedMaterial = playerRenderer.sharedMaterial;
        passageRenderer.flipX = playerRenderer.flipX;
        passageRenderer.flipY = playerRenderer.flipY;
        passageRenderer.sortingLayerID = playerRenderer.sortingLayerID;
        passageRenderer.sortingOrder = playerRenderer.sortingOrder;
        passageRenderer.spriteSortPoint = playerRenderer.spriteSortPoint;
        passageRenderer.maskInteraction = playerRenderer.maskInteraction;
        passageRenderer.enabled = !originalForceRenderingOff;
        passageRenderer.transform.localScale = Vector3.one * (1f - passageShrink * depth);

        Color color = playerRenderer.color;
        float light = 1f - passageDarkness * depth;
        passageRenderer.color = new Color(color.r * light, color.g * light,
            color.b * light, color.a * (1f - depth));
    }

    private void RestorePlayerVisual()
    {
        if (hidingPlayer && playerRenderer != null)
            playerRenderer.forceRenderingOff = originalForceRenderingOff;

        hidingPlayer = false;
        if (passageRenderer != null)
            passageRenderer.enabled = false;
    }

    void OnDisable()
    {
        RestorePlayerVisual();
    }

    void OnDestroy()
    {
        RestorePlayerVisual();
        if (passageRenderer != null)
            Destroy(passageRenderer.gameObject);
    }
}
