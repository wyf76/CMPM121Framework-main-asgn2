using UnityEngine;
using System.Collections;
using System.Collections.Generic; // For List
using System.Linq; // For Linq operations like FirstOrDefault

public class BaseSpell : Spell
{
    protected SpellData data; 
    protected int wave;       
    protected int power;     

    public BaseSpell(SpellCaster owner, SpellData data, int wave, int power) : base(owner)
    {
        this.data = data;
        this.wave = wave;
        this.power = power;
    }

    public override string GetName()
    {
        return data?.name ?? base.GetName();
    }

    public override string GetDescription()
    {
        return data?.description ?? base.GetDescription();
    }

    public override int GetIcon()
    {
        return (data != null) ? data.icon : base.GetIcon();
    }

    public override int GetManaCost() => (data != null && !string.IsNullOrEmpty(data.mana_cost)) ? RPNEvaluatorInt.Evaluate(data.mana_cost, wave, power) : base.GetManaCost();
    
    public override int GetDamage() => (data != null && data.damage != null && !string.IsNullOrEmpty(data.damage.amount)) ? RPNEvaluatorInt.Evaluate(data.damage.amount, wave, power) : base.GetDamage();
    
    public override float GetCooldown() => (data != null && !string.IsNullOrEmpty(data.cooldown)) ? RPNEvaluatorFloat.Evaluate(data.cooldown, wave, power) : base.GetCooldown();


    public override IEnumerator Cast(Vector3 castPosition, Vector3 targetPosition, Hittable.Team casterTeam)
    {
        this.last_cast = Time.time; 
        this.team = casterTeam;     

        if (data == null) {
            Debug.LogError($"BaseSpell [{this.GetName()}]: SpellData is null. Cannot cast.");
            yield break;
        }

        int manaCost = GetManaCost();
        if (owner.mana < manaCost) {
            Debug.LogWarning($"BaseSpell [{GetName()}]: Not enough mana. Needed {manaCost}, have {owner.mana}");
            yield break;
        }

        owner.mana -= manaCost;

        if (data.projectile != null)
        {
            float speed = (data.projectile != null && !string.IsNullOrEmpty(data.projectile.speed)) ? RPNEvaluatorFloat.Evaluate(data.projectile.speed, wave, power) : 15f;
            string trajectory = data.projectile.trajectory ?? "straight";
            int projectileSpriteIndex = (data.projectile != null) ? data.projectile.sprite : 0;
            float lifetime = (data.projectile != null && !string.IsNullOrEmpty(data.projectile.lifetime)) ? RPNEvaluatorFloat.Evaluate(data.projectile.lifetime, wave, power) : 2.0f;


            if (!string.IsNullOrEmpty(data.N) && !string.IsNullOrEmpty(data.spray)) 
            {
                int numProjectiles = RPNEvaluatorInt.Evaluate(data.N, wave, power);
                float sprayAngle = RPNEvaluatorFloat.Evaluate(data.spray, wave, power); 
                Vector3 direction = (targetPosition - castPosition).normalized;

                for (int i = 0; i < numProjectiles; i++)
                {
                    float angleOffset = (numProjectiles > 1) ? (-sprayAngle / 2) + (i * sprayAngle / (numProjectiles - 1)) : 0;
                    Quaternion rotation = Quaternion.Euler(0, 0, angleOffset) * Quaternion.LookRotation(Vector3.forward, direction);
                    Vector3 sprayDirection = rotation * Vector3.up;

                    if (GameManager.Instance.projectileManager != null)
                    {
                        GameManager.Instance.projectileManager.CreateProjectile(
                            projectileSpriteIndex, trajectory, castPosition, sprayDirection, speed, lifetime,
                            (Hittable other, Vector3 impactPoint) => { OnProjectileHit(other, impactPoint, data); },
                            data 
                        );
                    }
                }
            }
  
            else
            {
                if (GameManager.Instance.projectileManager != null)
                {
                     GameManager.Instance.projectileManager.CreateProjectile(
                        projectileSpriteIndex, trajectory, castPosition, (targetPosition - castPosition).normalized, speed, lifetime,
                        (Hittable other, Vector3 impactPoint) => { OnProjectileHit(other, impactPoint, data); },
                        data // Pass SpellData to projectile
                    );
                }
            }
        }

        yield return null;
    }


    protected virtual void OnProjectileHit(Hittable other, Vector3 impactPoint, SpellData spellData)
    {
        if (other.team == this.team) return; 

        int baseDamageAmount = (spellData.damage != null && !string.IsNullOrEmpty(spellData.damage.amount)) ? RPNEvaluatorInt.Evaluate(spellData.damage.amount, wave, power) : 0;
        Damage.Type damageType = (spellData.damage != null && !string.IsNullOrEmpty(spellData.damage.type)) ? Damage.TypeFromString(spellData.damage.type) : Damage.Type.PHYSICAL;
        
        if (baseDamageAmount > 0)
        {
            other.Damage(new Damage(baseDamageAmount, damageType));
        }

        if (spellData.effects != null)
        {
            foreach (var effectData in spellData.effects)
            {
                ApplyEffect(other, impactPoint, effectData, spellData);
            }
        }

        if (spellData.secondary_projectile != null && !string.IsNullOrEmpty(spellData.N) && !string.IsNullOrEmpty(spellData.secondary_damage))
        {
            HandleArcaneBlastSecondary(impactPoint, spellData);
        }
    }

    protected virtual void ApplyEffect(Hittable target, Vector3 impactPoint, EffectData effectData, SpellData spellData)
    {
        if (target == null || effectData == null) return;

        switch (effectData.type.ToLower())
        {
            case "chill":
                EnemyController ec = target.owner?.GetComponent<EnemyController>();
                if (ec != null)
                {
                    float duration = RPNEvaluatorFloat.Evaluate(effectData.duration, wave, power);
                    float slowFactor = RPNEvaluatorFloat.Evaluate(effectData.slow_factor, wave, power);
                    ec.ApplySlow(slowFactor, duration);
                }
                break;
        }
    }

    private void HandleArcaneBlastSecondary(Vector3 impactPoint, SpellData spellData)
    {
        int numSecondary = RPNEvaluatorInt.Evaluate(spellData.N, wave, power);
        int secondaryDamageAmount = RPNEvaluatorInt.Evaluate(spellData.secondary_damage, wave, power);
        Damage.Type primaryDamageType = (spellData.damage != null && !string.IsNullOrEmpty(spellData.damage.type)) ? Damage.TypeFromString(spellData.damage.type) : Damage.Type.ARCANE;


        float secondarySpeed = (spellData.secondary_projectile != null && !string.IsNullOrEmpty(spellData.secondary_projectile.speed)) ? RPNEvaluatorFloat.Evaluate(spellData.secondary_projectile.speed, wave, power) : 10f;
        string secondaryTrajectory = spellData.secondary_projectile.trajectory ?? "straight";
        int secondarySprite = (spellData.secondary_projectile != null) ? spellData.secondary_projectile.sprite : 0;
        float secondaryLifetime = (spellData.secondary_projectile != null && !string.IsNullOrEmpty(spellData.secondary_projectile.lifetime)) ? RPNEvaluatorFloat.Evaluate(spellData.secondary_projectile.lifetime, wave, power) : 0.5f;

        for (int i = 0; i < numSecondary; i++)
        {
            float angle = (360f / numSecondary) * i;
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
            if (GameManager.Instance.projectileManager != null)
            {
                GameManager.Instance.projectileManager.CreateProjectile(
                    secondarySprite, secondaryTrajectory, impactPoint, direction, secondarySpeed, secondaryLifetime,
                    (Hittable otherHit, Vector3 secondaryImpactPoint) => {
                        if (otherHit.team != this.team)
                        {
                            otherHit.Damage(new Damage(secondaryDamageAmount, primaryDamageType)); // Secondary inherits primary type for now
                        }
                    },
                    null // Secondary projectiles usually don't have further complex SpellData effects
                );
            }
        }
    }
}