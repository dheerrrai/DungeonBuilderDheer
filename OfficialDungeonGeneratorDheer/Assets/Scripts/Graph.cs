using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Graph<T>
{
    private Dictionary<T, List<T>> GraphDict;


    public Graph()
    {
        GraphDict = new Dictionary<T, List<T>>();
    }

    public void AddNode(T Node)
    {
        if (!GraphDict.ContainsKey(Node))
        {
            GraphDict[Node] = new List<T>();

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
    public void FindLinks(T Node)
    {
        foreach (T Values in GraphDict[Node])
        {

        }
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
    public void BFS(T Node)
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
                if (!Discovered.Contains(Node))
                {
                    Queue.Enqueue(Value);
                    Discovered.Add(Value);
                }
            }
        }

    }
    public void DFS(T Node)
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
    }

}
