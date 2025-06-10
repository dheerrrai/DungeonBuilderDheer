using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt RectInt = new RectInt(0, 0, 100, 50);
    public List<RectInt> Rooms = new List<RectInt>();
    public int RoomsCount = 0;
    public float CouroutineTime = 0.1f;

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
        
        List<RectInt> originalRooms = new List<RectInt>(Rooms);
        Rooms.Clear();

        for (int i = 0; i < originalRooms.Count; i++)
        {
            Rooms.Add(originalRooms[i]);
            
            yield return new WaitForSeconds(CouroutineTime);
            (RectInt, RectInt) RoomsSplits = SplitHorizontally(originalRooms[i]);
            
            Rooms.Add(RoomsSplits.Item1);
            
            Rooms.Add(RoomsSplits.Item2);
            Rooms.Remove(Rooms[i]);
        }
        

    }

    private (RectInt, RectInt) SplitHorizontally(RectInt PRect)
    {

        RectInt RoomA = new RectInt(PRect.x, PRect.y, PRect.width,PRect.height/2);
        RectInt RoomB = new RectInt(PRect.x, PRect.height / 2, PRect.width, PRect.height / 2);
        

        return (RoomA, RoomB);
    }
    private (RectInt, RectInt) SplitVertically(RectInt PRect)
    {

        RectInt RoomA = new RectInt(PRect.x, PRect.y, PRect.width/2,PRect.height);
        RectInt RoomB = new RectInt(PRect.width / 2, PRect.y, PRect.width/2, PRect.height);
        

        return (RoomA, RoomB);
    }
}
