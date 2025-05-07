using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public TMP_Text waveInfoText;
    public Button nextWaveButton;

    public TMP_Text rewardNameText;
    public TMP_Text rewardStatsText;
    public Image rewardIconImage;
    public Button takeButton;
    public Button skipButton;

    public Spell rewardSpell;


    void Start()
    {
        nextWaveButton.onClick.AddListener(() =>
        {
            rewardUI.SetActive(false);
            GameManager.Instance.enemySpawner.NextWave();
        });


        takeButton.onClick.AddListener(() =>
        {
            GameManager.Instance.player.GetComponent<PlayerController>().ReplaceSpell(rewardSpell);
            rewardSpell = null;
            rewardUI.SetActive(false);
            GameManager.Instance.enemySpawner.NextWave();
        });

        skipButton.onClick.AddListener(() =>
        {
            rewardSpell = null;
            rewardUI.SetActive(false);
            GameManager.Instance.enemySpawner.NextWave();
        });

    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            rewardUI.SetActive(true);

            int waveNum = GameManager.Instance.wave;
            string levelName = GameManager.Instance.enemySpawner.selectedLevel?.name ?? "Unknown";

            waveInfoText.text = $"Wave {waveNum} complete!\nLevel: {levelName}";

            // Only generate reward if it's null and wave just ended
            if (rewardSpell == null)
            {
                rewardSpell = new SpellBuilder().BuildRandom(
                GameManager.Instance.player.GetComponent<PlayerController>().spellcaster,
                GameManager.Instance.wave
                );

            rewardNameText.text = rewardSpell.GetName();
            rewardStatsText.text = $"Mana: {rewardSpell.GetManaCost()} | Damage: {rewardSpell.GetDamage()} | Cooldown: {rewardSpell.GetCooldown()}";

            rewardIconImage.sprite = GameManager.Instance.enemySpriteManager.Get(rewardSpell.GetIcon());
            }


            //nextWaveButton.gameObject.SetActive(true);
        }
        else if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            rewardUI.SetActive(true);
            waveInfoText.text = "Game Over";
            nextWaveButton.gameObject.SetActive(false);
        }
        else
        {
            rewardUI.SetActive(false);
        }
    }
}
