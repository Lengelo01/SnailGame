using UnityEngine;
using UnityEngine.SceneManagement;

// UI objects and button events are authored in MainMenu.unity.
public class MainMenuManager : MonoBehaviour
{
    private bool loading;
    void Start() { Time.timeScale = 1f; }

    public void PlayGame()
    {
        if (loading) return;
        loading = true;
        Time.timeScale = 1f;
        if (GameStatsManager.Instance != null)
            GameStatsManager.Instance.StartRun();
        SceneManager.LoadScene("LevelSelect");
    }

    public void GoToMainMenu()
    {
        if (loading) return;
        loading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
