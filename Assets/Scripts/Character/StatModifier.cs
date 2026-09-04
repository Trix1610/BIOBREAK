using System;

public class StatModifier
{
    public readonly float Value;
    public readonly StatModifierType Type;
    public readonly object Source;
    public readonly int Priority;

    public StatModifier(float value, StatModifierType type, object source = null, int priority = 0)
    {
        Value = value;
        Type = type;
        Source = source;
        Priority = priority;
    }
}

public enum StatModifierType
{
    Flat,       // +10
    PercentAdd, // +50%
    PercentMult // *1.5
}
