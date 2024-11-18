using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


//  Summary:
//      Represents an undirected graph where nodes are connected by edges.
//      Duplicate edges are allowed between nodes.
public class Graph<T> //Undirected Graph, Edge can be dupicate.
{
    // Gets the list of nodes in the graph.
    public List<T> Nodes { get; private set; }
    // Gets the adjacency list that maps each node to its list of connected neighbors.
    public Dictionary<T, List<T> > Adjacencylist { get; private set;}
    public Graph()
    {
        //Initializes a new instance of the Graph class.
        Nodes = new List<T>();
        Adjacencylist = new Dictionary<T, List<T>>();
    }
    // Adds a new node to the graph if it does not already exist.
    public virtual void AddNode(T node)
    {
        if ( !Nodes.Contains(node) )
        {
            Nodes.Add(node);
            Adjacencylist[node] = new List<T>();
        }
    }
    // Adds an undirected edge between two nodes.
    public virtual bool AddEdge(T from, T to)
    {
        if( Nodes.Contains(from) && Nodes.Contains(to) )
        {
            Adjacencylist[from].Add(to);
            Adjacencylist[to].Add(from);
            return true;
        }
        return false;
    }
    
    // Remove an edge between two nodes
    public virtual void RemoveEdge(T from, T to)
    {
        if (Adjacencylist.ContainsKey(from))
            Adjacencylist[from].Remove(to);

        if (Adjacencylist.ContainsKey(to))
            Adjacencylist[to].Remove(from);
    }
    // Remove a node and all associated edges
    public virtual void RemoveNode(T node)
    {
        if (Adjacencylist.ContainsKey(node))
        {
            foreach (var neighbor in Adjacencylist[node])
            {
                Adjacencylist[neighbor].Remove(node);
            }
            Adjacencylist.Remove(node);
            Nodes.Remove(node);
        }
    }
    // Get list of neighbors nodes
    public List<T> GetNeighbors(T node)
    {
        return Adjacencylist.ContainsKey(node) ? Adjacencylist[node] : new List<T>();
    }

    // Check if there is a path between two nodes using DFS
    public bool HasPathDFS(T start, T end, HashSet<T> visited = null)
    {
        if (start.Equals(end)) return true;
        visited ??= new HashSet<T>();
        visited.Add(start);

        foreach (var neighbor in GetNeighbors(start))
        {
            if (!visited.Contains(neighbor) && HasPathDFS(neighbor, end, visited))
                return true;
        }
        return false;
    }

    // Perform BFS and return list of visited nodes
    public List<T> BreadthFirstSearch(T start)
    {
        var visited = new HashSet<T>();
        var queue = new Queue<T>();
        var result = new List<T>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            result.Add(node);

            foreach (var neighbor in GetNeighbors(node))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        return result;
    }
    // Perform DFS and return list of visited nodes
    public List<T> DepthFirstSearch(T start)
    {
        var visited = new HashSet<T>();
        var stack = new Stack<T>();
        var result = new List<T>();

        stack.Push(start);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!visited.Contains(node))
            {
                visited.Add(node);
                result.Add(node);

                foreach (var neighbor in GetNeighbors(node))
                {
                    if (!visited.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }
            }
        }
        return result;
    }
}
//  Summary:
//      Represents a weighted, undirected graph where nodes are connected by edges with weights.
public class WeightedGraph<T> : Graph<T>
{
    // Gets the dictionary of edge weights, where each key is a tuple of nodes representing an edge.
    public Dictionary<(T, T), float> Weights { get; private set; }

    public WeightedGraph(): base()
    {
        Weights = new Dictionary<(T, T), float>();
    }
    // Adds a weighted, undirected edge between two nodes.
    public virtual void AddEdge(T from, T to, float weight)
    {
        base.AddEdge(from, to);
        Weights[(from,to)] = weight;
        Weights[(to,from)] = weight;
    }
    // Get the weight of the edge between two nodes

    // Remove an edge between two nodes
    public override void RemoveEdge(T from, T to)
    {
        base.RemoveEdge(from, to);
        Weights.Remove((from, to));
        Weights.Remove((to, from));
    }

    // Remove a node and all associated edges
    public override void RemoveNode(T node)
    {
        if (Adjacencylist.ContainsKey(node))
        {
            foreach (var neighbor in Adjacencylist[node])
            {
                Weights.Remove((node, neighbor));
                Weights.Remove((neighbor, node));
            }
        }
        base.RemoveNode(node);
    }

    public virtual float GetWeight(T from, T to)
    {
        return Weights.ContainsKey((from, to)) ? Weights[(from, to)] : float.PositiveInfinity;
    }
    public Dictionary<T, float> Dijkstra(T start)
    {
        var distances = new Dictionary<T, float>();
        var priorityQueue = new SortedDictionary<float, List<T>>();

        // Initialize distances
        foreach (var node in Nodes)
        {
            distances[node] = float.PositiveInfinity; // Start with infinite distance
        }
        distances[start] = 0; // Distance to start node is zero

        // Add the start node to the priority queue
        if (!priorityQueue.ContainsKey(0))
        {
            priorityQueue[0] = new List<T>();
        }
        priorityQueue[0].Add(start);

        while (priorityQueue.Count > 0)
        {
            // Get the node with the smallest distance
            var firstEntry = priorityQueue.First();
            float currentDistance = firstEntry.Key;
            var currentNodes = firstEntry.Value;

            // Choose the first node in the list (could be any node with that distance)
            T currentNode = currentNodes[0];
            currentNodes.RemoveAt(0);

            // If there are no nodes left at this distance, remove the entry
            if (currentNodes.Count == 0)
            {
                priorityQueue.Remove(currentDistance);
            }

            // Skip if the current distance is greater than the recorded distance
            if (currentDistance > distances[currentNode])
                continue;

            // Explore neighbors
            foreach (var neighbor in GetNeighbors(currentNode))
            {
                // Use GetWeight to calculate the distance to the neighbor
                float weight = GetWeight(currentNode, neighbor);
                float newDistance = currentDistance + weight;

                // If found a shorter path to the neighbor
                if (newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance; // Update the shortest distance

                    // Add to the priority queue
                    if (!priorityQueue.ContainsKey(newDistance))
                    {
                        priorityQueue[newDistance] = new List<T>();
                    }
                    priorityQueue[newDistance].Add(neighbor);
                }
            }
        }
        return distances; // Return the dictionary of distances
    }


    // Get the weight of the shortest path between two nodes
    public float GetShortestPathWeight(T start, T end)
    {
        var distances = Dijkstra(start);
        return distances.ContainsKey(end) ? distances[end] : float.PositiveInfinity;
    }

    public (T farthestNode, float farthestDistance) FindFarthestNode(T start)
    {
        var distances = Dijkstra(start);
        
        T farthestNode = default(T);
        float farthestDistance = float.MinValue;

        foreach (var kvp in distances)
        {
            if (kvp.Value > farthestDistance)
            {
                farthestDistance = kvp.Value;
                farthestNode = kvp.Key;
            }
        }

        return (farthestNode, farthestDistance);
    }
}
