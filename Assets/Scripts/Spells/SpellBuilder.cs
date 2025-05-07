using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class SpellBuilder 
{

    public Spell Build(string spellId, SpellCaster owner, int wave, int power)
    {
        SpellData data = SpellLibrary.Instance.GetSpellData(spellId);
        if (data == null)
        {
            Debug.LogWarning($"[SpellBuilder] Spell ID '{spellId}' not found, using default.");
            return new Spell(owner); // fallback
        }

        if (!string.IsNullOrEmpty(data.inner_spell))
        {
            // It's a modifier — recursively build inner spell
            Spell inner = Build(data.inner_spell, owner, wave, power);
            return new ModifierSpell(owner, data, inner, wave, power);
        }
        else
        {
            // Base spell
            return new BaseSpell(owner, data, wave, power);
        }
    }

   
    public SpellBuilder()
    {        
    }

    public Spell BuildRandom(SpellCaster owner, int wave)
    {
        int power = owner.spell_power;

        // 1. Choose random base spell
        List<string> baseSpellIds = SpellLibrary.Instance.GetAllSpellIDs().Where(id =>
        {
            SpellData d = SpellLibrary.Instance.GetSpellData(id);
            return d != null && string.IsNullOrEmpty(d.inner_spell);
        }).ToList();

        string baseId = baseSpellIds[Random.Range(0, baseSpellIds.Count)];
        Spell spell = Build(baseId, owner, wave, power);

        // 2. Add 0–2 random modifier spells
        int modifierCount = Random.Range(0, 3);

        for (int i = 0; i < modifierCount; i++)
        {
            List<string> modifierIds = SpellLibrary.Instance.GetAllSpellIDs().Where(id =>
            {
                SpellData d = SpellLibrary.Instance.GetSpellData(id);
                return d != null && !string.IsNullOrEmpty(d.inner_spell);
            }).ToList();

            if (modifierIds.Count == 0) break;

            string modId = modifierIds[Random.Range(0, modifierIds.Count)];
            SpellData modData = SpellLibrary.Instance.GetSpellData(modId);
            spell = new ModifierSpell(owner, modData, spell, wave, power);
        }

        return spell;
    }
}
