using System.Collections.Generic;
using UnityEngine;

// Wrapper for JSON data containing all enemy definitions.

[System.Serializable]
public class EnemyList
{
    // <summary>List of all configured enemies.</summary>
    public List<Enemy> Enemies = new List<Enemy>();
}

// Wrapper for JSON data containing all level definitions.

[System.Serializable]
public class LevelList
{
    // <summary>List of all configured levels.</summary>
    public List<Level> Levels = new List<Level>();
}

// Defines the stats and visual index for a single enemy type.

[System.Serializable]
public class Enemy
{
    [Tooltip("Unique name identifier for this enemy type.")]
    public string Name;

    [Tooltip("Index into the sprite atlas or array.")]
    public int SpriteIndex;

    [Tooltip("Base health points for this enemy.")]
    public int HP;

    [Tooltip("Movement speed for this enemy.")]
    public float Speed;

    [Tooltip("Damage dealt by this enemy.")]
    public int Damage;
}

// Configuration for spawning a particular enemy within a level.

[System.Serializable]
public class Spawn
{
    [Tooltip("Name of the enemy to spawn (matches Enemy.Name).")]
    public string EnemyName;

    [Tooltip("RPN expression or literal for total spawn count.")]
    public string CountExpression;

    [Tooltip("Optional sequence of batch sizes per spawn tick.")]
    public List<int> Sequence = new List<int>();

    [Tooltip("RPN expression or literal delay between batches.")]
    public string DelayExpression;

    [Tooltip("Spawn location specifier, e.g., \"random red\".")]
    public string Location;

    [Tooltip("RPN or \"base\" expression for spawned HP.")]
    public string HPExpression;

    [Tooltip("RPN or \"base\" expression for spawned speed.")]
    public string SpeedExpression;

    [Tooltip("RPN or \"base\" expression for spawned damage.")]
    public string DamageExpression;
}

// Represents a game level with one or more waves of spawns.

[System.Serializable]
public class Level
{
    [Tooltip("Unique name identifier for this level.")]
    public string Name;

    [Tooltip("Number of waves in this level; <= 0 indicates endless mode.")]
    public int Waves = 0;

    [Tooltip("List of spawn definitions for each wave.")]
    public List<Spawn> Spawns = new List<Spawn>();
}
