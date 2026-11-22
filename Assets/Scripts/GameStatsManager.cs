
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStatsManager : MonoBehaviour
{
    public static GameStatsManager Instance { get; private set; }

    public float TotalTime { get; private set; }
    public int TotalDeaths { get; private set; }

    private bool isRunning;
    private bool isPaused;
    private bool inLevel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureStatsForStartingScene()
    {
        // Play Mode can start directly in a level that has no stats component.
        if (Instance == null)
            new GameObject(nameof(GameStatsManager)).AddComponent<GameStatsManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        inLevel = FindFirstObjectByType<LevelManager>() != null;
        StartRun();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;
        inLevel = FindFirstObjectByType<LevelManager>() != null;
        if (scene.name != "VictoryScene") return;

        TMP_Text deaths = GameObject.Find("DeathsValue")?.GetComponent<TMP_Text>();
        TMP_Text time = GameObject.Find("TimeValue")?.GetComponent<TMP_Text>();
        if (deaths != null) deaths.text = TotalDeaths.ToString();
        if (time != null) time.text = GetFormattedTime();
    }

    private void Update()
    {
        if (inLevel && isRunning && !isPaused && Time.timeScale > 0f)
        {
            TotalTime += Time.unscaledDeltaTime;
        }
    }

    public void StartRun()
    {
        TotalTime = 0f;
        TotalDeaths = 0;
        isRunning = true;
        isPaused = false;
    }

    public void AddDeath()
    {
        if (isRunning)
            TotalDeaths++;
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public void FinishRun()
    {
        isRunning = false;
    }

    public void ResumeRun()
    {
        isRunning = true;
        isPaused = false;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(TotalTime / 60f);
        int seconds = Mathf.FloorToInt(TotalTime % 60f);
        int milliseconds = Mathf.FloorToInt((TotalTime % 1f) * 100f);

        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
}
