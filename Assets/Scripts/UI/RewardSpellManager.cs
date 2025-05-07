using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardSpellManager : MonoBehaviour
{
    public GameObject rewardPanel;
    public TextMeshProUGUI spellName;
    public TextMeshProUGUI spellDescription;
    public Image spellIcon;
    public Button takeButton;
    public Button skipButton;

    private Spell rewardSpell;

    public void ShowRewardSpell(Spell newSpell)
    {
        // Show spell info
        rewardSpell = newSpell;

        spellName.text = newSpell.GetName();
        spellDescription.text = $"Mana: {newSpell.GetManaCost()} | Damage: {newSpell.GetDamage()} | Cooldown: {newSpell.GetCooldown()}";
        spellIcon.sprite = GameManager.Instance.spellIconManager.Get(newSpell.GetIcon());

        rewardPanel.SetActive(true);
    }

    public void OnTakePressed()
    {
        GameManager.Instance.player.GetComponent<PlayerController>().ReplaceSpell(rewardSpell);
        rewardPanel.SetActive(false);
    }

    public void OnSkipPressed()
    {
        rewardPanel.SetActive(false);
    }
}
