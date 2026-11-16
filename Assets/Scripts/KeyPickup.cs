
using UnityEngine;
using UnityEngine.UI;

public class KeyPickup : MonoBehaviour
{
    public GameObject keyIcon;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!collected && other.CompareTag("Player"))
        {
            PlayerKey playerKey = other.GetComponentInParent<PlayerKey>();
            if (playerKey == null)
                return;

            collected = true;
            playerKey.hasKey = true;

            if (keyIcon != null)
            {
                keyIcon.SetActive(true);
                Image icon = keyIcon.GetComponent<Image>();
                if (icon != null)
                    icon.color = Color.white;
            }

            Destroy(gameObject);
        }
    }
}
