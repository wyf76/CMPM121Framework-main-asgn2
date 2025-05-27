using UnityEngine;
using UnityEngine.UI;

public class RelicUIController : MonoBehaviour
{
    [Header("Panel & Slots")]
    public GameObject relicSlotsPanel;
    public Button[]    takeButtons;      // size = 3
    public Image[]     iconImages;       // size = 3
    public Text[]      descriptionTexts; // size = 3

    // store the current batch of choices so OnTakeRelic can reference them
    private RelicDefinition[] currentChoices;

    void Start()
    {
        relicSlotsPanel.SetActive(false);

        for (int i = 0; i < takeButtons.Length; i++)
        {
            int idx = i; // capture for the lambda
            takeButtons[i].onClick.AddListener(() => OnTakeRelic(idx));
        }
    }


    public void ShowRelics(RelicDefinition[] choices)
    {
        currentChoices = choices;
        relicSlotsPanel.SetActive(true);

        for (int i = 0; i < choices.Length; i++)
        {
            // 1) pull the sprite from your RelicIconManager via its index
            var spriteIndex = choices[i].sprite;
            iconImages[i].sprite = GameManager.Instance
                                        .relicIconManager
                                        .GetIcon(spriteIndex);

            // 2) show the two‑line description
            descriptionTexts[i].text = 
                choices[i].trigger.description + "\n" +
                choices[i].effect.description;

            takeButtons[i].interactable = true;
        }
    }

    private void OnTakeRelic(int index)
    {
        // apply the RelicDefinition itself, not the index
        RelicRegistry.Apply(currentChoices[index]);
        relicSlotsPanel.SetActive(false);
    }
}
