
using UnityEngine;

public class EnemyKill : MonoBehaviour
{
    public GameOverManager gameOverManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && gameOverManager != null)
        {
            PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
            SpiderChase spider = GetComponentInParent<SpiderChase>();
            gameOverManager.PlayDefeat(player != null ? player.transform : other.transform,
                spider != null ? spider.transform : transform,
                spider != null ? DefeatCause.Spider : DefeatCause.Ladybug);
        }
    }
}
