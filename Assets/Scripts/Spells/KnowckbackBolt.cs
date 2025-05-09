using UnityEngine;
using System.Collections;
using System.Linq;

public class KnockbackBolt : BaseSpell
{
    public KnockbackBolt(SpellCaster owner, SpellData data, int wave, int power)
    : base(owner, data, wave, power) {}
    protected override void OnProjectileHit(Hittable other, Vector3 impactPoint, SpellData spellData)
    {
        // base damage and existing effects
        base.OnProjectileHit(other, impactPoint, spellData);

        // knockback logic
        var kb = spellData.effects?
            .FirstOrDefault(fx => fx.type.Equals("knockback", System.StringComparison.OrdinalIgnoreCase));
        if (kb == null) return;

        float force = RPNEvaluatorFloat.Evaluate(kb.force, wave, power);
        var victim = other.owner;
        if (victim == null) return;

        Vector3 dir = (victim.transform.position - impactPoint).normalized;
        var rb3d = victim.GetComponent<Rigidbody>();
        if (rb3d != null)
        {
            rb3d.AddForce(dir * force, ForceMode.Impulse);
            return;
        }
        var rb2d = victim.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.AddForce((Vector2)dir * force, ForceMode2D.Impulse);
            return;
        }
        victim.transform.position += dir * (force * 0.1f);
    }
}