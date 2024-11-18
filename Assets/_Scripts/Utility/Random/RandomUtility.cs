using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomUtility
{
    //  Summary:
    //      Generates a random value between Min and Max based on the specified Probability.
    public static int GetRandomValueWithProbability(IRandomWithProbability value)
    {
        if(value.Min > value.Max)
        {
            Debug.LogWarning("Min value is greater than Max value.");
            return 0;
        }
        int randomValue = value.Min;
        float change;

        for (int i = 0; i < value.Max - value.Min; i++)
        {
            change = Random.value;
            if (change <= value.Probability)
                randomValue++;
        }
        // Ensure randomValue does not exceed Max
        return Mathf.Min(randomValue, value.Max);
    }
}
