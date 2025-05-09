<<<<<<< HEAD
<<<<<<< HEAD
﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
=======
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
>>>>>>> 1e7a3e8 (some updates)
=======
/// ModifierSpell.cs
using UnityEngine;
using System;
using UnityEngine;
>>>>>>> 22ff77c (getting there)

public abstract class ModifierSpell : Spell
{
<<<<<<< HEAD
<<<<<<< HEAD
    protected readonly Spell inner;

    protected ModifierSpell(Spell inner) : base(inner.Owner)
    {
        this.inner = inner;
        mods = new StatBlock();
        InjectMods(mods);
    }

    public Spell InnerSpell => inner;
    public override string DisplayName => $"{inner.DisplayName} {Suffix}";
    public override int IconIndex => inner.IconIndex;
    protected abstract string Suffix { get; }


    protected override float BaseDamage   => inner.Damage;
    protected override float BaseMana     => inner.Mana;
    protected override float BaseCooldown => inner.Cooldown;
    protected override float BaseSpeed    => inner.Speed;

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        mods = new StatBlock();
        InjectMods(mods);
    }

    protected override IEnumerator Cast(Vector3 o, Vector3 t)
    {
        yield return ApplyModifierEffect(o, t);
    }

    protected virtual IEnumerator ApplyModifierEffect(Vector3 o, Vector3 t)
    {
        yield return inner.TryCast(o, t);
    }

    protected abstract void InjectMods(StatBlock mods);
=======
    private Spell innerSpell;  // the spell being modified
=======
    protected Spell inner;  // so derived spells can access the inner spell
    private ValueModifier dmgMod, manaMod, spdMod;
    private bool isSplit, isDouble;
    private float delay;
>>>>>>> 22ff77c (getting there)

    public ModifierSpell(string n, string d, int ico,
                         Spell sp,
                         ValueModifier dm, ValueModifier mm, ValueModifier sm,
                         bool split = false, bool dbl = false, float del = 0f)
    {
        Name = n; Description = d; IconIndex = ico;
        inner = sp; dmgMod = dm; manaMod = mm; spdMod = sm;
        isSplit = split; isDouble = dbl; delay = del;
    }

    internal override void CastInternal(Vector3 p, Vector3 dir, SpellModifierContext ctx)
    {
        if (dmgMod != null) ctx.damageMods.Add(dmgMod);
        if (manaMod != null) ctx.manaMods.Add(manaMod);
        if (spdMod != null) ctx.speedMods.Add(spdMod);

        if (isSplit)
        {
            float v = 5f; float a = UnityEngine.Random.Range(-v, v);
            inner.CastInternal(p, Quaternion.Euler(0, 0, a) * dir, ctx);
            inner.CastInternal(p, Quaternion.Euler(0, 0, -a) * dir, ctx);
        }
        else if (isDouble)
        {
            inner.CastInternal(p, dir, ctx);
            if (delay <= 0f) delay = 0.2f;
            ProjectileManager.Instance.StartCoroutine(Delayed(delay, () => inner.CastInternal(p, dir, ctx)));
        }
        else
        {
            inner.CastInternal(p, dir, ctx);
        }
    }

    private System.Collections.IEnumerator Delayed(float t, Action a)
    {
        yield return new WaitForSeconds(t);
        a();
    }

    public override int GetManaCost()
    {
        int cost = inner.GetManaCost();
        if (manaMod != null)
        {
            cost = manaMod.type == ModType.Multiplicative ? Mathf.RoundToInt(cost * manaMod.value) : cost + Mathf.RoundToInt(manaMod.value);
        }
        return cost;
    }

    public override float GetDamage()
    {
        float dmg = inner.GetDamage();
        if (dmgMod != null)
        {
            dmg = dmgMod.type == ModType.Multiplicative ? dmg * dmgMod.value : dmg + dmgMod.value;
        }
        if (isSplit) dmg *= 2f;
        if (isDouble) dmg *= 2f;
        return dmg;
    }

    public override float GetCooldown() => inner.GetCooldown();
    public override string GetProjectileType() => inner.GetProjectileType();
    public override int GetProjectileSprite() => inner.GetProjectileSprite();
    public override float GetProjectileSpeed()
    {
        float sp = inner.GetProjectileSpeed();
        if (spdMod != null) sp = spdMod.type == ModType.Multiplicative ? sp * spdMod.value : sp + spdMod.value;
        return sp;
    }
<<<<<<< HEAD

    public override float GetProjectileLifetime()
    {
        if (projectileOverrideInfo != null && !string.IsNullOrEmpty(projectileOverrideInfo.lifetime))
            return float.Parse(projectileOverrideInfo.lifetime);
        return innerSpell.GetProjectileLifetime();
    }
>>>>>>> 1e7a3e8 (some updates)
}
=======
    public override float GetProjectileLifetime() => inner.GetProjectileLifetime();
}
>>>>>>> 22ff77c (getting there)
