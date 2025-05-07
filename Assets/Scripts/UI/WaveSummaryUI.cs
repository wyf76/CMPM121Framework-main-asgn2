using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaveSummaryUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI statsText;
    public Button continueButton;

    void Start()
    {
        gameObject.SetActive(false);
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void Show(string stats)
    {
        statsText.text = stats;
        gameObject.SetActive(true);
    }

    public void OnContinueClicked()
    {
        gameObject.SetActive(false);
        GameManager.Instance.enemySpawner.NextWave();
    }
}
