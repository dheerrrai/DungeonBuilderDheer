using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public enum Algorithms
{
    BFS,
    Dijkstra,
    AStar
}

public class PathFinder : MonoBehaviour
{
    
    public GraphGenerator graphGenerator;
    
    private Vector3 startNode;
    private Vector3 endNode;
    
    public List<Vector3> path = new List<Vector3>();
    HashSet<Vector3> discovered = new HashSet<Vector3>();
    
    private Graph<Vector3> graph;
    
    public Algorithms algorithm = Algorithms.BFS;
    
    void Start()
    {
        graphGenerator = GetComponent<GraphGenerator>();
        graph = graphGenerator.GetGraph();
    }

    private Vector3 GetClosestNodeToPosition(Vector3 position)
    {
        Vector3 closestNode = Vector3.zero;
        float closestDistance = Mathf.Infinity;
        

        closestNode = new Vector3(Mathf.Round(position.x), Mathf.Round( position.y), Mathf.Round( position.z));
        //Find the closest node to the position

        return closestNode;
    }
    
    
    public List<Vector3> CalculatePath(Vector3 from, Vector3 to)
    {
        Vector3 playerPosition = from;
        
        startNode = GetClosestNodeToPosition(playerPosition);
        endNode = GetClosestNodeToPosition(to);

        List<Vector3> shortestPath = new List<Vector3>();
        
        switch (algorithm)
        {
            case Algorithms.BFS:
                shortestPath = BFS(startNode, endNode);
                break;
            case Algorithms.Dijkstra:
                shortestPath =  Dijkstra(startNode, endNode);
                break;
            case Algorithms.AStar:
                shortestPath =  AStar(startNode, endNode);
                break;
        }
        
        path = shortestPath; //Used for drawing the path
        
        return shortestPath;
    }
    
    List<Vector3> BFS(Vector3 start, Vector3 end) 
    {
        
        //Use this "discovered" list to see the nodes in the visual debugging used on OnDrawGizmos()
        Vector3 Point = start;
        discovered.Clear();
        Dictionary<Vector3, Vector3> Path = new Dictionary<Vector3, Vector3>();
        Queue<Vector3> PathQueue = new Queue<Vector3>();
        PathQueue.Enqueue(Point);
        discovered.Add(Point);
        while (PathQueue.Count > 0)
        {
            Point = PathQueue.Dequeue();
            if (Point.Equals(end))
            {
                return ReconstructPath(Path, start, end);
            }
            foreach(Vector3 Edges in graph.FindLinks(Point))
            {
                if (!discovered.Contains(Edges))
                {
                    PathQueue.Enqueue(Edges);
                    discovered.Add(Edges);
                    Path[Edges] = Point;
                }
            }
        }

        return new List<Vector3>(); // No path found
    }
    
    
    public List<Vector3> Dijkstra(Vector3 start, Vector3 end)
    {
        //Use this "discovered" list to see the nodes in the visual debugging used on OnDrawGizmos()
        Vector3 Point = start;
        float CurrentCost = 0;
        discovered.Clear();
        Dictionary<Vector3, Vector3> Path = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, float> Cost = new Dictionary<Vector3, float>();

        List<(Vector3 Node, float Cost)> GraphCosts = new();

        Queue<Vector3> PathQueue = new Queue<Vector3>();
        GraphCosts.Add((Point,0));
        discovered.Add(Point);
        Cost[Point]=0;
        while (GraphCosts.Count > 0)
        {
            GraphCosts= GraphCosts.OrderByDescending(Node => Node.Cost).ToList();
            Point = GraphCosts[GraphCosts.Count - 1].Node;
            GraphCosts.RemoveAt(GraphCosts.Count - 1);
            
            
            if (Point.Equals(end))
            {
                return ReconstructPath(Path, start, end);
            }
            foreach (Vector3 Edges in graph.FindLinks(Point))
            {
                float newCost = Cost[Point] + Vector3.Distance(Point, Edges);
                if (!Cost.ContainsKey(Edges))
                {
                    Cost[Edges] = newCost;
                    GraphCosts.Add((Edges, newCost));
                    //discovered.Add(Edges);
                    Path[Edges] = Point;
                }
                else if (newCost < Cost[Edges])
                {
                    Cost[Edges] = newCost;
                    GraphCosts.Add((Edges, newCost));
                    //discovered.Add(Edges);
                    Path[Edges] = Point;
                }

            }
        }
        /* */
        return new List<Vector3>(); // No path found
    }
    
    List<Vector3> AStar( Vector3 start, Vector3 end)
    {
        //Use this "discovered" list to see the nodes in the visual debugging used on OnDrawGizmos()
        discovered.Clear();
        Vector3 Point = start;
        float CurrentCost = 0;
        discovered.Clear();
        Dictionary<Vector3, Vector3> Path = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, float> Cost = new Dictionary<Vector3, float>();

        List<(Vector3 Node, float Cost)> GraphCosts = new();

        Queue<Vector3> PathQueue = new Queue<Vector3>();
        GraphCosts.Add((Point, 0));
        discovered.Add(Point);
        Cost[Point] = 0;
        while (GraphCosts.Count > 0)
        {
            GraphCosts = GraphCosts.OrderByDescending(Node => Node.Cost).ToList();
            Point = GraphCosts[GraphCosts.Count - 1].Node;
            GraphCosts.RemoveAt(GraphCosts.Count - 1);


            if (Point.Equals(end))
            {
                return ReconstructPath(Path, start, end);
            }
            foreach (Vector3 Edges in graph.FindLinks(Point))
            {
                float newCost = Cost[Point] + Vector3.Distance(Point, Edges);
                if (!Cost.ContainsKey(Edges))
                {
                    Cost[Edges] = newCost;
                    GraphCosts.Add((Edges, newCost + Heuristic(Edges, end)));
                    discovered.Add(Edges);
                    Path[Edges] = Point;
                }
                else if (newCost < Cost[Edges])
                {
                    Cost[Edges] = newCost;
                    GraphCosts.Add((Edges, newCost+Heuristic(Edges,end)));
                    discovered.Add(Edges);
                    Path[Edges] = Point;
                }

            }
        }
        /* */
        return new List<Vector3>(); // No path found
    }
    
    public float Cost(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to);
    }
    
    public float Heuristic(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to);
    }
    
    List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> parentMap, Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 currentNode = end;

        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = parentMap[currentNode];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startNode, .3f);
    
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endNode, .3f);
    
        if (discovered != null) {
            foreach (var node in discovered)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(node, .3f);
            }
        }
        
        if (path != null) {
            foreach (var node in path)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(node, .3f);
            }
        }
        
        
    }
}
