
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStatsManager : MonoBehaviour
{
    public static GameStatsManager Instance { get; private set; }

    public float TotalTime { get; private set; }
    public int TotalDeaths { get; private set; }
    public float Level4AttemptTime { get; private set; }

    private bool isRunning;
    private bool isPaused;
    private bool inLevel;
    private int level4StartFrame = -1;

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
        TrackScene(SceneManager.GetActiveScene());
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
        TrackScene(scene);
        if (scene.name != "VictoryScene") return;

        TMP_Text deaths = GameObject.Find("DeathsValue")?.GetComponent<TMP_Text>();
        TMP_Text time = GameObject.Find("TimeValue")?.GetComponent<TMP_Text>();
        TMP_Text level4Time = GameObject.Find("Level4TimeValue")?.GetComponent<TMP_Text>();
        if (deaths != null) deaths.text = TotalDeaths.ToString();
        if (time != null) time.text = GetFormattedTime();
        if (level4Time != null) level4Time.text = FormatTime(Level4AttemptTime);
    }

    private void TrackScene(Scene scene)
    {
        inLevel = FindFirstObjectByType<LevelManager>() != null;
        level4StartFrame = scene.name == "Level4" ? Time.frameCount : -1;
        if (level4StartFrame >= 0)
            Level4AttemptTime = 0f;
    }

    private void Update()
    {
        if (inLevel && isRunning && !isPaused && Time.timeScale > 0f)
        {
            TotalTime += Time.unscaledDeltaTime;
            // The loading frame can include time spent in the previous scene.
            if (level4StartFrame >= 0 && Time.frameCount > level4StartFrame && !Cabbage.IsFinishingLevel)
                Level4AttemptTime += Time.unscaledDeltaTime;
        }
    }

    public void StartRun()
    {
        TotalTime = 0f;
        TotalDeaths = 0;
        Level4AttemptTime = 0f;
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
        return FormatTime(TotalTime);
    }

    private static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int hundredths = Mathf.FloorToInt((time % 1f) * 100f);
        return $"{minutes:00}:{seconds:00}.{hundredths:00}";
    }
}
