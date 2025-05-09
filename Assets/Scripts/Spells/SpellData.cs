using System.Collections.Generic;


[System.Serializable]
public class DamageData
{
    public string amount; 
    public string type;   
}

[System.Serializable]
public class ProjectileData
{
    public string trajectory;
    public string speed;      
    public int sprite;       
    public string lifetime;   
}

[System.Serializable]
public class EffectData 
{
    public string type;
    // Pierce
    public string count;

    // Chill
    public string duration;
    public string slow_factor;

    // Chain reaction (if used)
    public string chance;
    public string secondary_spell_id;
    public string radius;

    // Lifesteal
    public string percent;

    // Knockback
    public string force;
}

[System.Serializable]
public class SpellData
{
    public string name;
    public string description;
    public int icon;
    public DamageData damage;
    public string mana_cost;
    public string cooldown;

    public ProjectileData projectile;
    public string inner_spell;

    // Stat modifiers
    public string damage_multiplier;
    public string mana_multiplier;
    public string mana_adder;
    public string speed_multiplier;
    public string cooldown_multiplier;
    public string delay;
    public string angle;
    public string projectile_trajectory;

    // Secondary projectile
    public string N;
    public string secondary_damage;
    public ProjectileData secondary_projectile;
    public string spray;

    // List of on-hit effects
    public List<EffectData> effects;
}
