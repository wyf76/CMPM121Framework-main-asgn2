using System.Collections.Generic;
using UnityEngine;

public class GainManaEffect : RelicEffect
{
    private int amount;

    public GainManaEffect(PlayerController player, string rpnAmount) 
        : base(player)
    {
        int currentWave = GameManager.Instance.wavesCompleted + 1;
        var vars = new Dictionary<string,int>
        {
            ["wave"]        = currentWave,
            ["spell_power"] = player.spellcaster.spellPower
        };
        amount = RPNEvaluator.Evaluate(rpnAmount, vars);
    }

    public override void Activate()
    {
        player.spellcaster.mana = Mathf.Min(
            player.spellcaster.max_mana, 
            player.spellcaster.mana + amount
        );
    }
}