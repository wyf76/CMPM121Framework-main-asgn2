using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class HomingModifier : ModifierSpell
{
    // damage reduction and mana add
    private float _damageMultiplier;
    private float _manaAdder;
    private string _suffix;

    public HomingModifier(Spell inner) : base(inner)
    {
        _damageMultiplier = 0.75f;
        _manaAdder        = 10f;
        _suffix           = "homing";
    }

    protected override string Suffix => _suffix;

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        if (json.TryGetValue("name", out var n))
            _suffix = n.Value<string>();

        if (json.TryGetValue("damage_multiplier", out var dm))
            _damageMultiplier = RPNEvaluator.SafeEvaluateFloat(dm.Value<string>(), vars, _damageMultiplier);

        if (json.TryGetValue("mana_adder", out var ma))
            _manaAdder = RPNEvaluator.SafeEvaluateFloat(ma.Value<string>(), vars, _manaAdder);

        base.LoadAttributes(json, vars);
    }

    // apply damage and mana modifiers
    protected override void InjectMods(StatBlock mods)
    {
        mods.DamageMods.Add(new ValueMod(ModOp.Mul, _damageMultiplier));
        mods.ManaMods.Add(new ValueMod(ModOp.Add, _manaAdder));
    }

    protected override IEnumerator ApplyModifierEffect(Vector3 origin, Vector3 target)
    {
        // choose behavior based on underlying spell
        if (inner is ArcaneSpray)
            yield return SprayHoming(origin, target);
        else if (inner is ArcaneBlast)
            yield return BlastHoming(origin, target);
        else
            yield return GenericHoming(origin, target);
    }

    private IEnumerator SprayHoming(Vector3 origin, Vector3 target)
    {
        Vector3 dir = (target - origin).normalized;
        float ang   = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        int count   = Mathf.Max(1, Mathf.RoundToInt(Damage) + 5);
        float span  = 60f;
        float step  = span / (count - 1);
        float start = ang - span / 2;

        for (int i = 0; i < count; i++)
        {
            float a = start + step * i;
            Vector3 d = new Vector3(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));

            // aim at closest enemy or spray direction
            var enemy = GameManager.Instance.GetClosestEnemy(origin);
            Vector3 tgt = enemy ? (enemy.transform.position - origin).normalized : d;

            GameManager.Instance.projectileManager.CreateProjectile(
                inner.IconIndex,
                "homing",
                origin,
                tgt,
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

    private IEnumerator BlastHoming(Vector3 origin, Vector3 target)
    {
        // fire single homing missile then secondary homing explosion
        var enemy = GameManager.Instance.GetClosestEnemy(origin);
        Vector3 dir = enemy ? (enemy.transform.position - origin).normalized : (target - origin).normalized;

        GameManager.Instance.projectileManager.CreateProjectile(
            inner.IconIndex,
            "homing",
            origin,
            dir,
            inner.Speed,
            (hit, pos) =>
            {
                if (hit.team != owner.team)
                {
                    hit.Damage(new global::Damage(Mathf.RoundToInt(Damage), global::Damage.Type.ARCANE));
                    CreateSecondaryHoming(pos, Mathf.RoundToInt(Damage) / 4);
                }
            }
        );
        yield return null;
    }

    private void CreateSecondaryHoming(Vector3 center, int dmg)
    {
        int count = 8;
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float a = i * step * Mathf.Deg2Rad;
            Vector3 d = new Vector3(Mathf.Cos(a), Mathf.Sin(a));

            GameManager.Instance.projectileManager.CreateProjectile(
                inner.IconIndex,
                "homing",
                center,
                d,
                inner.Speed * 0.8f,
                (hit, _) =>
                {
                    if (hit.team != owner.team)
                        hit.Damage(new global::Damage(dmg, global::Damage.Type.ARCANE));
                },
                0.3f
            );
        }
    }

    private IEnumerator GenericHoming(Vector3 origin, Vector3 target)
    {
        var enemy = GameManager.Instance.GetClosestEnemy(origin);
        Vector3 dir = enemy ? (enemy.transform.position - origin).normalized : (target - origin).normalized;

        GameManager.Instance.projectileManager.CreateProjectile(
            inner.IconIndex,
            "homing",
            origin,
            dir,
            inner.Speed,
            (hit, _) =>
            {
                if (hit.team != owner.team)
                    hit.Damage(new global::Damage(Mathf.RoundToInt(Damage), global::Damage.Type.ARCANE));
            }
        );
        yield return null;
    }
}
