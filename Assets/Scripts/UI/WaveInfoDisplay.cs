using TMPro;
using UnityEngine;

public class WaveInfoDisplay : MonoBehaviour
{
    public static WaveInfoDisplay Instance;

    private TMP_Text infoText;

    void Awake()
    {
        Instance = this;
        infoText = GetComponent<TMP_Text>();
    }

    public void Show(string text)
    {
        gameObject.SetActive(true);
        infoText.text = text;
    }

    public void Hide()
    {
        infoText.text = "";
    }
}
