using UnityEngine;
using TMPro;

public class GameOverButton : MonoBehaviour
{
    public TextMeshProUGUI label;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        label.text = "Back To Menu";
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void BackToMenu()
    {
        GameManager.Instance.state = GameManager.GameState.PREGAME;
    }
}