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

public abstract class ModifierSpell : Spell
{
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

    // Modifiers for properties 
    private ValueModifier damageMod;
    private ValueModifier manaMod;
    private ValueModifier speedMod;
    private SpellData.ProjectileData projectileOverrideInfo;  

    // Special behavior flags
    private bool isSplitter;
    private bool isDoubler;
    private float delay;  // delay for doubler second cast

    public ModifierSpell(string name, string description, int iconIndex,
                         Spell innerSpell,
                         ValueModifier damageMod, ValueModifier manaMod, ValueModifier speedMod,
                         SpellData.ProjectileData projectileOverrideInfo,
                         bool isSplitter = false, bool isDoubler = false, float delay = 0f)
    {
        this.Name = name;
        this.Description = description;
        this.IconIndex = iconIndex;
        this.innerSpell = innerSpell;
        this.damageMod = damageMod;
        this.manaMod = manaMod;
        this.speedMod = speedMod;
        this.projectileOverrideInfo = projectileOverrideInfo;
        this.isSplitter = isSplitter;
        this.isDoubler = isDoubler;
        this.delay = delay;
    }

    internal override void CastInternal(Vector3 spawnPosition, Vector3 direction, SpellModifierContext context)
    {
        if (damageMod != null)
            context.damageModifiers.Add(damageMod);
        if (manaMod != null)
            context.manaModifiers.Add(manaMod);
        if (speedMod != null)
            context.speedModifiers.Add(speedMod);
        if (projectileOverrideInfo != null && context.projectileOverride == null)
        {
            context.projectileOverride = projectileOverrideInfo;
        }

        // Handle special casting behaviors
        if (isSplitter)
        {
            float angleVariance = 5f;
            float randAngle = Random.Range(-angleVariance, angleVariance);
            Quaternion rot1 = Quaternion.Euler(0, 0, randAngle);
            Quaternion rot2 = Quaternion.Euler(0, 0, -randAngle);
            SpellModifierContext ctx1 = context.Clone();
            SpellModifierContext ctx2 = context.Clone();
            innerSpell.CastInternal(spawnPosition, rot1 * direction, ctx1);
            innerSpell.CastInternal(spawnPosition, rot2 * direction, ctx2);
        }
        else if (isDoubler)
        {
            SpellModifierContext ctxInitial = context.Clone();
            innerSpell.CastInternal(spawnPosition, direction, ctxInitial);
            if (delay <= 0f) delay = 0.2f;
            CoroutineManager cm = Object.FindAnyObjectByType<CoroutineManager>();
            if (cm != null)
            {
                // Start a coroutine to execute the second cast after the delay
                cm.StartCoroutine(ExecuteAfterDelay(delay, () =>
                {
                    SpellModifierContext ctxDelayed = context.Clone();
                    innerSpell.CastInternal(spawnPosition, direction, ctxDelayed);
                }));
            }
            else
            {
                Debug.LogWarning("CoroutineManager not found, second cast will not be delayed.");
            }
        }
        else
        {
            innerSpell.CastInternal(spawnPosition, direction, context);
        }
    }

    // Coroutine helper for delayed execution of an action
    private System.Collections.IEnumerator ExecuteAfterDelay(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action.Invoke();
    }

    public override int GetManaCost()
    {
        int cost = innerSpell.GetManaCost();
        if (manaMod != null)
        {
            // Apply multiplicative or additive mana cost change
            if (manaMod.Type == ValueModifier.ModType.Multiplicative)
                cost = Mathf.RoundToInt(cost * manaMod.Value);
            else if (manaMod.Type == ValueModifier.ModType.Additive)
                cost += Mathf.RoundToInt(manaMod.Value);
        }
        return cost;
    }

    public override float GetDamage()
    {
        float dmg = innerSpell.GetDamage();
        if (damageMod != null)
        {
            if (damageMod.Type == ValueModifier.ModType.Multiplicative)
                dmg *= damageMod.Value;
            else if (damageMod.Type == ValueModifier.ModType.Additive)
                dmg += damageMod.Value;
        }
        // If this modifier causes multiple casts, include their contribution in estimated damage
        if (isSplitter) dmg *= 2;
        if (isDoubler)  dmg *= 2;
        return dmg;
    }

    public override float GetCooldown()
    {
        return innerSpell.GetCooldown();
    }

    public override string GetProjectileType()
    {
        if (projectileOverrideInfo != null && !string.IsNullOrEmpty(projectileOverrideInfo.trajectory))
            return projectileOverrideInfo.trajectory;
        return innerSpell.GetProjectileType();
    }

    public override int GetProjectileSprite()
    {
        if (projectileOverrideInfo != null && projectileOverrideInfo.sprite != 0)
            return projectileOverrideInfo.sprite;
        return innerSpell.GetProjectileSprite();
    }

    public override float GetProjectileSpeed()
    {
        if (projectileOverrideInfo != null && !string.IsNullOrEmpty(projectileOverrideInfo.speed))
            return float.Parse(projectileOverrideInfo.speed);
        float baseSpeed = innerSpell.GetProjectileSpeed();
        if (speedMod != null && speedMod.Type == ValueModifier.ModType.Multiplicative)
            baseSpeed *= speedMod.Value;
        return baseSpeed;
    }

    public override float GetProjectileLifetime()
    {
        if (projectileOverrideInfo != null && !string.IsNullOrEmpty(projectileOverrideInfo.lifetime))
            return float.Parse(projectileOverrideInfo.lifetime);
        return innerSpell.GetProjectileLifetime();
    }
>>>>>>> 1e7a3e8 (some updates)
}
