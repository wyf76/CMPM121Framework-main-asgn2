using UnityEngine;

public class ReduceManaCostEffect : RelicEffect
{
    private int amount;

    public ReduceManaCostEffect(PlayerController player, string rpnAmount) : base(player)
    {
        amount = RPNEvaluator.SafeEvaluate(rpnAmount, null, 10);
    }

    public override void Activate()
    {
        // This will add a discount to the player's next spell cast
        if (player.spellcaster != null)
        {
            player.spellcaster.nextSpellManaDiscount += amount;
        }
    }
}