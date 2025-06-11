using System;
using System.Collections.Generic;
using System.Text;
using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(3)]
public class TileMapGenerator : MonoBehaviour
{
    
    [SerializeField]
    private UnityEvent onGenerateTileMap;


    [SerializeField]
    DungeonGenerator DungeonScript;    
    
    public int [,] _tileMap;
    
    private void Start()
    {
        DungeonScript = GetComponent<DungeonGenerator>();
    }
    
    
    public void GenerateTileMap(Vector2Int Bounds, List<RectInt> Rooms)
    {
        int [,] tileMap = new int[Bounds.x,Bounds.y];
        int rows = tileMap.GetLength(0);
        int cols = tileMap.GetLength(1);

        //Fill the map with empty spaces
        for (int i = 0; i < rows; i++)
        {
            for(int j = 0; j < cols; j++)
            {
                tileMap[i, j] = 0;
            }

        }
        foreach (RectInt room in Rooms)
        {
            foreach (Vector2Int pos in room.allPositionsWithin)
            {
                // Safety check to stay within bounds
                if (pos.x < 0 || pos.x >= rows || pos.y < 0 || pos.y >= cols)
                    continue;

                // It's a wall if it's on the edge, not a door, and not already placed
                bool isEdge = (pos.x == room.xMin || pos.x == room.xMax - 1 ||
                               pos.y == room.yMin || pos.y == room.yMax - 1);

                if (isEdge &&
                    !DungeonScript.DoorCoords.Contains(pos) &&
                    !DungeonScript.PlacedWalls.Contains(pos))
                {
                    tileMap[pos.x, pos.y] = 1; // Wall
                }
            }
        }
        _tileMap = tileMap;
        
        onGenerateTileMap.Invoke();
    }

    public string ToString(bool flip)
    {
        if (_tileMap == null) return "Tile map not generated yet.";
        
        int rows = _tileMap.GetLength(0);
        int cols = _tileMap.GetLength(1);
        
        var sb = new StringBuilder();
    
        int start = flip ? rows - 1 : 0;
        int end = flip ? -1 : rows;
        int step = flip ? -1 : 1;

        for (int i = start; i != end; i += step)
        {
            for (int j = 0; j < cols; j++)
            {
                sb.Append((_tileMap[i, j]==0?'0':'#')); //Replaces 1 with '#' making it easier to visualize
            }
            sb.AppendLine();
        }
    
        return sb.ToString();
    }
    
    public int[,] GetTileMap()
    {
        return _tileMap.Clone() as int[,];
    }
    
    [Button]
    public void PrintTileMap()
    {
        Debug.Log(ToString(true));
    }
    
    
}
