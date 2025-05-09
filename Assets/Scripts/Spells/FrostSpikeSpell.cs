using UnityEngine;
using System.Collections;
using System.Linq;

public class FrostSpikeModifierSpell : ModifierSpell
{
    public FrostSpikeModifierSpell(
    SpellCaster owner,
    SpellData data,
    Spell inner,
    int wave,
    int power
    ) : base(owner, data, inner, wave, power) {}

    public override IEnumerator Cast(
        Vector3 castPosition,
        Vector3 targetPosition,
        Hittable.Team casterTeam
    )
    {
        // add chill and pierce via JSON effects handled by BaseSpell.OnProjectileHit
        yield return base.Cast(castPosition, targetPosition, casterTeam);
    }
}
