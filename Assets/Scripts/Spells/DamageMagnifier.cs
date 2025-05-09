using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// Modifier that increases a spell's damage and mana cost by specified multipliers.

public sealed class DamageMagnifier : ModifierSpell
{
    private float damageMultiplier = 1.5f;
    private float manaMultiplier = 1.5f;
    private string modifierName = "damage-amplified";

    public DamageMagnifier(Spell innerSpell) : base(innerSpell) { }

    protected override string Suffix => modifierName;

    // Load damage and mana multipliers from JSON, falling back to defaults.
 
    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        modifierName     = json.Value<string>("name") ?? modifierName;
        damageMultiplier = json.Value<float?>("damage_multiplier") ?? damageMultiplier;
        manaMultiplier   = json.Value<float?>("mana_multiplier") ?? manaMultiplier;
        base.LoadAttributes(json, vars);
    }


    // Injects this modifier's multipliers into the spell's stat block.

    protected override void InjectMods(StatBlock stats)
    {
        stats.DamageMods.Add(new ValueMod(ModOp.Mul, damageMultiplier));
        stats.ManaMods.Add(new ValueMod(ModOp.Mul, manaMultiplier));
    }
}
