using System.Collections.Generic;
using UnityEngine;

public static class GameDataLoader
{
    public static List<EnemyData> LoadEnemies()
    {
        TextAsset json = Resources.Load<TextAsset>("enemies");
        return JsonUtilityWrapper.FromJsonList<EnemyData>(json.text);
    }

    public static List<LevelData> LoadLevels()
{
    TextAsset json = Resources.Load<TextAsset>("levels");
    return JsonUtilityWrapper.FromJsonList<LevelData>(json.text);
}
}
