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

    protected override void OnProjectileHit(
        Hittable other,
        Vector3 impactPoint,
        SpellData spellData
    )
    {
        base.OnProjectileHit(other, impactPoint, spellData);

        if (_lifeStealPct <= 0f) return;
        int dmg = RPNEvaluatorInt.Evaluate(spellData.damage.amount, wave, power);
        int heal = Mathf.RoundToInt(dmg * _lifeStealPct);
        owner.Heal(heal);
    }
}
