using UnityEngine;
using System.Collections;
using System.Linq;

public class VampiricEssenceModifierSpell : ModifierSpell
{
    private readonly float _lifeStealPct;

    public VampiricEssenceModifierSpell(
        SpellCaster owner,
        SpellData data,
        Spell inner,
        int wave,
        int power
    ) : base(owner, data, inner, wave, power)
    {
        var fx = data.effects?.FirstOrDefault(e => e.type == "lifesteal");
        if (fx != null)
            _lifeStealPct = RPNEvaluatorFloat.Evaluate(fx.percent, wave, power);
    }

    public override IEnumerator Cast(
        Vector3 castPosition,
        Vector3 targetPosition,
        Hittable.Team casterTeam
    )
    {
        yield return inner.Cast(castPosition, targetPosition, casterTeam);
    }
}
