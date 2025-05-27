using UnityEngine;
using TMPro;

public class MenuSelectorController : MonoBehaviour
{
    public TextMeshProUGUI label;
    public string level;
    public EnemySpawnerController spawner;
    public GameObject mainMenuPanel; // New field to hold the entire menu

    public void SetLevel(string text)
    {
        level = text;
        if (label != null) label.text = text;    
    }

    public void StartLevel()
    {
        // Hide the entire main menu
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        // Start the spawner
        if (spawner != null)
        {
            Debug.Log("Start: " + level);
            spawner.StartLevel(level);
        }
    }
}