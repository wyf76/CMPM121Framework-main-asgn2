using UnityEngine;

public class OnMissTrigger : RelicTrigger
{
    public OnMissTrigger(RelicEffect effect, PlayerController player) : base(effect, player) { }

    public override void Register()
    {
        // Subscribing to the new OnSpellMissed event
        RelicEventBus.OnSpellMissed += Handle;
    }

    public override void Unregister()
    {
        RelicEventBus.OnSpellMissed -= Handle;
    }

    void Handle()
    {
        effect.Activate();
    }
}