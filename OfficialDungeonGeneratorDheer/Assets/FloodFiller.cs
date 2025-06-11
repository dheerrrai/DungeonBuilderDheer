using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodFiller : MonoBehaviour
{
    private TileMapGenerator TileScript;
    private DungeonGenerator Dungeon;
    public GameObject Floor;
    int[,] TileMap ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        TileScript = GetComponent<TileMapGenerator>();
        Dungeon = GetComponent<DungeonGenerator>();
        
    }

    public IEnumerator Floodfill(int StartX, int StartY, GameObject FloorModel, GameObject FloorParent)
    {

        TileMap = TileScript.GetTileMap();
        int width = Dungeon.StartingRoom.width;
        int height = Dungeon.StartingRoom.height;

        // Guard clause
        if (StartX < 0 || StartX >= width || StartY < 0 || StartY >= height)
            yield break;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(StartX, StartY));

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int x = current.x;
            int y = current.y;

            // Bounds check again
            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;

            // Skip if already filled
            if (TileMap[x, y] != 0)
                continue;

            // Mark as visited immediately to avoid double-processing
            TileMap[x, y] = 1;

            // Spawn floor
            GameObject floor = Instantiate(FloorModel, new Vector3(x + 0.5f, 0f, y + 0.5f), FloorModel.transform.rotation, FloorParent.transform);

            // Spread to neighbors
            queue.Enqueue(new Vector2Int(x + 1, y));
            queue.Enqueue(new Vector2Int(x - 1, y));
            queue.Enqueue(new Vector2Int(x, y + 1));
            queue.Enqueue(new Vector2Int(x, y - 1));

            // Optional delay for visual build effect
            yield return null;
            //yield return new WaitForSeconds(Dungeon.BuildTime/4);
        }

    }public void NoTimeFloodfill(int StartX, int StartY, GameObject FloorModel, GameObject FloorParent)
    {

        TileMap = TileScript.GetTileMap();
        int width = Dungeon.StartingRoom.width;
        int height = Dungeon.StartingRoom.height;

        // Guard clause
        //if (StartX < 0 || StartX >= width || StartY < 0 || StartY >= height)
        //    yield break;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(StartX, StartY));

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int x = current.x;
            int y = current.y;

            // Bounds check again
            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;

            // Skip if already filled
            if (TileMap[x, y] != 0)
                continue;

            // Mark as visited immediately to avoid double-processing
            TileMap[x, y] = 1;

            // Spawn floor
            GameObject floor = Instantiate(FloorModel, new Vector3(x + 0.5f, 0f, y + 0.5f), FloorModel.transform.rotation, FloorParent.transform);

            // Spread to neighbors
            queue.Enqueue(new Vector2Int(x + 1, y));
            queue.Enqueue(new Vector2Int(x - 1, y));
            queue.Enqueue(new Vector2Int(x, y + 1));
            queue.Enqueue(new Vector2Int(x, y - 1));

            // Optional delay for visual build effect
            
            //yield return new WaitForSeconds(Dungeon.BuildTime/4);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
