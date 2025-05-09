using System.Collections.Generic;

public class SpellModifierContext
{
    public List<ValueModifier> damageModifiers = new List<ValueModifier>();
    public List<ValueModifier> speedModifiers  = new List<ValueModifier>();
    public List<ValueModifier> manaModifiers   = new List<ValueModifier>();
    public SpellData.ProjectileInfo projectileOverride = null;

    public SpellModifierContext Clone()
    {
        SpellModifierContext copy = new SpellModifierContext();
        // Copy value modifier lists
        foreach (var mod in damageModifiers) copy.damageModifiers.Add(new ValueModifier(mod.Type, mod.Value));
        foreach (var mod in speedModifiers)  copy.speedModifiers.Add(new ValueModifier(mod.Type, mod.Value));
        foreach (var mod in manaModifiers)   copy.manaModifiers.Add(new ValueModifier(mod.Type, mod.Value));
        // Copy projectile override if set
        if (projectileOverride != null)
        {
            copy.projectileOverride = projectileOverride.Clone();
        }
        return copy;
    }
}
