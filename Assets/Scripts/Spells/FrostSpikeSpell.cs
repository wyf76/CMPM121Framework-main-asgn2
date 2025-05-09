using UnityEngine;
using System.Collections;

public class FrostSpikeSpell : BaseSpell
{
    public FrostSpikeSpell(
        SpellCaster owner,
        SpellData data,
        int wave,
        int power
    ) : base(owner, data, wave, power)
    { }

    public override IEnumerator Cast(
        Vector3 castPosition,
        Vector3 targetPosition,
        Hittable.Team casterTeam
    )
    {
        // BaseSpell handles:
        //  • mana check
        //  • CreateProjectile(...)
        //  • OnProjectileHit → base damage + any data.effects of type "chill"
        yield return base.Cast(castPosition, targetPosition, casterTeam);
    }
}
