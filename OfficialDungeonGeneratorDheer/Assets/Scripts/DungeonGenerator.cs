using NaughtyAttributes;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt RectInt = new RectInt(0, 0, 100, 50);
    public List<RectInt> Rooms = new List<RectInt>();
    public int RoomsCount = 0;
    [SerializeField] private Vector2Int MinimumRoomSize = new Vector2Int(6, 6);
    public float CouroutineTime = 0.1f;
    public float SplitBuffer = 0.2f;
    [Button]

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hi
        Rooms.Clear();
        Rooms.Add(RectInt);
        Debug.Log($"Generated {Rooms.Count} rooms");
        StartCoroutine(GenerateDungeon());

    }

    // Update is called once per frame
    void Update()
    {
        foreach(RectInt room in Rooms) 
        {

            AlgorithmsUtils.DebugRectInt(room, color: Color.red); 
        
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
            

            int splitY = Mathf.Clamp(PRect.height / 2 + (int)(PRect.height * SplitBuffer), minHeight, PRect.height - minHeight);

            RectInt RoomA = new RectInt(PRect.x, PRect.y, PRect.width, splitY);
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

            int splitX = Mathf.Clamp(PRect.width / 2 + (int)(PRect.width * SplitBuffer), minWidth, PRect.width - minWidth);

            RectInt RoomA = new RectInt(PRect.x, PRect.y, splitX, PRect.height);
            RectInt RoomB = new RectInt(PRect.x + splitX, PRect.y, PRect.width - splitX, PRect.height);

            return (RoomA, RoomB);
        }
        
    }

    
}
