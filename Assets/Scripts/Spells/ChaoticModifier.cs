using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class ChaoticModifier : ModifierSpell
{
    // damage multiplier
    private float _damageMultiplier = 1.5f;
    private string _suffix = "chaotic";

    public ChaoticModifier(Spell inner) : base(inner) { }

    protected override string Suffix => _suffix;

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        if (json.TryGetValue("name", out var nameToken))
            _suffix = nameToken.Value<string>();

        if (json.TryGetValue("damage_multiplier", out var dm))
            _damageMultiplier = RPNEvaluator.SafeEvaluateFloat(dm.Value<string>(), vars, _damageMultiplier);

        base.LoadAttributes(json, vars);
    }

    // apply damage multiplier
    protected override void InjectMods(StatBlock mods)
    {
        mods.DamageMods.Add(new ValueMod(ModOp.Mul, _damageMultiplier));
    }

    protected override IEnumerator ApplyModifierEffect(Vector3 origin, Vector3 target)
    {
        if (inner is ArcaneSpray)
            yield return SpraySpiral(origin, target);
        else if (inner is ArcaneBlast)
            yield return BlastSpiral(origin, target);
        else
            yield return GenericSpiral(origin, target);
    }

    private IEnumerator SpraySpiral(Vector3 origin, Vector3 target)
    {
        float baseAngle = Mathf.Atan2(target.y - origin.y, target.x - origin.x) * Mathf.Rad2Deg;
        int count = Mathf.RoundToInt(Damage) + 5;
        float step = 60f / (count - 1);
        float start = baseAngle - 30f;

        for (int i = 0; i < count; i++)
        {
            float angle = start + step * i;
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameManager.Instance.projectileManager.CreateProjectile(
                inner.IconIndex,
                "spiraling",
                origin,
                dir,
                inner.Speed,
                (hit, _) =>
                {
                    if (hit.team != owner.team)
                        hit.Damage(new global::Damage(Mathf.RoundToInt(Damage), global::Damage.Type.ARCANE));
                },
                0.1f + inner.Speed / 40f
            );
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator BlastSpiral(Vector3 origin, Vector3 target)
    {
        Vector3 dir = (target - origin).normalized;
        GameManager.Instance.projectileManager.CreateProjectile(
            inner.IconIndex,
            "spiraling",
            origin,
            dir,
            inner.Speed,
            (hit, pos) =>
            {
                if (hit.team != owner.team)
                {
                    int dmg = Mathf.RoundToInt(Damage);
                    hit.Damage(new global::Damage(dmg, global::Damage.Type.ARCANE));
                    SecondaryExplosion(pos, dmg / 4);
                }
            });
        yield return null;
    }

    private void SecondaryExplosion(Vector3 center, int damage)
    {
        int count = 8;
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float rad = i * step * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
            GameManager.Instance.projectileManager.CreateProjectile(
                inner.IconIndex,
                "spiraling",
                center,
                dir,
                inner.Speed * 0.8f,
                (hit, _) =>
                {
                    if (hit.team != owner.team)
                        hit.Damage(new global::Damage(damage, global::Damage.Type.ARCANE));
                },
                0.3f
            );
        }
    }

    private IEnumerator GenericSpiral(Vector3 origin, Vector3 target)
    {
        GameManager.Instance.projectileManager.CreateProjectile(
            inner.IconIndex,
            "spiraling",
            origin,
            (target - origin).normalized,
            inner.Speed,
            (hit, _) =>
            {
                if (hit.team != owner.team)
                    hit.Damage(new global::Damage(Mathf.RoundToInt(Damage), global::Damage.Type.ARCANE));
            });
        yield return null;
    }
}
