using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap : MonoBehaviour
{
    public StagePoint startingPoint;
    public WorldMapGuy worldMapGuy;
    
    // Start is called before the first frame update
    void Start()
    {
        // Place guy at starting point
        worldMapGuy.SetStagePoint(startingPoint);
    }
}
