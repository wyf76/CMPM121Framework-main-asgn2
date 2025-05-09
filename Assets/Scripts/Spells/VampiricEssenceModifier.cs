using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class VampiricEssenceModifier : ModifierSpell
{
    // lifesteal percentage and modifier suffix
    private float _lifeStealPercent = 0.2f;
    private string _modifierName   = "vampiric";

    public VampiricEssenceModifier(Spell inner) : base(inner) { }

    protected override string Suffix => _modifierName;

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        if (json.TryGetValue("name", out var nameToken))
            _modifierName = nameToken.Value<string>();

        if (json.TryGetValue("effects", out var effectsToken) && effectsToken is JArray effects && effects.Count > 0)
        {
            var eff = effects[0];
            if (eff.Value<string>("type") == "lifesteal")
            {
                var pct = eff.Value<string>("percent");
                if (float.TryParse(pct, out var parsed))
                    _lifeStealPercent = parsed;
            }
        }

        base.LoadAttributes(json, vars);
    }

    protected override void InjectMods(StatBlock mods)
    {
        // no stat changes
    }

    protected override IEnumerator ApplyModifierEffect(Vector3 origin, Vector3 target)
    {
        var before = Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
        yield return inner.TryCast(origin, target);
        var after = Object.FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);

        foreach (var proj in after)
        {
            if (System.Array.IndexOf(before, proj) < 0)
            {
                proj.OnHit += (hitReceiver, _) =>
                {
                    if (hitReceiver.team != owner.team)
                    {
                        int healAmt = Mathf.RoundToInt(inner.Damage * _lifeStealPercent);
                        var hitter = owner.GetComponent<Hittable>();
                        if (hitter != null)
                            hitter.Heal(healAmt);
                    }
                };
            }
        }
    }
}
