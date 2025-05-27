public abstract class RelicEffect
{
    protected PlayerController player;
    public RelicEffect(PlayerController player)
    {
        this.player = player;
    }

    public abstract void Activate();
}
