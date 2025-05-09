using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    private Button restartButton;

    private void Awake()
    {
        // singleton setup
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // hide immediately 
        panel.SetActive(false);
    }

    private void Start()
    {
        // wire up the restart button
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(OnRestartClicked);

        // ensure clicks go to the button, not the panel background
        if (panel.TryGetComponent<Image>(out var bg)) bg.raycastTarget = false;
        if (panel.TryGetComponent<CanvasGroup>(out var cg)) cg.blocksRaycasts = false;
    }

    public void Show(string message)
    {
        messageText.text = message;
        panel.SetActive(true);
    }

    private void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
