using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class FrostSpikeModifier : ModifierSpell
{
    private int   _pierceCount  = 1;
    private float _slowDuration = 2f;
    private float _slowFactor   = 0.5f;

    public FrostSpikeModifier(Spell inner) : base(inner) { }

    protected override string Suffix => "frost-spiked";

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        base.LoadAttributes(json, vars);
        foreach (var eff in json["effects"])
        {
            switch (eff["type"].Value<string>())
            {
                case "pierce":
                    _pierceCount  = int.Parse(eff["count"].Value<string>());
                    break;
                case "chill":
                    _slowDuration = float.Parse(eff["duration"].Value<string>());
                    _slowFactor   = float.Parse(eff["slow_factor"].Value<string>());
                    break;
            }
        }
    }

    // Required by ModifierSpell, even if we don't change any base stats here
    protected override void InjectMods(StatBlock mods)
    {
        // no stat modifications (pierce & slow are applied in ApplyModifierEffect)
    }

    protected override IEnumerator ApplyModifierEffect(Vector3 origin, Vector3 target)
    {
        // Record existing projectiles
        var before = Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);

        // Cast the inner spell
        yield return inner.TryCast(origin, target);

        // Find new projectiles and attach pierce/slow behavior
        var after = Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
        foreach (var proj in after)
        {
            if (System.Array.IndexOf(before, proj) < 0)
            {
                // Configure the new projectile
                proj.PierceCount = _pierceCount;
                proj.OnHit += (hit, pos) =>
                {
                    if (hit.team != owner.team)
                    {
                        hit.owner.GetComponent<EnemyController>()
                           ?.ApplySlow(_slowDuration, _slowFactor);
                    }
                };
            }
        }
    }
}
