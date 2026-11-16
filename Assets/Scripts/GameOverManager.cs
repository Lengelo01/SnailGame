using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverCanvas;
    public bool IsGameOver { get; private set; }
    private bool panelReady;
    private bool leaving;
    private GameOverView view;

    void Awake()
    {
        if (gameOverCanvas != null)
        {
            view = new GameOverView(gameOverCanvas, RestartLevel, GoToLevelSelect);
            gameOverCanvas.SetActive(false);
        }
    }

    void Update()
    {
        if (!panelReady || Keyboard.current == null) return;
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            RestartLevel();
    }

    public void ShowGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;
        StartCoroutine(RevealPanel(null));
    }

    public void PlayDefeat(Transform player, Transform enemy)
    {
        if (IsGameOver) return;
        IsGameOver = true; // Latch before the first yield: multiple colliders cannot replay death.
        Time.timeScale = 0f;
        StartCoroutine(DefeatThenPanel(player, enemy));
    }

    private IEnumerator DefeatThenPanel(Transform player, Transform enemy)
    {
        SpriteRenderer victim = player != null ? player.GetComponentInChildren<SpriteRenderer>() : null;
        SpriteRenderer attacker = enemy != null ? enemy.GetComponentInChildren<SpriteRenderer>() : null;
        Sprite portrait = attacker != null ? attacker.sprite : null;
        if (victim != null && attacker != null)
            yield return DefeatAnimation.Play(victim, attacker);
        yield return RevealPanel(portrait);
    }

    private IEnumerator RevealPanel(Sprite portrait)
    {
        if (view == null) yield break;
        gameOverCanvas.SetActive(true);
        view.SetPortrait(portrait);
        view.SetReveal(0f);
        float elapsed = 0f;
        while (elapsed < 0.38f)
        {
            elapsed += Time.unscaledDeltaTime;
            view.SetReveal(Mathf.Clamp01(elapsed / 0.38f));
            yield return null;
        }
        view.SetReveal(1f);
        panelReady = true;
        view.FocusRetry();
    }

    void LateUpdate()
    {
        if (panelReady) view?.FitToScreen();
    }

    public void RestartLevel()
    {
        if (leaving) return;
        leaving = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToLevelSelect()
    {
        if (leaving) return;
        leaving = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelect");
    }
}
