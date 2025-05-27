using System.Collections.Generic;

public class GainSpellPowerEffect : RelicEffect
{
    private int amount;

    public GainSpellPowerEffect(PlayerController player, string rpnAmount) 
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
        player.spellcaster.spellPower += amount;
    }
}