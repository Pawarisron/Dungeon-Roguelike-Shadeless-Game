using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DictionaryExtensions
{
    //  Summary: 
    //       Like .Union()
    //       Merges two dictionaries into one, resolving duplicate keys by taking the value from the first dictionary.
    public static Dictionary<TKey, TValue> Union<TKey, TValue>(
        this Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second)
    {
        // Concatenate the two dictionaries and group by keys to handle duplicates
        return first
            .Concat(second)
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.First().Value); // Use value from first dictionary if keys are duplicated
    }
    //  Summary:
    //      Like .UnionWith()
    //      Merges two dictionaries into the first, using values from the first for duplicate keys.
    public static void UnionWith<TKey, TValue>(
       this Dictionary<TKey, TValue> first,
       Dictionary<TKey, TValue> second)
    {
        foreach (var pair in second)
        {
            // Add or update the first dictionary with values from the second
            first[pair.Key] = pair.Value;
        }
    }
}
