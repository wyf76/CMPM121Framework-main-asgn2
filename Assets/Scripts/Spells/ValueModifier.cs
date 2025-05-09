using System.Collections.Generic;

/// <summary>
/// Specifies how a numeric value should be modified: add a flat amount or apply a multiplier.
/// </summary>
public enum ModType
{
    Additive,
    Multiplicative
}

/// <summary>
/// Encapsulates a single modification to a base value, either additive or multiplicative.
/// </summary>
public class ValueModifier
{
    /// <summary>Determines if this modifier adds or multiplies.</summary>
    public ModType Type { get; }

    /// <summary>Amount to add or factor to multiply by.</summary>
    public float Value { get; }

    public ValueModifier(ModType type, float value)
    {
        Type = type;
        Value = value;
    }

    /// <summary>
    /// Applies all modifiers in two phases: first all multiplicative, then all additive.
    /// </summary>
    public static float Apply(IEnumerable<ValueModifier> modifiers, float baseValue)
    {
        float result = baseValue;
        float additiveSum = 0f;
        foreach (var mod in modifiers)
        {
            if (mod.Type == ModType.Multiplicative)
            {
                result *= mod.Value;
            }
            else
            {
                additiveSum += mod.Value;
            }
        }
        return result + additiveSum;
    }

    /// <summary>
    /// Factory for a multiplicative modifier.
    /// </summary>
    public static ValueModifier Multiply(float factor)
        => new ValueModifier(ModType.Multiplicative, factor);

    /// <summary>
    /// Factory for an additive modifier.
    /// </summary>
    public static ValueModifier Add(float amount)
        => new ValueModifier(ModType.Additive, amount);
}
