using System.Collections.Generic;

public class BuffNextSpellPowerEffect : RelicEffect
{
    private int bonus;
    private bool active = true;

    public BuffNextSpellPowerEffect(PlayerController player, string rpnAmount) 
        : base(player)
    {
        int currentWave = GameManager.Instance.wavesCompleted + 1;
        var vars = new Dictionary<string,int>
        {
            ["wave"]        = currentWave,
            ["spell_power"] = player.spellcaster.spellPower
        };
        bonus = RPNEvaluator.Evaluate(rpnAmount, vars);
    }

    public override void Activate()
    {
        if (!active) return;
        player.spellcaster.spellPower += bonus;
        active = false;
    }
}