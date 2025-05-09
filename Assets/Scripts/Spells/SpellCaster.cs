using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public Hittable.Team casterTeam;
    public int spell_power;
    public int level;
    public Spell spell;

    public GameObject CasterGameObject { get; private set; } 

    public SpellCaster(int mana, int mana_reg, Hittable.Team team, int power, int level)
    {
        this.mana = mana;
        this.max_mana = mana; // Initial max mana is current mana
        this.mana_reg = mana_reg;
        this.casterTeam = team;
        this.spell_power = power;
        this.level = level;
    }

    public void SetCasterGameObject(GameObject casterGO) 
    {
        this.CasterGameObject = casterGO;
    }
    
    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            if (mana < max_mana)
            {
                mana = Mathf.Min(mana + mana_reg, max_mana);
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}