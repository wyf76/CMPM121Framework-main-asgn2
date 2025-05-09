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

        // custom base spell
        if (spellId == "frost_spike")
            return new FrostSpikeSpell(owner, data, wave, power);

        // custom modifiers
        if (spellId == "chain_reaction_modifier")
        {
            var inner = Build(data.inner_spell, owner, wave, power);
            return new ChainReactionModifierSpell(owner, data, inner, wave, power);
        }
        if (spellId == "vampiric_essence_modifier")
        {
            var inner = Build(data.inner_spell, owner, wave, power);
            return new VampiricEssenceModifierSpell(owner, data, inner, wave, power);
        }

        // generic modifier
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

        var bases = all.Where(id =>
            SpellLibrary.Instance.GetSpellData(id).inner_spell == null).ToList();
        var baseId = bases[Random.Range(0, bases.Count)];
        var spell = Build(baseId, owner, wave, power);

        int mods = Random.Range(0, 3);
        var modifiers = all.Where(id =>
            SpellLibrary.Instance.GetSpellData(id).inner_spell != null).ToList();

        for (int i = 0; i < mods && modifiers.Count > 0; i++)
        {
            var modId = modifiers[Random.Range(0, modifiers.Count)];
            spell = Build(modId, owner, wave, power);
        }

        return spell;
    }
}
