using NaughtyAttributes;

using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt RectInt = new RectInt(0, 0, 100, 50);
    public List<RectInt> Rooms = new List<RectInt>();
    public int RoomsCount = 0;
    [SerializeField] private Vector2Int MinimumRoomSize = new Vector2Int(6, 6);
    public float CouroutineTime = 0.1f;
    public float SplitBuffer = 0.2f;

    public Graph<RectInt> Graph;

    [Space(10)]
    public GameObject Floor;

    public List<GameObject> WallPos = new List<GameObject>();
    private List<Vector2> PlacedWalls = new List<Vector2>();
    private List<GameObject> PlacedFloors = new();
    public GameObject Wall;
    private GameObject WallParent;
    private GameObject FloorParent;
    public NavMeshSurface NavMeshSurface;
    public float BuildTime = 0.01f;

    [Button]

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hi
        Graph = new Graph<RectInt>();
        Rooms.Clear();
        Rooms.Add(RectInt);
        Debug.Log($"Generated {Rooms.Count} rooms");
        StartCoroutine(GenerateDungeon());

    }

    // Update is called once per frame
    void Update()
    {
        foreach(RectInt Room in Rooms) 
        {

            AlgorithmsUtils.DebugRectInt(Room, color: Color.red);
            //Graph Debug Show
            Vector3 CentreCoodrinate = new Vector3(Room.center.x, 0f, Room.center.y);
            DebugExtension.DebugCircle(CentreCoodrinate);

        }
    }
    public IEnumerator GenerateDungeon()
    {
        
        int SmallestRoomIndex = 0;
        Rooms.Clear();
        Rooms.Add(RectInt); // Start with the initial full area

        while (Rooms.Count < RoomsCount)
        {
            List<RectInt> newRooms = new List<RectInt>();
            bool splitOccurred = false;

            foreach (var room in Rooms)
            {
                if (Rooms.Count + newRooms.Count >= RoomsCount) //If the room was not split, the room count would remain to be greater than or equal to the Final room count
                {
                    newRooms.Add(room); // And preserves the non split room
                    continue;
                }

                var split = Splitlogic(room);
                if (split.HasValue)
                {
                    var (roomA, roomB) = split.Value;
                    newRooms.Add(roomA);
                    newRooms.Add(roomB);
                    splitOccurred = true;

                    
                }
                else
                {
                    newRooms.Add(room); // couldn't split, keep as-is
                }
            }

            // If nothing was split this round, break
            if (!splitOccurred)
                break;

            Rooms = newRooms;
            yield return new WaitForSeconds(CouroutineTime);
        }

        for(int i = 0; i < Rooms.Count; i++) 
        {        
            int SmallestHeight = Rooms[SmallestRoomIndex].height;
            int SmallestWidth = Rooms[SmallestRoomIndex].width;

            if (Rooms[i].height * Rooms[i].width < SmallestHeight * SmallestWidth) SmallestRoomIndex=i; 

        }
        Rooms.Remove(Rooms[SmallestRoomIndex]);

        GraphCreatorAndConnecter(Rooms);

    }

    public void GraphCreatorAndConnecter(List<RectInt> RoomList)
    {
        foreach(RectInt Room in RoomList)
        {
            Graph.AddNode(Room);
            
        }
        Graph.PrintGraph();
    }

    private (RectInt, RectInt)? Splitlogic(RectInt PRect, float? GarunteeBias = 0)
    {
        float aspectRatio = (float)PRect.width / PRect.height;
        float HorizontalSplitChance = 0.5f;
        

        // Bias split chance based on aspect ratio
        if (aspectRatio > 1.25f)
        {
            HorizontalSplitChance = 0.15f; 
        }
        else if (aspectRatio < 0.8f)
        {
            HorizontalSplitChance = 0.85f;
        } 

        

        bool SplitHorizontally = Random.value + GarunteeBias < HorizontalSplitChance;

        float BufferProper = Random.Range(-SplitBuffer, SplitBuffer);

        int minHeight = MinimumRoomSize.y;
        int minWidth = MinimumRoomSize.x;


        if (SplitHorizontally)
        {
            

            
            if (PRect.height < minHeight * 2)
            {

                if (PRect.width > minWidth * 2 && GarunteeBias == 0f)
                {
                    Debug.Log("Recursed");
                    return Splitlogic(PRect, -1f);

                }

                return null; 
            }
            

            int splitY = Mathf.Clamp(PRect.height / 2 + (int)(PRect.height * BufferProper), minHeight, PRect.height - minHeight);

            RectInt RoomA = new RectInt(PRect.x, PRect.y, PRect.width, splitY +1);
            RectInt RoomB = new RectInt(PRect.x, PRect.y + splitY, PRect.width, PRect.height - splitY);

            return (RoomA, RoomB);
        }
        else
        {

            if (PRect.width < minWidth * 2)
            {

                if (PRect.height > minHeight * 2 && GarunteeBias == 0f)
                {
                    Debug.Log("Recursed");
                    return Splitlogic(PRect, 1f);

                }

                return null;
            }

            int splitX = Mathf.Clamp(PRect.width / 2 + (int)(PRect.width * BufferProper), minWidth, PRect.width - minWidth);

            RectInt RoomA = new RectInt(PRect.x, PRect.y, splitX + 1, PRect.height  );
            RectInt RoomB = new RectInt(PRect.x + splitX, PRect.y, PRect.width - splitX, PRect.height );

            return (RoomA, RoomB);
        }
        
    }

    [Button]
    public IEnumerator SpawnDungeonAssets()
    {
        Destroy(WallParent);
        Destroy(FloorParent);

        WallParent = new GameObject("WallParent");
        FloorParent = new GameObject("FloorParent");




        PlacedFloors.Clear();
        WallPos.Clear();
        PlacedWalls.Clear();
        foreach (RectInt i in Rooms)
        {
            foreach (Vector2 j in i.allPositionsWithin)
            {
                
                //if ((j.x == i.xMax - 1 || j.x == i.xMin || j.y == i.yMax - 1 || j.y == i.yMin) && j != Door.position && !PlacedWalls.Contains(j))
                if ((j.x == i.xMax - 1 || j.x == i.xMin || j.y == i.yMax - 1 || j.y == i.yMin) && !PlacedWalls.Contains(j))
                {
                    yield return new WaitForNextFrameUnit();
                    GameObject WallPiece = Instantiate(Wall, new Vector3(j.x + 0.5f, 0.5f, j.y + 0.5f), Wall.transform.rotation, WallParent.transform);
                    WallPos.Add(WallPiece);
                    PlacedWalls.Add(j);
                }


            }
            FloorGenerate(i, i.width, i.height);


        }
        StartCoroutine(Wait());



    }
    public void FloorGenerate(RectInt Room, float Width, float Height)
    {
        GameObject FloorObj = Instantiate(Floor, new Vector3(Room.center.x, 0f, Room.center.y), Floor.transform.rotation, FloorParent.transform);
        FloorObj.transform.localScale = new Vector3(Width, Height, 1f);
    }

    // Create the BakeNavMesh() function here
    [Button]
    public void BakeNavMesh()
    {

    }
    IEnumerator Wait()
    {

        yield return new WaitForSeconds(0.1f);
        NavMeshSurface.BuildNavMesh();
    }


}
