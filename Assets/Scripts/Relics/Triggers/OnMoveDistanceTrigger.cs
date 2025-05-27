using UnityEngine;

public class OnMoveDistanceTrigger : RelicTrigger
{
    private float distanceThreshold;
    private float distanceMoved;

    public OnMoveDistanceTrigger(RelicEffect effect, PlayerController player, string amount) : base(effect, player)
    {
        // Using RPNEvaluator to get the distance from the JSON
        distanceThreshold = RPNEvaluator.SafeEvaluate(amount, null, 50);
        distanceMoved = 0;
    }

    public override void Register()
    {
        // Subscribing to the new OnPlayerMovedDistance event
        RelicEventBus.OnPlayerMovedDistance += Handle;
    }

    public override void Unregister()
    {
        RelicEventBus.OnPlayerMovedDistance -= Handle;
    }

    void Handle(float distance)
    {
        distanceMoved += distance;
        if (distanceMoved >= distanceThreshold)
        {
            effect.Activate();
            distanceMoved = 0; // Reset after triggering
        }
    }
}