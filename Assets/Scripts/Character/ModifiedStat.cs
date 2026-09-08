using System;
using System.Collections.Generic;
using System.Linq;

public class ModifiedStat
{
    private readonly float baseValue;
    private readonly List<StatModifier> modifiers = new();
    private bool isDirty = true;
    private float cachedValue;

    public float BaseValue => baseValue;
    public float Value
    {
        get
        {
            if (isDirty)
            {
                cachedValue = CalculateFinalValue();
                isDirty = false;
            }

            return cachedValue;
        }
    }

    public ModifiedStat(float baseValue)
    {
        this.baseValue = baseValue;
    }

    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
        SortModifiers();
        isDirty = true;
    }

    public void RemoveModifier(StatModifier modifier)
    {
        if (modifiers.Remove(modifier))
            isDirty = true;
    }

    public void RemoveModifiersFromSource(object source)
    {
        if (modifiers.RemoveAll(m => m.Source == source) > 0)
            isDirty = true;
    }

    public void RemoveAllModifiers()
    {
        if (modifiers.Count > 0)
        {
            modifiers.Clear();
            isDirty = true;
        }
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
