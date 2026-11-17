
using UnityEngine;

public class SpikeDamage : MonoBehaviour
{
    public SpikesController spikesController;
    public GameOverManager gameOverManager;

    private void Awake()
    {
        if (spikesController == null) spikesController = GetComponentInParent<SpikesController>();
        if (gameOverManager == null) gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerMovement player =
            other.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        if (spikesController != null && gameOverManager != null &&
            spikesController.IsDangerous && !player.IsInShell)
        {
            gameOverManager.PlaySpikeDefeat(player.transform, spikesController.transform);
        }
    }
}
