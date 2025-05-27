public abstract class RelicTrigger
{
    protected RelicEffect effect;
    protected PlayerController player;

    public RelicTrigger(RelicEffect effect, PlayerController player)
    {
        this.effect = effect;
        this.player = player;
        Register(); // default: auto-register on construction
    }

    public abstract void Register();
    public virtual void Unregister() { } // optional cleanup
}
