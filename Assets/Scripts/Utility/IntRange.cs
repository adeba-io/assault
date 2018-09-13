using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntRange
{
    public int rangeStart, rangeEnd;
    
    int GetRandomValue()
    {
        return Random.Range(rangeStart, rangeEnd);
    }

    public static bool operator ==(IntRange range1, IntRange range2)
    {
        return range1.rangeStart == range2.rangeStart && range1.rangeEnd == range2.rangeEnd;
    }

    public static bool operator !=(IntRange range1, IntRange range2)
    {
        return range1.rangeStart != range2.rangeStart || range1.rangeEnd != range2.rangeEnd;
    }

    public static implicit operator int(IntRange i)
    {
        return i.GetRandomValue();
    }
}

public class IntRangeAttribute : PropertyAttribute
{
    public int minLimit, maxLimit;

    public IntRangeAttribute(int limitMin, int limitMax)
    {
        minLimit = limitMin;
        maxLimit = limitMax;
    }
}
