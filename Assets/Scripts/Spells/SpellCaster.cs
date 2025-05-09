using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public int spell_power;
    public int current_wave;

    public Hittable.Team team;
    public Spell spell;

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana += mana_reg;
            mana = Mathf.Min(mana, max_mana);
            yield return new WaitForSeconds(1);
        }
    }

    public SpellCaster(int mana, int mana_reg, Hittable.Team team, int spell_power, int current_wave)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.spell_power = spell_power;
        this.current_wave = current_wave;
        this.team = team;

        // Give the player an initial spell
        spell = new SpellBuilder().Build("arcane_bolt", this, current_wave, spell_power);
    }

    public IEnumerator Cast(Vector3 where, Vector3 target)
    {
        if (mana >= spell.GetManaCost() && spell.IsReady())
        {
            mana -= spell.GetManaCost();
            yield return spell.Cast(where, target, team);
        }
        yield break;
    }
}
