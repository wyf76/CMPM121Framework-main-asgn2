using UnityEngine;

public class OnKillEnemyTrigger : RelicTrigger
{
    public OnKillEnemyTrigger(RelicEffect effect, PlayerController player) : base(effect, player) { }

    public override void Register()
    {
        RelicEventBus.OnEnemyKilled += Handle;
    }

    public override void Unregister()
    {
        RelicEventBus.OnEnemyKilled -= Handle;
    }

    void Handle(GameObject enemy)
    {
        effect.Activate();
    }
}
