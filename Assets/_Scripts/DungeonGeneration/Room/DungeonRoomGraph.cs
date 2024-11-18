using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonRoomGraph : WeightedGraph<DungeonRoomData>
{
    //TODO: Upgrade dungeonRoomGraph into dungonGraph change each DungeonRoomData into Node
    public Dictionary<(DungeonRoomData, DungeonRoomData), PathInfo> Paths { get; private set; }

    public DungeonRoomGraph() : base()
    {
        Paths = new Dictionary<(DungeonRoomData, DungeonRoomData), PathInfo> ();
    }

    public void AddEdge(DungeonRoomData from, DungeonRoomData to, float distance, PathInfo path)
    {
        base.AddEdge(from, to, distance);
        Paths[(from, to)] = path;
        Paths[(to, from)] = path;
    }
    public override void RemoveEdge(DungeonRoomData from, DungeonRoomData to)
    {
        base.RemoveEdge(from, to);
        Paths.Remove((from, to));
        Paths.Remove((to, from));
    }
    public override void RemoveNode(DungeonRoomData node)
    {
        if (Adjacencylist.ContainsKey(node))
        {
            foreach (var neighbor in Adjacencylist[node])
            {
                Paths.Remove((node, neighbor));  // Remove paths associated with this node
                Paths.Remove((neighbor, node));
            }
        }
        base.RemoveNode(node); // Call base method to remove from adjacency list, nodes, and weights

    }
    public string PrintGraph()
    {
        var result = new System.Text.StringBuilder();

        foreach (var node in Nodes)
        {
            result.Append($"{node.RoomNumber}: ");
            foreach (var adNode in Adjacencylist[node])
            {
                result.Append($"{adNode.RoomNumber}, ");
            }
            result.AppendLine();
        }
        return result.ToString();
    }
    public DungeonRoomData FindDuplicatePosition(Vector2Int RoomCenterPosition)
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            var node = Nodes[i];
            if(node.CenterPos == RoomCenterPosition)
                return node; 
        }
        return null;
    }
}
