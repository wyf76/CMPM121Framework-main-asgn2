using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


// Modifier that casts the underlying spell twice with a configurable delay,
// increasing mana cost and cooldown duration accordingly.

public sealed class Doubler : ModifierSpell
{
    private float _delay;
    private float _manaMultiplier;
    private float _cooldownMultiplier;
    private string _suffix;

    public Doubler(Spell inner) : base(inner)
    {
        // Default values
        _delay               = 0.5f;
        _manaMultiplier      = 1.5f;
        _cooldownMultiplier  = 1.5f;
        _suffix              = "doubled";
    }

    protected override string Suffix => _suffix;

    // Loads custom values from JSON, overriding defaults if present.

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        _suffix = json.Value<string>("name") ?? _suffix;
        if (json.TryGetValue("delay", out var d))
            _delay = RPNEvaluator.SafeEvaluateFloat(d.Value<string>(), vars, _delay);
        if (json.TryGetValue("mana_multiplier", out var mm))
            _manaMultiplier = RPNEvaluator.SafeEvaluateFloat(mm.Value<string>(), vars, _manaMultiplier);
        if (json.TryGetValue("cooldown_multiplier", out var cm))
            _cooldownMultiplier = RPNEvaluator.SafeEvaluateFloat(cm.Value<string>(), vars, _cooldownMultiplier);

        base.LoadAttributes(json, vars);
    }

    // Applies the mana and cooldown multipliers to the spell's stats.
    protected override void InjectMods(StatBlock stats)
    {
        stats.ManaMods.Add(new ValueMod(ModOp.Mul,     _manaMultiplier));
        stats.CooldownMods.Add(new ValueMod(ModOp.Mul, _cooldownMultiplier));
    }


    // Casts the inner spell twice: immediately and again after the delay.
    // Recomputes start/end so the second cast originates from the caster's current position.

    protected override IEnumerator ApplyModifierEffect(Vector3 origin, Vector3 target)
    {
        // First cast
        yield return inner.TryCast(origin, target);

        // Wait before second cast
        yield return new WaitForSeconds(_delay);

        // Compute new origin and target based on caster's updated position
        var casterPos = Owner.transform.position;
        var direction = (target - origin).normalized;
        var secondTarget = casterPos + direction * Vector3.Distance(origin, target);

        // Second cast
        yield return inner.TryCast(casterPos, secondTarget);
    }
}
