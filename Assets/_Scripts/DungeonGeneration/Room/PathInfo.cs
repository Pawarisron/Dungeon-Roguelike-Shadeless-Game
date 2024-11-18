using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Stores PathBase info
public class PathInfo
{
    public HashSet<Vector2Int> PathBase { get; set; }  = new HashSet<Vector2Int>(); //Minimum PathBase Width that the agent can walk through
    public HashSet<Vector2Int> PathExpanded { get; set; }  = new HashSet<Vector2Int>(); //PathBase Expanded to Corridor Size (Include PathBase)
}
