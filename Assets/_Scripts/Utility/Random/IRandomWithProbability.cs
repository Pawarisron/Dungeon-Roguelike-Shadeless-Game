using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//  Summary:
//      Interface for generating random values based on a specified range and probability.
public interface IRandomWithProbability
{
    public int Max { get; }                 //The maximum value in the random range.
    public int Min { get; }                 //The minimum value in the random range.
    public float Probability { get; }       //The probability of incrementing the generated random value.
}
