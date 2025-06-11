using NaughtyAttributes;

using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt RectInt = new RectInt(0, 0, 100, 50);
    public List<RectInt> Rooms = new List<RectInt>();
    public int RoomsCount = 0;
    [SerializeField] private Vector2Int MinimumRoomSize = new Vector2Int(6, 6);
    public float CouroutineTime = 0.1f;
    public float SplitBuffer = 0.2f;

    public int DoorRange=2;
    public Graph GraphScript;
    public Graph<RectInt> DungeonGraph;

    public List<Vector2Int> DoorCoords = new List<Vector2Int>();

    public float YHeight = 5f;
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



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hi
        
        
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
        DungeonGraph = new Graph<RectInt>();        
        DoorCoords.Clear();
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

        StartCoroutine(GraphCreatorAndConnecter(Rooms));
        StartCoroutine(CreateDoorsForOverlappingRooms(Rooms));
        StartCoroutine(SpawnDungeonAssets());
    }
    

    public IEnumerator CreateDoorsForOverlappingRooms(List<RectInt> rooms)
    {
        DoorCoords.Clear();
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                RectInt RoomA = rooms[i];
                RectInt RoomB = rooms[j];
                int xMin = 0;
                int xMax = 0;
                int yMin = 0;
                int yMax = 0;
                if (RoomA.Overlaps(RoomB))
                {
                    RectInt OverLap = AlgorithmsUtils.Intersect(RoomA, RoomB);
                    int doorX = 0;
                    int doorY = 0;
                    if (OverLap.width < OverLap.height)
                    {

                        // Get the range of the door spot
                        xMin = Mathf.Max(RoomA.xMin, RoomB.xMin);
                        xMax = Mathf.Min(RoomA.xMax - 2, RoomB.xMax - 2);
                        yMin = Mathf.Max(RoomA.yMin , RoomB.yMin );
                        yMax = Mathf.Min(RoomA.yMax - 2, RoomB.yMax - 2);
                        
                        doorX = OverLap.x;
                        doorY = Random.Range(yMin+1, yMax);
                    }
                    else
                    {

                        // Get the range of the door spot
                        xMin = Mathf.Max(RoomA.xMin + 2, RoomB.xMin +2);
                        xMax = Mathf.Min(RoomA.xMax , RoomB.xMax );
                        yMin = Mathf.Max(RoomA.yMin + 2, RoomB.yMin + 2);
                        yMax = Mathf.Min(RoomA.yMax, RoomB.yMax);
                        
                        doorX = Random.Range(xMin, xMax-1);
                        doorY = OverLap.y;
                    }
                    //Choose a point in the overlapping area for a door

                    


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
    }

    
    public IEnumerator GraphCreatorAndConnecter(List<RectInt> RoomList)
    {
        foreach(RectInt Room in RoomList)
        {
            yield return new WaitForSeconds(BuildTime);
            DungeonGraph.AddNode(Room);
            
        }
        //DungeonGraph.PrintGraph();
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
        



    }
    public void FloorGenerate(RectInt Room, float Width, float Height)
    {
        GameObject FloorObj = Instantiate(Floor, new Vector3(Room.center.x, 0f, Room.center.y), Floor.transform.rotation, FloorParent.transform);
        FloorObj.transform.localScale = new Vector3(Width, Height, 1f);
    }

    
    
    IEnumerator Wait()
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
