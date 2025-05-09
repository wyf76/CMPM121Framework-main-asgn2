using UnityEngine;
using System.Collections;

public class ModifierSpell : Spell
{
    protected readonly SpellData data;
    protected readonly Spell inner;
    protected readonly int wave;
    protected readonly int power;

    public ModifierSpell(
        SpellCaster owner,
        SpellData data,
        Spell inner,
        int wave,
        int power
    ) : base(owner)
    {
        this.data  = data;
        this.inner = inner;
        this.wave  = wave;
        this.power = power;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;

        if (data.name.ToLower().Contains("doubler"))
        {
            yield return inner.Cast(where, target, team);
            yield return new WaitForSeconds(0.2f);
            yield return inner.Cast(where, target, team);
        }
        else if (data.name.ToLower().Contains("splitter"))
        {
            yield return inner.Cast(where, target, team);
            Vector3 offsetDir = Quaternion.Euler(0, 0, 3f) * (target - where);
            yield return inner.Cast(where, where + offsetDir, team);
        }
        else
        {
            yield return inner.Cast(where, target, team);
        }
    }

    public override int GetManaCost()
    {
        int baseCost = inner.GetManaCost();
        if (data.name.ToLower().Contains("damage amp"))
            return Mathf.RoundToInt(baseCost * 1.5f);
        if (data.name.ToLower().Contains("chaos"))
            return baseCost + 5;
        return baseCost;
    }

    public override int GetDamage()
    {
        int baseDamage = inner.GetDamage();
        if (data.name.ToLower().Contains("damage amp"))
            return Mathf.RoundToInt(baseDamage * 1.5f);
        if (data.name.ToLower().Contains("chaos"))
            return Mathf.RoundToInt(baseDamage * 2f);
        if (data.name.ToLower().Contains("homing"))
            return Mathf.RoundToInt(baseDamage * 0.7f);
        return baseDamage;
    }

    public override float GetCooldown()  => inner.GetCooldown();
    public override int   GetIcon()      => inner.GetIcon();
}
