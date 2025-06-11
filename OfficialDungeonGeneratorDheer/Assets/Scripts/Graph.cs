using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class Graph<T>
{
    private Dictionary<T, List<T>> GraphDict;


    public Graph()
    {
        GraphDict = new Dictionary<T, List<T>>();
    }
    public void Clear()
    {
        GraphDict.Clear();
    }

    public void AddNode(T Node)
    {
        if (!GraphDict.ContainsKey(Node))
        {
            GraphDict[Node] = new List<T>();

        }

    }
    public void RemoveNode(T Node)
    {
        if (!GraphDict.ContainsKey(Node)) 
        {
            GraphDict.Remove(Node);
        }
    }
    public void AddLink(T FromNode, T ToNode)
    {
        if (!GraphDict.ContainsKey(FromNode) || !GraphDict.ContainsKey(ToNode))
        {
            Debug.Log("One of more Nodes Unavailable/Do not Exist");
            return;
        }
        GraphDict[FromNode].Add(ToNode);
        GraphDict[ToNode].Add(FromNode);
    }
    public List<T> FindLinks(T Node)
    {
        if (!GraphDict.ContainsKey(Node))
        {
            return new List<T>();
        }

        return new List<T>(GraphDict[Node]);
    }
    public List<T> GetAllNodes()
    {
        return GraphDict.Keys.ToList();
    }
    public bool ContainsNode(T node)
    {
        return GraphDict.ContainsKey(node);
    }
    public void PrintGraph()
    {
        string Line = "";
        foreach (T Key in GraphDict.Keys)
        {
            Line += $"{Key} :\t{ListPrint(GraphDict[Key])}\n";
        }
        Debug.Log(Line);
    }
    public string ListPrint(List<T> PList)
    {
        string ListPrinted = "| ";

        foreach (T Value in PList)
        {
            ListPrinted += $"{Value} |";


        }
        return ListPrinted;
    }
    public List<T> BFS(T Node)
    {
        List<T> Discovered = new List<T>();
        Queue<T> Queue = new Queue<T>();
        Queue.Enqueue(Node);
        Discovered.Add(Node);
        while (Queue.Count > 0)
        {

            Node = Queue.Dequeue();
            Debug.Log(Node);
            foreach (T Value in GraphDict[Node])
            {
                if (!Discovered.Contains(Value))
                {
                    Queue.Enqueue(Value);
                    Discovered.Add(Value);
                }
            }
        }
        return Discovered;

    }
    public List<T> DFS(T Node)
    {
        List<T> Discovered = new List<T>();
        Stack<T> Stack = new Stack<T>();
        Stack.Push(Node);
        while (Stack.Count > 0)
        {
            Node = Stack.Pop();
            if (!Discovered.Contains(Node))
            {
                Discovered.Add(Node);
                Debug.Log(Node);
                foreach (T Value in GraphDict[Node])
                {
                    Stack.Push(Value);
                }
            }
        }
        return Discovered;
    }

}
