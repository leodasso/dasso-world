using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class HacknodeMenu 
{

    [MenuItem("HacknodE/Game Master")]
    static void SelectGameMaster()
    {
        Selection.activeObject = GameMaster.Get();
    }

    [MenuItem("HacknodE/Begin Game %b")]
    static void BeginGame()
    {
        GameMaster.Get().BeginGame();
    }
}
