using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public abstract class ModifierSpell : Spell
{
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
}
