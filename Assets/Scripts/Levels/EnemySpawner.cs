using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    private List<LevelData> levels;
    public LevelData selectedLevel;

    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        GameManager.Instance.enemySpawner = this;

        levels = GameDataLoader.LoadLevels();

        float y = 130;
        foreach (var lvl in levels)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, y);
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(lvl.name);
            y -= 100; 
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
        selectedLevel = levels.FirstOrDefault(l => l.name == levelname);
        if (selectedLevel == null)
        {
            Debug.LogError($"Level '{levelname}' not found!");
            return;
        }

        level_selector.gameObject.SetActive(false);
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        GameManager.Instance.wave++;
        int waveNumber = GameManager.Instance.wave; // wave starts from 1
        
        GameManager.Instance.player.GetComponent<PlayerController>().ApplyWaveStats(GameManager.Instance.wave);
        

        if (selectedLevel != null)
        {
            string text = $"<b>Wave {waveNumber} - {selectedLevel.name}</b>\n";
            foreach (var spawn in selectedLevel.spawns)
            {
                int count = RPNEvaluator.Evaluate(spawn.count, new Dictionary<string, int> { { "wave", waveNumber } });
                text += $"- {spawn.enemy}: {count} enemies\n";
            }
            WaveInfoDisplay.Instance.Show(text);
        }

        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }

        WaveInfoDisplay.Instance.Hide();
        GameManager.Instance.state = GameManager.GameState.INWAVE;

        foreach (var spawn in selectedLevel.spawns)
        {
            int baseHp = GameManager.Instance.GetEnemyBaseHP(spawn.enemy);
            int baseSpeed = GameManager.Instance.GetEnemyBaseSpeed(spawn.enemy);
            int baseDamage = GameManager.Instance.GetEnemyBaseDamage(spawn.enemy);

            var vars = new Dictionary<string, int>
            {
                { "base", 0 }, // filled per stat below
                { "wave", waveNumber }
            };

            int count = RPNEvaluator.Evaluate(spawn.count, new Dictionary<string, int> { { "wave", waveNumber } });
            int delay = RPNEvaluator.Evaluate(spawn.delay ?? "2", vars);

            List<int> sequence = spawn.sequence != null && spawn.sequence.Count > 0 ? spawn.sequence : new List<int> { 1 };
            int seqIndex = 0;

            for (int spawned = 0; spawned < count;)
            {
                int groupCount = Mathf.Min(sequence[seqIndex % sequence.Count], count - spawned);
                for (int i = 0; i < groupCount; i++)
                {
                    // Calculate stats
                    vars["base"] = baseHp;
                    int hp = RPNEvaluator.Evaluate(spawn.hp ?? "base", vars);

                    vars["base"] = baseSpeed;
                    int speed = RPNEvaluator.Evaluate(spawn.speed ?? "base", vars);

                    vars["base"] = baseDamage;
                    int damage = RPNEvaluator.Evaluate(spawn.damage ?? "base", vars);

                    SpawnEnemy(spawn.enemy, spawn.location, hp, speed, damage);
                }

                spawned += groupCount;
                seqIndex++;
                if (spawned < count)
                    yield return new WaitForSeconds(delay);
            }
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);

        // After wave ends
        if (selectedLevel.waves > 0 && GameManager.Instance.wave >= selectedLevel.waves){
            GameManager.Instance.EndGame(true); // Player wins
        }
        else{
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
            string stats = $"Wave {GameManager.Instance.wave} Complete!";
            FindObjectOfType<WaveSummaryUI>().Show(stats);
            // Generate and display random spell reward
            Spell reward = new SpellBuilder().BuildRandom(GameManager.Instance.player.GetComponent<PlayerController>().spellcaster, GameManager.Instance.wave);
            FindObjectOfType<RewardSpellManager>().ShowRewardSpell(reward);

        }
    }


    IEnumerator SpawnZombie()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }

    void SpawnEnemy(string enemyType, string location, int hp, int speed, int damage)
    {
        SpawnPoint[] candidates = SpawnPoints;
        if (!string.IsNullOrEmpty(location) && location != "random")
        {
            string[] parts = location.Split(' ');
            if (parts.Length > 1)
            {
                string tag = parts[1];
                candidates = SpawnPoints.Where(p => p.kind.ToString().ToLower() == tag.ToLower()).ToArray();
            }
        }

        SpawnPoint spawnPoint = candidates[Random.Range(0, candidates.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 spawnPos = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);

        GameObject new_enemy = Instantiate(enemy, spawnPos, Quaternion.identity);
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.GetEnemySpriteByName(enemyType);

        EnemyController ec = new_enemy.GetComponent<EnemyController>();
        ec.hp = new Hittable(hp, Hittable.Team.MONSTERS, new_enemy);
        ec.speed = speed;
        ec.damage = damage;

        GameManager.Instance.AddEnemy(new_enemy);
    }

    public void ResetUI()
    {
        selectedLevel = null;
        level_selector.gameObject.SetActive(true);
    }
}
