using System;

public class ValueModifier
{
    public enum ModType { Additive, Multiplicative }
    public ModType Type;
    public float Value;

    public ValueModifier(ModType type, float value)
    {
        this.Type = type;
        this.Value = value;
    }

    public float Apply(float currentValue)
    {
        return (Type == ModType.Multiplicative) ? currentValue * Value 
                                               : currentValue + Value;
    }


    public static float ApplyModifiers(System.Collections.Generic.List<ValueModifier> modifiers, float baseValue)
    {
        float result = baseValue;
        if (modifiers == null) return result;
        float additiveTotal = 0f;
        foreach (ValueModifier mod in modifiers)
        {
            if (mod.Type == ModType.Multiplicative)
            {
                result *= mod.Value;
            }
            else if (mod.Type == ModType.Additive)
            {
                additiveTotal += mod.Value;
            }
        }
        result += additiveTotal;
        return result;
    }
}
