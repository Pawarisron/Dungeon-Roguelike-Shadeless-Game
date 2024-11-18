using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SimpleRandomWalkParameters_",menuName ="PCG/SimpleRandomWalkData")]
public class SimpleRandomWalkSO : ScriptableObject
{
    //Offset(X,Y):Distances to move in the X and Y directions.
    public int iterations = 10, walkLength = 10;

    //Grid(X,Y):The size of the grid in the X and Y dimensions.
    public int gridX = 1, gridY = 1, offsetX=1,offsetY=1;

    public int wallHeight = 4, wallEdgeWidth = 1;

    //When start new iteration each iteration will random start point.
    public bool startRandomlyEachIteration = true;

}
