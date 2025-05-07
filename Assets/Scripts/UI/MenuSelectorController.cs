using UnityEngine;
using TMPro;

public class MenuSelectorController : MonoBehaviour
{
    public TextMeshProUGUI label;
    public string level;
    public EnemySpawner spawner;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevel(string text)
    {
        level = text;
        label.text = text;
        
        Debug.Log("Setting button: " + text);
        GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StartLevel);
    }

    public void StartLevel()
    {
        Debug.Log("Button clicked: " + level);
        spawner.StartLevel(level);
    }
}
