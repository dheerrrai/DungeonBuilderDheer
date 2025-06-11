using NaughtyAttributes;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

using RangeAttribute = UnityEngine.RangeAttribute;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Seed Generation")]
    public bool UseSeed = false;
    public System.Random RandomSys;
    public int Seed;

    [Space(20)]

    [Header("Rooms")]
    public RectInt StartingRoom = new RectInt(0, 0, 100, 50);
    public List<RectInt> Rooms = new List<RectInt>();
    public int RoomsCount = 0;
    [SerializeField] private Vector2Int MinimumRoomSize = new Vector2Int(6, 6);
    [Range(0f, 0.5f)]
    public float SplitBuffer = 0.2f;

    public int DoorRange=2;
    public Graph GraphScript;
    public Graph<RectInt> DungeonGraph;
    private TileMapGenerator TileMapScript;

    public List<Vector2Int> DoorCoords = new List<Vector2Int>();

    public float YHeight = 5f;
    [Space(10)]
    public GameObject Floor;
    [Header("Asset Spawning")]
    public List<GameObject> WallPos = new List<GameObject>();
    public List<Vector2> PlacedWalls = new List<Vector2>();
    private List<GameObject> PlacedFloors = new();
    public GameObject Wall;
    private GameObject WallParent;
    private GameObject FloorParent;
    public NavMeshSurface NavMeshSurface;
    public float BuildTime = 0.01f;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hi
        TileMapScript = GetComponent<TileMapGenerator>();
        
        StartCoroutine(GenerateDungeon());

    }

    // Update is called once per frame
    void Update()
    {
        foreach(RectInt Room in Rooms) 
        {

            AlgorithmsUtils.DebugRectInt(Room, color: Color.red);
            

        }


    }
    [Button]
    public IEnumerator GenerateDungeon()
    {
        if (UseSeed) RandomSys = new System.Random(Seed);
        else
        {
            RandomSys = new System.Random(System.Environment.TickCount);
            Seed = System.Environment.TickCount;
        }
        DungeonGraph = new Graph<RectInt>();        
        DoorCoords.Clear();
        
        Rooms.Clear();
        Rooms.Add(StartingRoom); // Start with the initial full area

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
            yield return new WaitForSeconds(BuildTime);
        }


        
        StartCoroutine(GraphCreatorAndConnecter(Rooms));
        
    }
    
    public IEnumerator SmallestRoomRemover(List<RectInt>Rooms)
    {
        List<int> RoomIndexes = new List<int>();
        List<int> RoomAreas = new List<int>();
        for (int i = 0; i < Rooms.Count - 1; i++)
        {
            RoomAreas.Add(Rooms[i].height * Rooms[i].width);
            RoomIndexes.Add(i);
        }
        for(int i = 0;i < RoomIndexes.Count-1; i++)
        {
            bool Swapped = false;
            for(int j = 0; j < RoomIndexes.Count - i - 1; j++)
            {
                if (RoomAreas[j] > RoomAreas[j + 1])
                {
                    int temp = RoomAreas[j];
                    RoomAreas[j] = RoomAreas[j + 1];
                    RoomAreas[j + 1] = temp;
                    temp = RoomIndexes[j];
                    RoomIndexes[j] = RoomIndexes[j + 1];
                    RoomIndexes[j + 1] = temp;
                    Swapped = true;
                }
            }
            if (!Swapped)
                break;
        }
        int minimum = RoomIndexes.Count / 10;
        List<RectInt> smallestRooms = RoomIndexes
            .GetRange(0, minimum)
            .Select(index => Rooms[index])
            .ToList();
        foreach (RectInt tempRoom in smallestRooms)
        {
            // Clone the full graph
            Graph<RectInt> SimulatedGraph = DungeonGraph;

            SimulatedGraph.RemoveNode(tempRoom);

            // Now remove any doors that were only connected to that room
            List<RectInt> orphanedDoors = SimulatedGraph.GetAllNodes()
                .Where(node => !Rooms.Contains(node)) // Only doors
                .Where(door => SimulatedGraph.FindLinks(door).Count == 0) // No connections left
                .ToList();

            foreach (var door in orphanedDoors)
            {
                SimulatedGraph.RemoveNode(door);
            }

            // Check if it's still connected
            RectInt? startRoom = Rooms.FirstOrDefault(r => !r.Equals(tempRoom));
            if (startRoom == null)
            {
                Debug.LogWarning("No valid start room found for BFS");
                continue; // Skip this room
            }
            int reachable = SimulatedGraph.BFS(startRoom.Value).Count;
            int expected = SimulatedGraph.GetAllNodes().Count();

            if (reachable == expected)
            {
                List<RectInt> connectedDoors = DungeonGraph.FindLinks(tempRoom)
                    .Where(linked => DoorCoords.Contains(linked.position)) // Is a door
                    .ToList();

                foreach (RectInt door in connectedDoors)
                {
                    List<RectInt> linkedRooms = DungeonGraph.FindLinks(door)
                        .Where(link => Rooms.Contains(link))
                        .ToList();

                    // If the door only connects to the room being removed OR no other valid rooms, remove it
                    if (linkedRooms.Count <= 2)
                    {
                        DungeonGraph.RemoveNode(door);
                        DoorCoords.Remove(door.position); // Clean from door list
                    }
                }

                // Now remove the room
                Rooms.Remove(tempRoom);
                DungeonGraph.RemoveNode(tempRoom);
            }
            else
            {
                Debug.Log($"Room {tempRoom} removal would break connectivity");
            }
            yield return new WaitForSeconds(BuildTime);
        }
        
        
        foreach (var door in DoorCoords)
        {
            var doorRect = new RectInt(door, new Vector2Int(1, 1));
            var links = DungeonGraph.FindLinks(doorRect);
            if (links.Count < 2) // Not connected to at least two rooms
            {
                yield return new WaitForSeconds(BuildTime);
                DungeonGraph.RemoveNode(doorRect);
                DoorCoords.Remove(door);
            }
        }



        StartCoroutine(SpawnDungeonAssetsAndTileMapCreation());



    }

    public IEnumerator CreateDoorsForOverlappingRooms(List<RectInt> Rooms)
    {
        DoorCoords.Clear();
        for (int i = 0; i < Rooms.Count; i++)
        {
            for (int j = i + 1; j < Rooms.Count; j++)
            {
                RectInt RoomA = Rooms[i];
                RectInt RoomB = Rooms[j];
                int xMin = 0;
                int xMax = 0;
                int yMin = 0;
                int yMax = 0;
                if (RoomA.Overlaps(RoomB))
                {
                    RectInt OverLap = AlgorithmsUtils.Intersect(RoomA, RoomB);
                    int doorX = 0;
                    int doorY = 0;
                    
                    //Choose a point in the overlapping area for a door

                    if (OverLap.width < OverLap.height)
                    {
                        // Vertical wall => door on vertical face
                        xMin = Mathf.Max(RoomA.xMin, RoomB.xMin);
                        xMax = Mathf.Min(RoomA.xMax, RoomB.xMax);

                        yMin = Mathf.Max(RoomA.yMin + 1, RoomB.yMin + 1);
                        yMax = Mathf.Min(RoomA.yMax - 1, RoomB.yMax - 1);

                        if (yMax <= yMin) continue; // Skip if range invalid
                        doorX = OverLap.x;
                        doorY = RandomSys.Next(yMin, yMax);
                    }
                    else
                    {
                        // Horizontal wall => door on horizontal face
                        yMin = Mathf.Max(RoomA.yMin, RoomB.yMin);
                        yMax = Mathf.Min(RoomA.yMax, RoomB.yMax);

                        xMin = Mathf.Max(RoomA.xMin + 1, RoomB.xMin + 1);
                        xMax = Mathf.Min(RoomA.xMax - 1, RoomB.xMax - 1);

                        if (xMax <= xMin) continue; // Skip if range invalid
                        doorX = RandomSys.Next(xMin, xMax);
                        doorY = OverLap.y;
                    }

                    int overlapWidth = xMax  - xMin;
                    int overlapHeight = yMax  - yMin;


                    if (overlapWidth <= 2 && overlapHeight <= 2) continue; //Basically removes 1x1 corners that overlap more than one rect
                    //int doorX = ((xMin + xMax) / 2);
                    //int doorY = ((yMin + yMax) / 2);
                    
                    

                    

                    Vector2Int NewCoords = new Vector2Int(doorX, doorY);

                    RectInt DoorRect = new RectInt(NewCoords, new Vector2Int(1, 1));

                    if (!DoorCoords.Contains(NewCoords))
                    //PlaceDoor(new Vector2Int(doorX, doorY);
                    {
                        yield return new WaitForSeconds(BuildTime);

                        DungeonGraph.AddNode(DoorRect);
                        DoorCoords.Add(NewCoords);
                        DungeonGraph.AddLink(RoomA, DoorRect);
                        DungeonGraph.AddLink(DoorRect, RoomB);
                    }
                }
            }
            //if (overlapWidth > 7 && overlapHeight > 7)
            //{
            //    DoorCoords.Add(new Vector2Int(NewCoords.x, NewCoords.y));
            //}


        }
        StartCoroutine(SmallestRoomRemover(Rooms));
        
    }

    
    public IEnumerator GraphCreatorAndConnecter(List<RectInt> RoomList)
    {
        foreach(RectInt Room in RoomList)
        {
            yield return new WaitForSeconds(BuildTime);
            DungeonGraph.AddNode(Room);
            
        }

        StartCoroutine(CreateDoorsForOverlappingRooms(Rooms));

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

        

        bool SplitHorizontally = (RandomSys.Next(0,101)/100f) + GarunteeBias < HorizontalSplitChance;

        //float BufferProper = RandomSys.Next(-SplitBuffer, SplitBuffer);

        int minHeight = MinimumRoomSize.y;
        int minWidth = MinimumRoomSize.x;


        if (SplitHorizontally)
        {
            

            
            if (PRect.height < minHeight * 2)
            {

                if (PRect.width > minWidth * 2 && GarunteeBias == 0f)
                {
                    Debug.Log("Recursed");
                    return Splitlogic(PRect, -1f); //RECURSION

                }

                return null; 
            }
            

            int splitY = RandomSys.Next(minHeight, PRect.height - minHeight+1);

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

            int splitX = RandomSys.Next(minWidth, PRect.width - minWidth + 1);

            RectInt RoomA = new RectInt(PRect.x, PRect.y, splitX + 1, PRect.height  );
            RectInt RoomB = new RectInt(PRect.x + splitX, PRect.y, PRect.width - splitX, PRect.height );

            return (RoomA, RoomB);
        }
        
    }

    public IEnumerator SpawnDungeonAssetsAndTileMapCreation()
    {
        Destroy(WallParent);
        Destroy(FloorParent);

        WallParent = new GameObject("WallParent");
        FloorParent = new GameObject("FloorParent");




        PlacedFloors.Clear();
        WallPos.Clear();
        PlacedWalls.Clear();

        TileMapScript.GenerateTileMap(StartingRoom.size, Rooms);

        foreach (RectInt i in Rooms)
        {
            foreach (Vector2Int j in i.allPositionsWithin)
            {
                
                
                if ((j.x == i.xMax - 1 || j.x == i.xMin || j.y == i.yMax - 1 || j.y == i.yMin) && !PlacedWalls.Contains(j) && !DoorCoords.Contains(j))
                {
                    yield return new WaitForSeconds(BuildTime);
                    GameObject WallPiece = Instantiate(Wall, new Vector3(j.x + 0.5f, 0.5f, j.y + 0.5f), Wall.transform.rotation, WallParent.transform);
                    WallPos.Add(WallPiece);
                    PlacedWalls.Add(j);
                }


            }
            FloorGenerate(i, i.width, i.height);


        }
        //if ()
        //{
        //    NavMeshBuilder();
        //}



    }
    public void FloorGenerate(RectInt Room, float Width, float Height)
    {
        GameObject FloorObj = Instantiate(Floor, new Vector3(Room.center.x, 0f, Room.center.y), Floor.transform.rotation, FloorParent.transform);
        FloorObj.transform.localScale = new Vector3(Width, Height, 1f);
    }

    
    
    IEnumerator NavMeshBuilder()
    {

        yield return new WaitForSeconds(0.1f);
        NavMeshSurface.BuildNavMesh();
    }

    public void OnDrawGizmos()
    {
        
        {
            if (DungeonGraph == null) return;

            foreach (RectInt Room in Rooms)
            {
                if (!DungeonGraph.ContainsNode(Room)) continue;
                Vector3 CentreCoodrinate = new Vector3(Room.center.x, YHeight, Room.center.y);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(CentreCoodrinate, 1f);
                foreach(RectInt Nodes in DungeonGraph.FindLinks(Room))
                {
                    Vector3 CentreNodeCoodrinate = new Vector3(Nodes.center.x, YHeight, Nodes.center.y);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(CentreCoodrinate, CentreNodeCoodrinate);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(CentreNodeCoodrinate, 1f);
                }
            }
            
        }
    }
}
