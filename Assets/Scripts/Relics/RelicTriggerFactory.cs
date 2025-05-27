using UnityEngine;         
using System;

public static class RelicTriggerFactory
{
    public static RelicTrigger Create(TriggerDefinition data, RelicEffect effect, PlayerController player)
    {
        switch (data.type)
        {
            case "take-damage":
                return new OnTakeDamageTrigger(effect, player);

            case "on-kill":
                return new OnKillEnemyTrigger(effect, player);

            case "stand-still":      
                return new OnIdleTrigger(effect, player, data.amount);

            case "on-miss": 
                return new OnMissTrigger(effect, player);

            case "on-move-distance": 
                return new OnMoveDistanceTrigger(effect, player, data.amount);

            default:
                Debug.LogError($"RelicTriggerFactory: Unknown trigger type '{data.type}'");
                return null;
        }
    }
}