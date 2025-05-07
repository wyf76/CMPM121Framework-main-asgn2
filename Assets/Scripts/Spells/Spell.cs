using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Spell 
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
    }

    public virtual string GetName()
    {
        return "Generic Spell";
    }

    public virtual string GetDescription()
    {
        return "A basic magical effect."; 
    }

    public virtual int GetIcon()
    {
        return 0;
    }


    public virtual int GetManaCost()
    {
        return 10;
    }

    public virtual int GetDamage()
    {
  
        return 10; 
    }

    public virtual float GetCooldown()
    {
        return 1.0f;
    }

    public bool IsReady()
    {
        float cooldown = GetCooldown();
        if (cooldown <= 0) return true;
        return (last_cast + cooldown < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team teamOfCaster) 
    {
        this.last_cast = Time.time; // Record cast time
        this.team = teamOfCaster;   

        if (GameManager.Instance != null && GameManager.Instance.projectileManager != null)
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                GetIcon(),         
                "straight",      
                where,
                (target - where).normalized,
                15f,            
                OnHit            
            );
        }
        yield return null;
    }

    protected virtual void OnHit(Hittable other, Vector3 impactPoint) 
    {
        if (other.team != this.team) 
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }
    }

    // Helper to find a specific effect from SpellData (will be used more in BaseSpell/ModifierSpell)
    protected EffectData GetEffectData(SpellData spellData, string effectType)
    {
        if (spellData?.effects == null) return null;
        foreach (var effect in spellData.effects)
        {
            if (effect.type.ToLower() == effectType.ToLower())
            {
                return effect;
            }
        }
        return null;
    }
}
