using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager 
{
    public enum GameState
    {
        PREGAME,
        INWAVE,
        WAVEEND,
        COUNTDOWN,
        GAMEOVER
    }
    public GameState state;

    public int countdown;
    private static GameManager theInstance;
    public static GameManager Instance {  get
        {
            if (theInstance == null)
                theInstance = new GameManager();
            return theInstance;
        }
    }

    public GameObject player;
    
    public ProjectileManager projectileManager;
    public SpellIconManager spellIconManager;
    public EnemySpriteManager enemySpriteManager;
    public PlayerSpriteManager playerSpriteManager;
    public RelicIconManager relicIconManager;
    public EnemySpawner enemySpawner;

    private List<GameObject> enemies;
    private List<EnemyData> enemyData;
    public int wave = -1;
    public int enemy_count { get { return enemies.Count; } }

    public void AddEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }
    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
    }

    public GameObject GetClosestEnemy(Vector3 point)
    {
        if (enemies == null || enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        return enemies.Aggregate((a,b) => (a.transform.position - point).sqrMagnitude < (b.transform.position - point).sqrMagnitude ? a : b);
    }

    private GameManager()
    {
        enemies = new List<GameObject>();
        enemyData = GameDataLoader.LoadEnemies();
    }

    public int GetEnemyBaseHP(string name)
    {
        var e = enemyData.FirstOrDefault(x => x.name == name);
        return e?.hp ?? 20;
    }

    public int GetEnemyBaseSpeed(string name)
    {
        var e = enemyData.FirstOrDefault(x => x.name == name);
        return e?.speed ?? 5;
    }

    public int GetEnemyBaseDamage(string name)
    {
        var e = enemyData.FirstOrDefault(x => x.name == name);
        return e?.damage ?? 5;
    }

    public Sprite GetEnemySpriteByName(string name)
    {
        var e = enemyData.FirstOrDefault(x => x.name == name);
        return e != null ? enemySpriteManager.Get(e.sprite) : null;
    }

    public void EndGame(bool victory)
    {
        state = GameState.GAMEOVER;
        string msg = victory ? "You Win!" : "Game Over";
        Debug.Log($"EndGame called, showing UI: {msg}");
        GameOverUI.Instance.Show(msg);
    }
}
