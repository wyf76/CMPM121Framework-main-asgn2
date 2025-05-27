using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnIdleTrigger : RelicTrigger
{
    private float idleTime;
    private float threshold;
    private bool effectApplied;

    public OnIdleTrigger(RelicEffect effect, PlayerController player, string rpnAmount)
        : base(effect, player)
    {
        int currentWave = GameManager.Instance.wavesCompleted + 1;
        var vars = new Dictionary<string,float>
        {
            ["wave"]        = currentWave,
            ["spell_power"] = player.spellcaster.spellPower
        };
        threshold = RPNEvaluator.EvaluateFloat(rpnAmount, vars);
    }

    public override void Register()
    {
        RelicEventBus.OnPlayerMoved += ResetIdleTimer;
        player.StartCoroutine(IdleWatcher());
    }

    public override void Unregister()
    {
        RelicEventBus.OnPlayerMoved -= ResetIdleTimer;
    }

    private void ResetIdleTimer()
    {
        idleTime = 0f;
        effectApplied = false;
    }

    private IEnumerator IdleWatcher()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            idleTime += 1f;

            if (idleTime >= threshold && !effectApplied)
            {
                effect.Activate();
                effectApplied = true;
            }
        }
    }
}