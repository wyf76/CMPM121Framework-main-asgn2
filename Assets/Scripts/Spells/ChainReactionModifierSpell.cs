using UnityEngine;
using System.Collections;
using System.Linq;

public class ChainReactionModifierSpell : ModifierSpell
{
    private readonly float _chance;
    private readonly float _radius;
    private readonly string _secondarySpellId;

    public ChainReactionModifierSpell(
        SpellCaster owner,
        SpellData data,
        Spell inner,
        int wave,
        int power
    ) : base(owner, data, inner, wave, power)
    {
        // pull from your effects list
        var fx = data.effects?.FirstOrDefault(e => e.type == "chain_reaction");
        if (fx != null)
        {
            _chance           = RPNEvaluatorFloat.Evaluate(fx.chance, wave, power);
            _radius           = RPNEvaluatorFloat.Evaluate(fx.radius, wave, power);
            _secondarySpellId = fx.secondary_spell_id;
        }
    }

    public override IEnumerator Cast(
        Vector3 castPosition,
        Vector3 targetPosition,
        Hittable.Team casterTeam
    )
    {
        // first do the wrapped spell
        yield return inner.Cast(castPosition, targetPosition, casterTeam);
    }

    // BaseSpell will call this for each projectile hit
    protected override void OnProjectileHit(
        Hittable other,
        Vector3 impactPoint,
        SpellData spellData
    )
    {
        // first apply normal damage/effects
        base.OnProjectileHit(other, impactPoint, spellData);

        if (_chance <= 0f || string.IsNullOrEmpty(_secondarySpellId))
            return;

        if (Random.value > _chance)
            return;

        // find next target
        var next = EnemyManager.Instance
            .GetNearestEnemy(impactPoint, _radius, exclude: other);
        if (next == null) return;

        // build & fire the secondary spell
        Vector3 dir = (next.Position - impactPoint).normalized;
        var builder = new SpellBuilder();
        var sec     = builder.Build(_secondarySpellId, owner, wave, power);
        owner.StartCoroutine(
            sec.Cast(impactPoint, impactPoint + dir, other.team)
        );
    }
}
