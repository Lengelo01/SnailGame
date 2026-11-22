
using UnityEngine;
using UnityEngine.UI;

public class KeyPickup : MonoBehaviour
{
    [Header("Level 3 key settings")]
    public bool useColoredKeys = false;
    public KeyType keyType = KeyType.Yellow;

    [Header("Inventory icon")]
    public GameObject keyIcon;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        PlayerKey playerKey =
            other.GetComponentInParent<PlayerKey>();

        if (playerKey == null)
            return;

        collected = true;

        if (useColoredKeys)
        {
            playerKey.CollectKey(keyType);
        }
        else
        {
            // Originalno ponašanje Levela 2.
            playerKey.hasKey = true;
        }

        if (keyIcon != null)
        {
            keyIcon.SetActive(true);

            Image icon = keyIcon.GetComponent<Image>();

            if (icon != null)
                icon.color = Color.white;
        }

        GameSfx.Play(GameSfx.Cue.KeyPickup, transform.position);
        Destroy(gameObject);
    }
}
