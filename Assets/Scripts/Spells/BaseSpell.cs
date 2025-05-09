using UnityEngine;
using System;


public class BaseSpell : Spell
{
    // Final base values for the spell 
    private int damage;
    private int secondaryDamage;
    private int manaCost;
    private float cooldown;
    private string damageType;
    // Projectile properties
    private string projectileTrajectory;
    private float projectileSpeed;
    private float projectileLifetime;
    private int projectileSprite;
    // Behavior properties
    private int projectileCount;   // how many projectiles to spawn on cast
    private int secondaryCount;    // how many projectiles to spawn on hit

    public BaseSpell(string name, string description, int iconIndex,
                     int damage, int secondaryDamage,
                     int manaCost, float cooldown, string damageType,
                     string projTrajectory, float projSpeed, float projLifetime, int projSprite,
                     int projectileCount = 1, int secondaryCount = 0)
    {
        this.Name = name;
        this.Description = description;
        this.IconIndex = iconIndex;
        this.damage = damage;
        this.secondaryDamage = secondaryDamage;
        this.manaCost = manaCost;
        this.cooldown = cooldown;
        this.damageType = damageType;
        this.projectileTrajectory = projTrajectory;
        this.projectileSpeed = projSpeed;
        this.projectileLifetime = projLifetime;
        this.projectileSprite = projSprite;
        this.projectileCount = projectileCount;
        this.secondaryCount = secondaryCount;
    }

    internal override void CastInternal(Vector3 spawnPosition, Vector3 direction, SpellModifierContext context)
    {
        // Apply all value modifiers from context to base values
        float finalDamage = ValueModifier.ApplyModifiers(context.damageModifiers, damage);
        float finalSecondaryDamage = secondaryDamage;
        if (secondaryDamage > 0)
        {
            finalSecondaryDamage = ValueModifier.ApplyModifiers(context.damageModifiers, secondaryDamage);
        }
        float finalSpeed = ValueModifier.ApplyModifiers(context.speedModifiers, projectileSpeed);
        float finalLifetime = (projectileLifetime > 0f) ? projectileLifetime : 5f;  // default lifetime if none specified

        // Determine final projectile type and sprite
        string finalTrajectory = projectileTrajectory;
        int finalSprite = projectileSprite;
        if (context.projectileOverride != null)
        {
            // Override trajectory, speed, sprite, lifetime if provided by a modifier
            if (!string.IsNullOrEmpty(context.projectileOverride.trajectory))
                finalTrajectory = context.projectileOverride.trajectory;
            if (!string.IsNullOrEmpty(context.projectileOverride.speed))
                finalSpeed = float.Parse(context.projectileOverride.speed);
            if (!string.IsNullOrEmpty(context.projectileOverride.lifetime))
                finalLifetime = float.Parse(context.projectileOverride.lifetime);
            // Use overridden sprite
            finalSprite = context.projectileOverride.sprite;
        }

        // Prepare secondary projectile parameters
        string secondaryTrajectory = projectileTrajectory;
        int secondarySprite = projectileSprite;
        float secondarySpeed = projectileSpeed;
        float secondaryLifetime = 0f;
        if (secondaryCount > 0)
        {
            // If secondary projectile info exists in data, it would be used here. For simplicity, use primary's values with possibly shorter lifetime.
            secondarySpeed = projectileSpeed;
            secondaryLifetime = 0.5f;  // short-lived secondaries by default
            if (secondaryLifetime <= 0f) secondaryLifetime = 2f;
        }

        // Spawn primary projectiles. If multiple, spread them in a cone
        if (projectileCount <= 1)
        {
            // Single projectile
            SpawnProjectile(finalSprite, finalTrajectory, spawnPosition, direction, finalSpeed, finalDamage, finalLifetime,
                            finalSecondaryDamage, secondaryTrajectory, secondarySprite, secondarySpeed, secondaryLifetime);
        }
        else
        {
            // Multiple projectiles. Spread evenly in an angle range.
            float spreadAngle = 60f;
            for (int i = 0; i < projectileCount; i++)
            {
                float angleOffset = (projectileCount > 1) ? (-spreadAngle / 2f + (spreadAngle / (projectileCount - 1)) * i) : 0f;
                Quaternion rot = Quaternion.Euler(0, 0, angleOffset);
                Vector3 newDir = rot * direction;
                SpawnProjectile(finalSprite, finalTrajectory, spawnPosition, newDir.normalized, finalSpeed, finalDamage, finalLifetime,
                                finalSecondaryDamage, secondaryTrajectory, secondarySprite, secondarySpeed, secondaryLifetime);
            }
        }
    }
    private void SpawnProjectile(
        int spriteId,
        string trajectory,
        Vector3 position,
        Vector3 direction,
        float speed,
        float damageAmount,
        float lifetime,
        float secondaryDamageAmount,
        string secondaryTrajectory,
        int secondarySprite,
        float secondarySpeed,
        float secondaryLifetime)
    {
        // Primary on‐hit callback
        Action<Hittable, Vector3> onHitCallback = (Hittable target, Vector3 hitPoint) =>
        {
            if (target != null)
            {
                // 1) Parse your string into the Damage.Type enum
                if (!Enum.TryParse<Damage.Type>(damageType, true, out var dmgTypeEnum))
                    dmgTypeEnum = Damage.Type.PHYSICAL; // fallback default

                var dmg = new Damage(Mathf.RoundToInt(damageAmount), dmgTypeEnum);
                target.Damage(dmg);
            }

            // 2) Secondary explosion bolts
            if (secondaryCount > 0)
            {
                float secDamage = (secondaryDamageAmount > 0f) ? secondaryDamageAmount : damageAmount;

                // Pre‐parse once for efficiency
                if (!Enum.TryParse<Damage.Type>(damageType, true, out var secTypeEnum))
                    secTypeEnum = Damage.Type.PHYSICAL;

                for (int j = 0; j < secondaryCount; j++)
                {
                    float angle = 360f * j / secondaryCount;
                    Vector3 secDir = Quaternion.Euler(0, 0, angle) * Vector3.right;

                    // Secondary on‐hit callback
                    Action<Hittable, Vector3> secondaryHit = (Hittable secTarget, Vector3 secPoint) =>
                    {
                        if (secTarget != null)
                        {
                            var secDmg = new Damage(Mathf.RoundToInt(secDamage), secTypeEnum);
                            secTarget.Damage(secDmg);
                        }
                    };

                    // 3) Spawn the secondary projectile
                    ProjectileManager.Instance.CreateProjectile(
                        secondarySprite,
                        secondaryTrajectory,
                        hitPoint,
                        secDir.normalized,
                        secondarySpeed,
                        secondaryLifetime,
                        secondaryHit
                    );
                }
            }
        };

        // 4) Spawn the primary projectile
        ProjectileManager.Instance.CreateProjectile(
            spriteId,
            trajectory,
            position,
            direction.normalized,
            speed,
            lifetime,
            onHitCallback
        );
    }


    // Getters for base spell properties
    public override int GetManaCost()    => manaCost;
    public override float GetDamage()    => damage;
    public override float GetCooldown()  => cooldown;
    public override string GetProjectileType()   => projectileTrajectory;
    public override int GetProjectileSprite()    => projectileSprite;
    public override float GetProjectileSpeed()   => projectileSpeed;
    public override float GetProjectileLifetime() => projectileLifetime;
}
