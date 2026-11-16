
using UnityEngine;

public class EnemyKill : MonoBehaviour
{
    public GameOverManager gameOverManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && gameOverManager != null)
        {
            PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
            gameOverManager.PlayDefeat(player != null ? player.transform : other.transform, transform);
        }
    }
}
