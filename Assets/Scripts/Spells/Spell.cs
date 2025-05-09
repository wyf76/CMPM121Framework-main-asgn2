using UnityEngine;

<<<<<<< HEAD

//Base class for all spells, handling stats and casting.

public abstract class Spell
{
    protected SpellCaster owner;
    public SpellCaster Owner => owner;
    public float lastCast;
    public StatBlock mods = new();

    protected Spell(SpellCaster owner) { this.owner = owner; }

    public abstract string DisplayName { get; }
    public abstract int    IconIndex   { get; }

    protected virtual float BaseDamage   => 10f;
    protected virtual float BaseMana     => 10f;
    protected virtual float BaseCooldown => 1f;
    protected virtual float BaseSpeed    => 8f;

    public float Damage   => StatBlock.Apply(BaseDamage,   mods.DamageMods);
    public float Mana     => StatBlock.Apply(BaseMana,     mods.ManaMods);
    public float Cooldown => StatBlock.Apply(BaseCooldown, mods.CooldownMods);
    public float Speed    => StatBlock.Apply(BaseSpeed,    mods.SpeedMods);
    public bool  IsReady  => Time.time >= lastCast + Cooldown;

    // Attempts to cast immediately, assuming mana and cooldown checks are done by SpellCaster.

    public IEnumerator TryCast(Vector3 from, Vector3 to)
    {
        Debug.Log($"[Spell] Casting {DisplayName}");
        yield return Cast(from, to);
    }

    protected abstract IEnumerator Cast(Vector3 from, Vector3 to);

    public virtual void LoadAttributes(JObject json, Dictionary<string,float> vars) { }
=======
public abstract class Spell
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public int IconIndex { get; protected set; }

    protected float lastCastTime = -Mathf.Infinity;

    internal abstract void CastInternal(Vector3 spawnPosition, Vector3 direction, SpellModifierContext context);


    public void Cast(Vector3 spawnPosition, Vector3 direction)
    {
        SpellModifierContext context = new SpellModifierContext();
        CastInternal(spawnPosition, direction, context);
        PutOnCooldown();
    }

    public abstract int GetManaCost();

    public abstract float GetDamage();

    public abstract float GetCooldown();

    public abstract string GetProjectileType();

    public abstract int GetProjectileSprite();

    public abstract float GetProjectileSpeed();

    public abstract float GetProjectileLifetime();

    public bool IsOffCooldown()
    {
        return Time.time >= lastCastTime + GetCooldown();
    }

    public void PutOnCooldown()
    {
        lastCastTime = Time.time;
    }
>>>>>>> 1e7a3e8 (some updates)
}
