using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PropData", menuName = "ObjectData/PropData")]
public class PropData : ScriptableObject, IRandomWithProbability
{
    [Header("Prop data:")]
    public GameObject PropPrefab;

    public Vector2Int PropSize = Vector2Int.one;

    [Space, Header("Placement type:")]
    public bool Corner = true;
    public bool NearWallUP = true;
    public bool NearWallDown = true;
    public bool NearWallRight = true;
    public bool NearWallLeft = true;
    public bool Inner = true;

    [Min(1)]
    public int MinPlacementQuantity = 1;
    [Min(1)]
    public int MaxPlacementQuantity = 1;
    [Range(0, 1)]
    public float PlacementProbability = 0.5f; //TODO: make its used

    [Space, Header("Group placement:")]
    public bool PlaceAsGroup = false;
    [Min(1)]
    public int GroupMinCount = 1;
    [Min(1)]
    public int GroupMaxCount = 1;

    int IRandomWithProbability.Max { get => MaxPlacementQuantity; }
    int IRandomWithProbability.Min { get => MinPlacementQuantity; }
    float IRandomWithProbability.Probability { get => PlacementProbability; }
}
