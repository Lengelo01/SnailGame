
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cabbage : MonoBehaviour
{
    private static Cabbage activeMeal;

    public static bool IsFinishingLevel => activeMeal != null;

    private bool eating;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (eating || IsFinishingLevel ||
            !other.CompareTag("Player"))
            return;

        var manager = FindFirstObjectByType<GameOverManager>();

        if (manager != null && manager.IsGameOver)
            return;

        var player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        eating = true;
        activeMeal = this;

        StartCoroutine(EatThenFinish(player));
    }

    private IEnumerator EatThenFinish(PlayerMovement player)
    {
        yield return CabbageEatingAnimation.Play(
            player,
            GetComponentInChildren<SpriteRenderer>()
        );

        string sceneName = SceneManager.GetActiveScene().name;

        int currentLevel = int.Parse(
            sceneName.Replace("Level", "")
        );

        // Poslednji level - zavrsetak igre!
        if (currentLevel == 4)
        {
            if (GameStatsManager.Instance != null)
            {
                GameStatsManager.Instance.FinishRun();
            }

            Time.timeScale = 1f;

            SceneManager.LoadScene("VictoryScene");
            yield break;
        }

        // Postojeci sistem otkljucavanja levela
        int nextLevel = currentLevel + 1;

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (nextLevel > unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextLevel);
            PlayerPrefs.Save();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelect");
    }

    private void OnDisable()
    {
        if (activeMeal == this)
            activeMeal = null;
    }
}
