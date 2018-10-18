using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntRange
{
    public int rangeStart = 0, rangeEnd = 1;
    
    public int GetRandomValue()
    {
        return Random.Range(rangeStart, rangeEnd);
    }

    public bool WithinRange(int number, bool lowerInclusive = true, bool upperInclusive = true)
    {
        if (lowerInclusive)
        {
            return number >= rangeStart && number < (upperInclusive ? rangeEnd + 1 : rangeEnd);
        }
        else
        {
            return number > rangeStart && number < (upperInclusive ? rangeEnd + 1 : rangeEnd); ;
        }
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

    public override bool Equals(object obj)
    {
        try
        {
            IntRange range = (IntRange)obj;
            return this == range;
        }
        catch
        { return false; }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return string.Format("Minimum : {0}; Maximum : {1}", rangeStart, rangeEnd);
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
