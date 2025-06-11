using UnityEngine;

public class FloodFiller : MonoBehaviour
{
    private TileMapGenerator TileScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TileScript = GetComponent<TileMapGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
