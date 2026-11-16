#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class DefeatPreview
{
    [MenuItem("SnailTask/Preview pause in Play Mode %#j")]
    private static void PreviewPause()
    {
        if (!Application.isPlaying) return;
        PauseMenu pause = Object.FindFirstObjectByType<PauseMenu>();
        if (pause != null) pause.PauseGame();
    }

    [MenuItem("SnailTask/Preview defeat in Play Mode %#k")]
    private static void Preview()
    {
        if (!Application.isPlaying) { Debug.Log("Enter Play Mode before previewing defeat."); return; }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        EnemyKill enemy = Object.FindFirstObjectByType<EnemyKill>();
        GameOverManager manager = Object.FindFirstObjectByType<GameOverManager>();
        if (player == null || enemy == null || manager == null || manager.IsGameOver) return;
        enemy.transform.position = player.transform.position + Vector3.right * 0.65f;
        // Exercise the same contact handler used in normal gameplay.
        enemy.SendMessage("OnTriggerEnter2D", player.GetComponent<Collider2D>());
    }
}
#endif
