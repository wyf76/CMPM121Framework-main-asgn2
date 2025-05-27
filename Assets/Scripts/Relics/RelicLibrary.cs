using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class RelicLibrary : MonoBehaviour
{
    public static RelicLibrary Instance { get; private set; }

    private List<RelicData> allRelics;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRelics();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadRelics()
    {
        TextAsset json = Resources.Load<TextAsset>("relics");
        if (json == null)
        {
            Debug.LogError("relics.json not found in Resources!");
            allRelics = new List<RelicData>();
            return;
        }

        allRelics = JsonConvert.DeserializeObject<List<RelicData>>(json.text);
        Debug.Log($"Loaded {allRelics.Count} relics.");
    }

    public List<RelicData> GetAllRelics()
    {
        return allRelics;
    }

    // Returns a list of random relics the player doesn't already own.

    public List<RelicData> GetRandomRelics(int count, List<string> alreadyOwned)
    {
        List<RelicData> candidates = allRelics.FindAll(r => !alreadyOwned.Contains(r.name));
        List<RelicData> selection = new List<RelicData>();

        while (selection.Count < count && candidates.Count > 0)
        {
            int index = Random.Range(0, candidates.Count);
            selection.Add(candidates[index]);
            candidates.RemoveAt(index);
        }

        return selection;
    }
}
