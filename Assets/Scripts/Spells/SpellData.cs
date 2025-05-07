using System;

[Serializable]
public class ProjectileData
{
    public string trajectory;
    public string speed;
    public int sprite;
    public string lifetime; // optional
}

[Serializable]
public class DamageData
{
    public string amount;
    public string type;
}

[Serializable]
public class SpellData
{
    public string name;
    public string description;
    public int icon;

    public string mana_cost;
    public string cooldown;
    public string N;

    public DamageData damage;
    public string secondary_damage;

    public ProjectileData projectile;
    public ProjectileData secondary_projectile;

    public string inner_spell; // for modifier spells
}
