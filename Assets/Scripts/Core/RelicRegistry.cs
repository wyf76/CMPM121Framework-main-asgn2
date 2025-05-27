using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public static class RelicRegistry
{
    // All definitions loaded from JSON
    private static List<RelicDefinition> defs => GameDataLoader.Relics;

    // Picks N random relics
    public static RelicDefinition[] GetRandomChoices(int count)
    {
        return defs
            .OrderBy(x => Random.value)
            .Take(count)
            .ToArray();
    }

    // Instantiates the trigger+effect for that relic and registers it
    public static void Apply(RelicDefinition def)
    {
        var pc = GameManager.Instance.player.GetComponent<PlayerController>();

        var effect = RelicEffectFactory.Create(def.effect, pc);

        var trigger = RelicTriggerFactory.Create(def.trigger, effect, pc);
        trigger.Register();

        GameManager.Instance.relicIconManager.Spawn(def.sprite);
    }
}
