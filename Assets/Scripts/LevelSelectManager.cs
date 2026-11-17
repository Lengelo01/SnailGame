
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public Button[] levelButtons;

    void Start()
    {
        RefreshCards();
    }

    public void RefreshCards()
    {
        int unlockLevel = Mathf.Max(1, PlayerPrefs.GetInt("UnlockedLevel", 1));

        for (int i = 0; i < levelButtons.Length; i++)
        {
            Button button = levelButtons[i];
            if (button == null) continue;
            int level = i + 1;
            bool exists = Application.CanStreamedLevelBeLoaded("Level" + level);
            bool unlocked = level <= unlockLevel;
            LevelCardState card = button.GetComponent<LevelCardState>();
            if (card != null) card.Show(exists, unlocked, level == unlockLevel);
            else button.interactable = exists && unlocked;
        }
    }

    public void OpenLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > PlayerPrefs.GetInt("UnlockedLevel", 1) ||
            !Application.CanStreamedLevelBeLoaded("Level" + levelNumber)) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level" + levelNumber);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
