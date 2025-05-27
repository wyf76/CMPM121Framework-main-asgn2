using System.Collections.Generic;
using UnityEngine;

public static class RelicEffectFactory
{
    public static RelicEffect Create(EffectDefinition def, PlayerController player)
    {
        switch (def.type)
        {
            case "gain-mana":
                return new GainManaEffect(player, def.amount);

            case "gain-spellpower":
                return new GainSpellPowerEffect(player, def.amount);

            case "buff-next-spellpower":
                return new BuffNextSpellPowerEffect(player, def.amount);
            
            case "reduce-mana-cost": 
                return new ReduceManaCostEffect(player, def.amount);

            case "auto-cast-retaliation":
                return new AutoCastRetaliationEffect(player, def.cooldown);

            default:
                Debug.LogError($"Unknown effect type '{def.type}'");
                return null;
        }
    }
}