using UnityEngine;

public class Relic
{
    private RelicTrigger trigger;
    private RelicEffect  effect;

    public string Name   { get; }
    public int    Sprite { get; }

    public Relic(RelicDefinition def, PlayerController player)
    {
        Name   = def.name;
        Sprite = def.sprite;

        // 1) Build the effect from the JSON definition
        effect = RelicEffectFactory.Create(def.effect, player);

        // 2) Build the trigger from the JSON definition
        trigger = RelicTriggerFactory.Create(def.trigger, effect, player);

        // 3) Start listening
        trigger.Register();
    }


    public void Cleanup()
    {
        trigger.Unregister();
    }
}
