using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawnerController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Container for level‐selection buttons.")]
    [SerializeField] private Transform LevelSelectorContainer = null;
    [Tooltip("Button prefab for each level.")]
    [SerializeField] private Button LevelButtonPrefab = null;

    [Header("Spawning")]
    [Tooltip("Prefab to spawn for each enemy.")]
    [SerializeField] private GameObject EnemyPrefab = null;
    [Tooltip("All available spawn points in the scene.")]
    [SerializeField] private SpawnPoint[] SpawnPoints = null;

    public Level CurrentLevel        { get; private set; }
    public int   CurrentWave         { get; private set; }
    public int   LastWaveEnemyCount  { get; private set; }

    private bool waveInProgress = false;
    private bool IsEndless => CurrentLevel != null && CurrentLevel.Waves <= 0;

    private void Start()
    {
        // Dynamically build your three “Easy/Medium/Endless” buttons
        if (LevelSelectorContainer == null || LevelButtonPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: UI references not set!");
            return;
        }

        foreach (var lvl in GameManager.Instance.levelDefs)
        {
            // Instantiate and parent under your UI container
            Button btn = Instantiate(LevelButtonPrefab, LevelSelectorContainer, false);
            var ctrl  = btn.GetComponent<MenuSelectorController>();
            ctrl.spawner = this;
            ctrl.SetLevel(lvl.Name);

            // Capture for the listener
            string nameCopy = lvl.Name;
            btn.onClick.AddListener(() => StartLevel(nameCopy));
        }
    }

    /// <summary>
    /// Kicks off the chosen level.
    /// </summary>
    public void StartLevel(string levelName)
    {
        Debug.Log($"[EnemySpawner] StartLevel('{levelName}')");

        // Hide the level‐select UI
        LevelSelectorContainer.gameObject.SetActive(false);

        // Lookup the Level object
        CurrentLevel = GameManager.Instance.levelDefs
            .Find(l => l.Name == levelName);
        if (CurrentLevel == null)
        {
            Debug.LogError($"Level not found: {levelName}");
            return;
        }

        CurrentWave = 1;
        InitializePlayerForWave();
        StartCoroutine(SpawnWaveRoutine());
    }

    /// <summary>
    /// Manually trigger the next wave (e.g. from UI).
    /// </summary>
    public void NextWave()
    {
        if (!waveInProgress)
            StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        if (waveInProgress) yield break;
        waveInProgress = true;

        // 1) Scale player stats
        InitializePlayerForWave();

        // 2) Countdown
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        for (int i = 3; i > 0; i--)
        {
            GameManager.Instance.countdown = i;
            yield return new WaitForSeconds(1f);
        }
        GameManager.Instance.countdown = 0;

        // 3) In‐wave state
        GameManager.Instance.state = GameManager.GameState.INWAVE;

        // 4) Spawn each defined enemy batch
        int totalSpawned = 0;
        foreach (var spawn in CurrentLevel.Spawns)
            yield return StartCoroutine(SpawnEnemies(spawn, c => totalSpawned += c));
        LastWaveEnemyCount = totalSpawned;

        // 5) Wait until all monsters are dead
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);

        // 6) Win check (non‐endless only)
        if (!IsEndless && CurrentWave >= CurrentLevel.Waves)
        {
            TriggerWin();
            yield break;
        }

        // 7) End‐of‐wave UI
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        GameManager.Instance.wavesCompleted++;

        // 8) Prep next wave
        CurrentWave++;
        waveInProgress = false;
    }

    private IEnumerator SpawnEnemies(Spawn spawn, System.Action<int> onComplete)
    {
        // Find the base enemy definition
        var def = GameManager.Instance.enemyDefs
                     .Find(e => e.Name == spawn.EnemyName);
        if (def == null)
        {
            onComplete?.Invoke(0);
            yield break;
        }

        // Evaluate RPN
        var intVars = new Dictionary<string,int> { ["base"]=def.HP, ["wave"]=CurrentWave };
        int total    = RPNEvaluator.SafeEvaluate(spawn.CountExpression,  intVars, 0);
        int hpValue  = RPNEvaluator.SafeEvaluate(spawn.HPExpression,     intVars, def.HP);
        float speed  = RPNEvaluator.SafeEvaluate(spawn.SpeedExpression,  intVars, (int)def.Speed);
        int dmg      = RPNEvaluator.SafeEvaluate(spawn.DamageExpression, intVars, def.Damage);
        float delay  = RPNEvaluator.SafeEvaluate(spawn.DelayExpression,  intVars, 2);

        speed = Mathf.Clamp(speed, 1f, 20f);
        var seq = (spawn.Sequence.Count>0) ? spawn.Sequence : new List<int>{1};

        int spawned = 0, idx = 0;
        while (spawned < total)
        {
            int batch = seq[idx++ % seq.Count];
            for (int i = 0; i < batch && spawned < total; i++)
                SpawnOne(def, hpValue, dmg, ref spawned);
            yield return new WaitForSeconds(delay);
        }

        onComplete?.Invoke(spawned);
    }

    private void SpawnOne(Enemy def, int hpValue, int damageValue, ref int spawned)
    {
        // Pick & offset a spawn point
        SpawnPoint pt = PickSpawnPoint(def.Name);
        Vector2 ofs   = GetNonOverlappingOffset(pt.transform.position);
        Vector3 pos   = pt.transform.position + (Vector3)ofs;

        // Instantiate
        var go = Instantiate(EnemyPrefab, pos, Quaternion.identity);
        go.GetComponent<SpriteRenderer>()
          .sprite = GameManager.Instance.enemySpriteManager.Get(def.SpriteIndex);

        // Setup controller
        var ec = go.GetComponent<EnemyController>();
        ec.hp     = new Hittable(hpValue, Hittable.Team.MONSTERS, go);
        ec.speed  = Mathf.RoundToInt(def.Speed);
        ec.damage = damageValue;

        GameManager.Instance.AddEnemy(go);
        spawned++;
    }

    private Vector2 GetNonOverlappingOffset(Vector3 center)
    {
        const float R = 3f;
        const float C = 0.75f;
        for (int i = 0; i < 10; i++)
        {
            var ofs = Random.insideUnitCircle * R;
            if (Physics2D.OverlapCircleAll(center+(Vector3)ofs, C).Length==0)
                return ofs;
        }
        return Random.insideUnitCircle * R;
    }

    private SpawnPoint PickSpawnPoint(string loc)
    {
        if (string.IsNullOrEmpty(loc) || !loc.StartsWith("random"))
            return SpawnPoints[Random.Range(0,SpawnPoints.Length)];
        if (loc=="random")
            return SpawnPoints[Random.Range(0,SpawnPoints.Length)];

        var kind = loc.Split(' ')[1].ToUpperInvariant();
        var matches = new List<SpawnPoint>();
        foreach (var sp in SpawnPoints)
            if (sp.kind.ToString().ToUpperInvariant()==kind)
                matches.Add(sp);
        return (matches.Count>0)
            ? matches[Random.Range(0,matches.Count)]
            : SpawnPoints[Random.Range(0,SpawnPoints.Length)];
    }

    private void InitializePlayerForWave()
    {
        var pc = GameManager.Instance.player.GetComponent<PlayerController>();
        pc.StartLevel();
        ScalePlayerForWave(CurrentWave);
    }

    private void ScalePlayerForWave(int wave)
    {
        var vars = new Dictionary<string,float> { ["wave"]=wave };
        float rHP  = RPNEvaluator.EvaluateFloat("95 wave 5 * +", vars);
        float rMana= RPNEvaluator.EvaluateFloat("90 wave 10 * +", vars);
        float rRe  = RPNEvaluator.EvaluateFloat("10 wave +", vars);
        float rPow = RPNEvaluator.EvaluateFloat("wave 10 *", vars);
        float rSpd = RPNEvaluator.EvaluateFloat("5", vars);

        var pc = GameManager.Instance.player.GetComponent<PlayerController>();
        pc.hp.SetMaxHP(Mathf.RoundToInt(rHP), true);
        pc.spellcaster.max_mana   = Mathf.RoundToInt(rMana);
        pc.spellcaster.mana       = pc.spellcaster.max_mana;
        pc.spellcaster.mana_reg   = Mathf.RoundToInt(rRe);
        pc.spellcaster.spellPower = Mathf.RoundToInt(rPow);
        pc.speed                  = Mathf.RoundToInt(rSpd);

        pc.healthui.SetHealth(pc.hp);
        pc.manaui.SetSpellCaster(pc.spellcaster);

        Debug.Log($"→ PlayerStats: HP={pc.hp.hp}/{pc.hp.max_hp}, " +
                  $"Mana={rMana:F1}, Regen={rRe:F1}, Power={rPow:F1}, Speed={rSpd:F1}");
    }

    private void TriggerWin()
    {
        GameManager.Instance.playerWon    = true;
        GameManager.Instance.IsPlayerDead = false;
        GameManager.Instance.state        = GameManager.GameState.GAMEOVER;
        Debug.Log("You Win: all waves completed.");
    }
}
