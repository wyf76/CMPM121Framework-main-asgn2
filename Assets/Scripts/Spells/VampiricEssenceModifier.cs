using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class VampiricEssenceModifier : ModifierSpell
{
    private float _lifeStealPercent = 0.2f;
    private string _suffix = "vampiric";

    public VampiricEssenceModifier(Spell inner) : base(inner) { }

    protected override string Suffix => _suffix;

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        base.LoadAttributes(json, vars);
        _suffix = json["name"]?.Value<string>() ?? _suffix;

        var eff = json["effects"]?[0];
        if (eff != null && eff["type"].Value<string>() == "lifesteal")
            _lifeStealPercent = float.Parse(eff["percent"].Value<string>());
    }

    // No base stats are modified by vampiric essence
    protected override void InjectMods(StatBlock mods) { }

    protected override IEnumerator ApplyModifierEffect(Vector3 origin, Vector3 target)
    {
        // Capture existing projectiles
        var before = Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);

        // Cast the inner spell
        yield return inner.TryCast(origin, target);

        // Hook only the newly spawned projectiles
        var after = Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
        foreach (var proj in after)
        {
            if (System.Array.IndexOf(before, proj) < 0)
            {
                proj.OnHit += (hitReceiver, impactPos) =>
                {
                    if (hitReceiver.team != owner.team)
                    {
                        // Compute heal amount from this spell's damage
                        int healAmt = Mathf.RoundToInt(inner.Damage * _lifeStealPercent);

                        // Find the caster's Hittable and heal it
                        var casterHittable = owner.GetComponent<Hittable>();
                        if (casterHittable != null)
                            casterHittable.Heal(healAmt);
                    }
                };
            }
        }
    }
}
