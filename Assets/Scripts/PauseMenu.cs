using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;  

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseCanvas;

    private bool isPaused = false;
    private GameOverManager gameOverManager;
    private PauseMenuView view;

    void Start()
    {
        gameOverManager = FindFirstObjectByType<GameOverManager>();
        view = new PauseMenuView(pauseCanvas, new Vector2(560, 460), true);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SpriteRenderer snail = player != null ? player.GetComponent<SpriteRenderer>() : null;
        view.BuildPause(snail != null ? snail.sprite : null, ResumeGame, GoToLevelSelect);
        Time.timeScale = 1.0f;
        pauseCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Cabbage.IsFinishingLevel || (gameOverManager != null && gameOverManager.IsGameOver))
            return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();

            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (Cabbage.IsFinishingLevel || (gameOverManager != null && gameOverManager.IsGameOver)) return;
        pauseCanvas.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        view.Show();
    }

    public void ResumeGame()
    {
        pauseCanvas.SetActive(false);
        Time.timeScale = 1.0f;
        isPaused = false;
    }

    public void GoToLevelSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelect");
    }

    void LateUpdate()
    {
        if (isPaused) view?.Tick();
    }

}
