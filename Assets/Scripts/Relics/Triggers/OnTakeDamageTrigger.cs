public class OnTakeDamageTrigger : RelicTrigger
{
    public OnTakeDamageTrigger(RelicEffect effect, PlayerController player) : base(effect, player) { }

    public override void Register()
    {
        RelicEventBus.OnPlayerDamaged += Handle;
    }

    public override void Unregister()
    {
        RelicEventBus.OnPlayerDamaged -= Handle;
    }

    void Handle(int dmg)
    {
        effect.Activate();
    }
}
