using UnityEngine;
using System.Linq;

public class SpellBuilder
{
    public Spell Build(string spellId, SpellCaster owner, int wave, int power)
    {
        var data = SpellLibrary.Instance.GetSpellData(spellId);
        if (data == null)
        {
            Debug.LogWarning($"[SpellBuilder] '{spellId}' not found.");
            return new Spell(owner);
        }

        // Frost Spike modifier
        if (spellId == "frost_spike_modifier")
        {
            var inner = Build(data.inner_spell, owner, wave, power);
            return new FrostSpikeModifierSpell(owner, data, inner, wave, power);
        }

        // Vampiric Essence modifier
        if (spellId == "vampiric_essence_modifier")
        {
            var inner = Build(data.inner_spell, owner, wave, power);
            return new VampiricEssenceModifierSpell(owner, data, inner, wave, power);
        }

        // Knockback Bolt base spell
        if (spellId == "knockback_bolt")
            return new KnockbackBolt(owner, data, wave, power);

        // generic modifier (fallback)
        if (!string.IsNullOrEmpty(data.inner_spell))
        {
            var inner = Build(data.inner_spell, owner, wave, power);
            return new ModifierSpell(owner, data, inner, wave, power);
        }

        // all other base spells
        return new BaseSpell(owner, data, wave, power);
    }

    public Spell BuildRandom(SpellCaster owner, int wave)
    {
        int power = owner.spell_power;
        var all = SpellLibrary.Instance.GetAllSpellIDs();

        // pick a random base
        var baseIds = all.Where(id =>
            SpellLibrary.Instance.GetSpellData(id).inner_spell == null).ToList();
        var baseId = baseIds[Random.Range(0, baseIds.Count)];
        var spell = Build(baseId, owner, wave, power);

        // apply 0–2 modifiers
        int mods = Random.Range(0, 3);
        var modIds = all.Where(id =>
            !string.IsNullOrEmpty(SpellLibrary.Instance.GetSpellData(id).inner_spell))
            .ToList();

        for (int i = 0; i < mods && modIds.Count > 0; i++)
        {
            string modId = modIds[Random.Range(0, modIds.Count)];
            spell = Build(modId, owner, wave, power);
        }

        return spell;
    }
}
