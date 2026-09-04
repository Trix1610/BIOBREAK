using System;
using System.Collections.Generic;
using System.Linq;

public class ModifiedStat
{
    private readonly float baseValue;
    private readonly List<StatModifier> modifiers = new();

    public float BaseValue => baseValue;
    public float Value => CalculateFinalValue();

    public ModifiedStat(float baseValue)
    {
        this.baseValue = baseValue;
    }

    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
        SortModifiers();
    }

    public void RemoveModifier(StatModifier modifier)
    {
        modifiers.Remove(modifier);
    }

    public void RemoveModifiersFromSource(object source)
    {
        modifiers.RemoveAll(m => m.Source == source);
    }

    public void RemoveAllModifiers()
    {
        modifiers.Clear();
    }

    private void SortModifiers()
    {
        modifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    private float CalculateFinalValue()
    {
        float finalValue = baseValue;

        // Apply flat modifiers
        float flatSum = modifiers
            .Where(m => m.Type == StatModifierType.Flat)
            .Sum(m => m.Value);

        finalValue += flatSum;

        // Apply percent add modifiers (stacking)
        float percentAddSum = modifiers
            .Where(m => m.Type == StatModifierType.PercentAdd)
            .Sum(m => m.Value);

        finalValue *= (1 + percentAddSum);

        // Apply percent mult modifiers (multiplicative)
        float percentMultProduct = modifiers
            .Where(m => m.Type == StatModifierType.PercentMult)
            .Aggregate(1f, (product, m) => product * (1 + m.Value));

        finalValue *= percentMultProduct;

        return finalValue;
    }
}
