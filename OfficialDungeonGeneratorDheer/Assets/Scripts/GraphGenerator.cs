using System;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GraphGenerator : MonoBehaviour
{
    [SerializeField]
    public DungeonGenerator Dungeon;
    public TileMapGenerator TileMap;
    private RectInt dungeonBounds;

    private int[,] Tiles; 

    private Graph<Vector3> graph = new Graph<Vector3>();
    
    //public GameObject floor;
    
    private void Start()
    {
        dungeonBounds = Dungeon.StartingRoom;
        //GenerateGraph();
    }

    [Button]
    public void GenerateGraph()
    {
        graph.Clear();
        Tiles = TileMap.GetTileMap();
        // Connect neighbors
        for (int x = dungeonBounds.xMin ; x < dungeonBounds.xMax; x++)
        {
            for (int y = dungeonBounds.yMin; y < dungeonBounds.yMax; y++)
            {
                if (Tiles[x, y] == 0)
                {
                    //Debug.Log("FoundOneNodePLEASE");
                    Vector3 currentPos = new Vector3(x + 0.5f, 0, y + 0.5f);

                    // Cardinal directions (up, down, left, right)
                    TryConnectNeighbor(x + 1, y, currentPos);    // Right
                    TryConnectNeighbor(x - 1, y, currentPos);    // Left
                    TryConnectNeighbor(x, y + 1, currentPos);    // Up
                    TryConnectNeighbor(x, y - 1, currentPos);    // Down

                    // Diagonal directions
                    TryConnectNeighbor(x + 1, y + 1, currentPos); // Top-right
                    TryConnectNeighbor(x - 1, y + 1, currentPos); // Top-left
                    TryConnectNeighbor(x + 1, y - 1, currentPos); // Bottom-right
                    TryConnectNeighbor(x - 1, y - 1, currentPos); // Bottom-left
                }

            }
        }

        //floor.transform.position = new Vector3( dungeonBounds.center.x - .5f, -.5f, dungeonBounds.center.y - .5f);
        //floor.transform.localScale = new Vector3(dungeonBounds.width, 1, dungeonBounds.height);
    }
    
    
    private void TryConnectNeighbor(int nx, int ny, Vector3 currentPos)
    {
        if (nx >= dungeonBounds.xMin && nx < dungeonBounds.xMax &&
        ny >= dungeonBounds.yMin && ny < dungeonBounds.yMax &&
        Tiles[nx, ny] == 0)
        {
            Vector3 neighborPos = new Vector3(nx + 0.5f, 0, ny + 0.5f);
            graph.AddNode(currentPos);
            graph.AddNode(neighborPos);
            graph.AddLink(currentPos, neighborPos);
        }
    }
    
    private void Update()
    {
        foreach (var node in graph.GetAllNodes())
        {
            DebugExtension.DebugWireSphere(node, Color.cyan, .2f);
            foreach (var neighbor in graph.FindLinks(node))
            {
                Debug.DrawLine(node, neighbor, Color.cyan);
            }
        }
    }

    public Graph<Vector3> GetGraph()
    {
        return graph;
    }
    
}
