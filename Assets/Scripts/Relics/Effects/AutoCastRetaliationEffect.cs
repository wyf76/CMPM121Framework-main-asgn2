using UnityEngine;
using System.Collections;

public class AutoCastRetaliationEffect : RelicEffect
{
    private float cooldown;
    private float lastCastTime = -999f; // Initialize to allow immediate first cast

    public AutoCastRetaliationEffect(PlayerController player, string rpnCooldown) : base(player)
    {
        cooldown = RPNEvaluator.SafeEvaluate(rpnCooldown, null, 10);
    }

    public override void Activate()
    {
        if (Time.time >= lastCastTime + cooldown)
        {
            if (player.spellcaster != null && player.spellcaster.spells.Count > 0)
            {
                // Find the nearest enemy to retaliate against
                GameObject closestEnemy = GameManager.Instance.GetClosestEnemy(player.transform.position);
                if (closestEnemy != null)
                {
                    // Cast the first available spell
                    player.StartCoroutine(player.spellcaster.CastSlot(0, player.transform.position, closestEnemy.transform.position));
                    lastCastTime = Time.time;
                }
            }
        }
    }
}