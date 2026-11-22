
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public enum DefeatCause
{
    Ladybug,
    Spike,
    Spider,
    Fire,
    Frog,
    ArmoredFrog
}

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
            view = new GameOverView(
                gameOverCanvas,
                RestartLevel,
                GoToLevelSelect
            );

            gameOverCanvas.SetActive(false);
        }
    }

    void Update()
    {
        if (!panelReady || Keyboard.current == null)
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            RestartLevel();
        }
    }

    // Obican Game Over
    public void ShowGameOver(
        Sprite portrait = null,
        DefeatCause cause = DefeatCause.Ladybug)
    {
        if (IsGameOver || Cabbage.IsFinishingLevel)
            return;

        IsGameOver = true;

        RegisterDeath();

        Time.timeScale = 0f;

        StartCoroutine(RevealPanel(portrait, cause));
    }

    // Smrt od neprijatelja
    public void PlayDefeat(
        Transform player,
        Transform enemy,
        DefeatCause cause = DefeatCause.Ladybug)
    {
        if (IsGameOver || Cabbage.IsFinishingLevel)
            return;

        IsGameOver = true;

        RegisterDeath();

        Time.timeScale = 0f;

        StartCoroutine(
            DefeatThenPanel(player, enemy, cause)
        );
    }

    // Smrt od bodlji
    public void PlaySpikeDefeat(
        Transform player,
        Transform spikes)
    {
        PlayDefeat(
            player,
            spikes,
            DefeatCause.Spike
        );
    }

    // Smrt od vatre i drugih zamki
    public void PlayHazardDefeat(
        Transform player,
        Sprite portrait,
        DefeatCause cause)
    {
        if (IsGameOver || Cabbage.IsFinishingLevel)
            return;

        IsGameOver = true;

        RegisterDeath();

        Time.timeScale = 0f;

        StartCoroutine(
            HazardThenPanel(player, portrait, cause)
        );
    }

    // NOVO: Brojanje smrti
    private void RegisterDeath()
    {
        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.AddDeath();
        }
    }

    private IEnumerator HazardThenPanel(
        Transform player,
        Sprite portrait,
        DefeatCause cause)
    {
        var victim = player != null
            ? player.GetComponentInChildren<SpriteRenderer>()
            : null;

        if (victim != null)
        {
            yield return DefeatAnimation.PlayHazard(
                victim,
                cause == DefeatCause.Fire
            );
        }

        yield return RevealPanel(portrait, cause);
    }

    private IEnumerator DefeatThenPanel(
        Transform player,
        Transform enemy,
        DefeatCause cause)
    {
        SpriteRenderer victim = player != null
            ? player.GetComponentInChildren<SpriteRenderer>()
            : null;

        SpriteRenderer attacker = enemy != null
            ? enemy.GetComponentInChildren<SpriteRenderer>()
            : null;

        Sprite portrait = attacker != null
            ? attacker.sprite
            : null;

        if (victim != null && attacker != null)
        {
            if (cause == DefeatCause.Spike)
            {
                yield return DefeatAnimation.PlaySpike(victim);
            }
            else if (cause == DefeatCause.Spider)
            {
                yield return DefeatAnimation.PlaySpider(
                    victim,
                    attacker
                );
            }
            else
            {
                yield return DefeatAnimation.Play(
                    victim,
                    attacker
                );
            }
        }

        yield return RevealPanel(portrait, cause);
    }

    private IEnumerator RevealPanel(
        Sprite portrait,
        DefeatCause cause = DefeatCause.Ladybug)
    {
        if (view == null)
            yield break;

        gameOverCanvas.SetActive(true);

        view.SetPortrait(portrait);
        view.SetCause(cause);
        view.SetReveal(0f);

        float elapsed = 0f;

        while (elapsed < 0.38f)
        {
            elapsed += Time.unscaledDeltaTime;

            view.SetReveal(
                Mathf.Clamp01(elapsed / 0.38f)
            );

            yield return null;
        }

        view.SetReveal(1f);

        panelReady = true;

        view.FocusRetry();
    }

    void LateUpdate()
    {
        if (panelReady)
        {
            view?.FitToScreen();
        }
    }

    public void RestartLevel()
    {
        if (leaving)
            return;

        leaving = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void GoToLevelSelect()
    {
        if (leaving)
            return;

        leaving = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene("LevelSelect");
    }
}
