using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class SpellLibrary : MonoBehaviour
{
    public static SpellLibrary Instance { get; private set; }

    public Dictionary<string, SpellData> spells;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadSpells();
    }

    void LoadSpells()
    {
        TextAsset json = Resources.Load<TextAsset>("spells");
        if (json == null)
        {
            Debug.LogError("spells.json not found in Resources folder!");
            return;
        }

        JObject parsed = JObject.Parse(json.text);
        spells = new Dictionary<string, SpellData>();

        foreach (var kvp in parsed)
        {
            SpellData data = kvp.Value.ToObject<SpellData>();
            spells[kvp.Key] = data;
        }

        Debug.Log($"Loaded {spells.Count} spells.");
    }

    public SpellData GetSpellData(string id)
    {
        return spells.ContainsKey(id) ? spells[id] : null;
    }

    public List<string> GetAllSpellIDs()
    {
        return new List<string>(spells.Keys);
    }
}
