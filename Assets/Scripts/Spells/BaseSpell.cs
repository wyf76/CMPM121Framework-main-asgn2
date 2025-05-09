using UnityEngine;
using System.Collections;
using System.Collections.Generic; // For List<EffectData>
using System.Linq;               // For FirstOrDefault()

public class BaseSpell : Spell
{
    protected SpellData data;
    protected int wave;
    protected int power;

    public BaseSpell(SpellCaster owner, SpellData data, int wave, int power)
        : base(owner)
    {
        this.data  = data;
        this.wave  = wave;
        this.power = power;
    }

    public override string GetName() =>
        data?.name ?? base.GetName();

    public override string GetDescription() =>
        data?.description ?? base.GetDescription();

    public override int GetIcon() =>
        data != null ? data.icon : base.GetIcon();

    public override int GetManaCost() =>
        data != null && !string.IsNullOrEmpty(data.mana_cost)
            ? RPNEvaluatorInt.Evaluate(data.mana_cost, wave, power)
            : base.GetManaCost();

    public override int GetDamage() =>
        data != null && data.damage != null && !string.IsNullOrEmpty(data.damage.amount)
            ? RPNEvaluatorInt.Evaluate(data.damage.amount, wave, power)
            : base.GetDamage();

    public override float GetCooldown() =>
        data != null && !string.IsNullOrEmpty(data.cooldown)
            ? RPNEvaluatorFloat.Evaluate(data.cooldown, wave, power)
            : base.GetCooldown();

    public override IEnumerator Cast(
        Vector3 castPosition,
        Vector3 targetPosition,
        Hittable.Team casterTeam
    )
    {
        last_cast = Time.time;
        team      = casterTeam;

        if (data == null)
        {
            Debug.LogError($"BaseSpell [{GetName()}]: SpellData is null. Cannot cast.");
            yield break;
        }

        int manaCost = GetManaCost();
        if (owner.mana < manaCost)
        {
            Debug.LogWarning($"BaseSpell [{GetName()}]: Not enough mana. Needed {manaCost}, have {owner.mana}");
            yield break;
        }

        owner.mana -= manaCost;

        if (data.projectile != null)
        {
            // read projectile params
            float speed = !string.IsNullOrEmpty(data.projectile.speed)
                ? RPNEvaluatorFloat.Evaluate(data.projectile.speed, wave, power)
                : 15f;
            string traj = data.projectile.trajectory ?? "straight";
            int sprite  = data.projectile.sprite;
            float life  = !string.IsNullOrEmpty(data.projectile.lifetime)
                ? RPNEvaluatorFloat.Evaluate(data.projectile.lifetime, wave, power)
                : 2f;

            // N‐shot spray?
            if (!string.IsNullOrEmpty(data.N) && !string.IsNullOrEmpty(data.spray))
            {
                int n = RPNEvaluatorInt.Evaluate(data.N, wave, power);
                float angle = RPNEvaluatorFloat.Evaluate(data.spray, wave, power);
                Vector3 dir = (targetPosition - castPosition).normalized;

                for (int i = 0; i < n; i++)
                {
                    float offset = n > 1
                        ? (-angle/2f) + (i*angle/(n-1))
                        : 0;
                    Quaternion rot = Quaternion.Euler(0,0,offset)
                                     * Quaternion.LookRotation(Vector3.forward, dir);
                    Vector3 sdir = rot * Vector3.up;

                    ProjectileManager.Instance.CreateProjectile(
                        sprite, traj, castPosition, sdir,
                        speed, life,
                        (h, hitPoint) => OnProjectileHit(h, hitPoint, data),
                        data
                    );
                }
            }
            else
            {
                Vector3 dir = (targetPosition - castPosition).normalized;
                ProjectileManager.Instance.CreateProjectile(
                    sprite, traj, castPosition, dir,
                    speed, life,
                    (h, hitPoint) => OnProjectileHit(h, hitPoint, data),
                    data
                );
            }
        }

        yield return null;
    }

    protected virtual void OnProjectileHit(
        Hittable other,
        Vector3 impactPoint,
        SpellData spellData
    )
    {
        if (other.team == team) return;

        // — base damage —
        int dmg = RPNEvaluatorInt.Evaluate(spellData.damage.amount, wave, power);
        other.Damage(new Damage(dmg, Damage.TypeFromString(spellData.damage.type)));

        // — loop JSON effects —
        if (spellData.effects != null)
        {
            foreach (var fx in spellData.effects)
            {
                switch (fx.type.ToLower())
                {
                    // Chill
                    case "chill":
                    {
                        var ec = other.owner?.GetComponent<EnemyController>();
                        if (ec != null)
                        {
                            float dur  = RPNEvaluatorFloat.Evaluate(fx.duration, wave, power);
                            float slow = RPNEvaluatorFloat.Evaluate(fx.slow_factor, wave, power);
                            ec.ApplySlow(slow, dur);
                        }
                    }
                    break;

                    // Lifesteal
                    case "lifesteal":
                    {
                        int baseDmg = RPNEvaluatorInt.Evaluate(spellData.damage.amount, wave, power);
                        float pct   = RPNEvaluatorFloat.Evaluate(fx.percent, wave, power);
                        var hitter  = owner.CasterGameObject.GetComponent<Hittable>();
                        if (hitter != null)
                            hitter.Heal(Mathf.RoundToInt(baseDmg * pct));
                    }
                    break;
                }
            }
        }

        // — existing secondary_projectile logic —
        if (spellData.secondary_projectile != null
            && !string.IsNullOrEmpty(spellData.N)
            && !string.IsNullOrEmpty(spellData.secondary_damage))
        {
            HandleArcaneBlastSecondary(impactPoint, spellData);
        }
    }

    private void HandleArcaneBlastSecondary(Vector3 impactPoint, SpellData spellData)
    {
        int count = RPNEvaluatorInt.Evaluate(spellData.N, wave, power);
        int subDmg = RPNEvaluatorInt.Evaluate(spellData.secondary_damage, wave, power);
        Damage.Type subType = spellData.damage != null && !string.IsNullOrEmpty(spellData.damage.type)
            ? Damage.TypeFromString(spellData.damage.type)
            : Damage.Type.ARCANE;

        float speed = !string.IsNullOrEmpty(spellData.secondary_projectile?.speed)
            ? RPNEvaluatorFloat.Evaluate(spellData.secondary_projectile.speed, wave, power)
            : 10f;
        string traj = spellData.secondary_projectile?.trajectory ?? "straight";
        int spr     = spellData.secondary_projectile?.sprite ?? 0;
        float life  = !string.IsNullOrEmpty(spellData.secondary_projectile?.lifetime)
            ? RPNEvaluatorFloat.Evaluate(spellData.secondary_projectile.lifetime, wave, power)
            : 0.5f;

        for (int i = 0; i < count; i++)
        {
            float a = (360f / count) * i;
            Vector3 dir = Quaternion.Euler(0,0,a) * Vector3.up;
            ProjectileManager.Instance.CreateProjectile(
                spr, traj, impactPoint, dir,
                speed, life,
                (h2, pt2) => {
                    if (h2.team != team)
                        h2.Damage(new Damage(subDmg, subType));
                },
                null
            );
        }
    }
}
