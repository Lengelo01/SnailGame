using UnityEngine;
using UnityEngine.UI;

// Only selects an authored state. Text, images, colors and layout live in the scene.
public class LevelCardState : MonoBehaviour
{
    public GameObject ready;
    public GameObject upNext;
    public GameObject locked;
    public GameObject comingSoon;

    public void Show(bool exists, bool unlocked, bool current)
    {
        GameObject selected = !exists || !unlocked ? locked : current ? upNext : ready;
        foreach (GameObject state in new[] { ready, upNext, locked, comingSoon })
            if (state != null) state.SetActive(state == selected);
        Button button = GetComponent<Button>();
        button.interactable = exists && unlocked;
        if (selected != null) button.targetGraphic = selected.GetComponent<Image>();
    }
}
