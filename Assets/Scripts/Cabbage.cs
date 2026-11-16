
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cabbage : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            string sceneName = SceneManager.GetActiveScene().name;

            int currentLevel = int.Parse(
                sceneName.Replace("Level", "")
            );

            int nextLevel = currentLevel + 1;

            int unlockedLevel =
                PlayerPrefs.GetInt("UnlockedLevel", 1);

            if (nextLevel > unlockedLevel)
            {
                PlayerPrefs.SetInt("UnlockedLevel", nextLevel);
                PlayerPrefs.Save();
            }

            SceneManager.LoadScene("LevelSelect");
        }
    }
}
