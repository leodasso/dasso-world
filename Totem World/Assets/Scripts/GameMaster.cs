using System.Collections;
using System.Collections.Generic;
using Arachnid;
using Rewired;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "HacknodE/Game Master")]
public class GameMaster : ScriptableObject
{
    [TabGroup("main", "Prefabs")]
    [Tooltip("The prefab for our main character, Hack")]
    [HideLabel, BoxGroup("main/Prefabs/Hack", centerLabel:true)]
    public GlobalGameObject hackPrefab;
    
    [HideLabel, BoxGroup("main/Prefabs/Rewired Input Manager", centerLabel:true)]
    public GlobalGameObject rewiredInputPrefab;
    
    [HideLabel, BoxGroup("main/Prefabs/Main Camera", centerLabel:true)]
    public GlobalGameObject cameraPrefab;

    [HideLabel, BoxGroup("main/Prefabs/Default Virtual Camera", centerLabel:true)]
    public GlobalGameObject defaultVirtualCamPrefab;

    [TabGroup("main", "Prefabs"), AssetList(Path = "Prefabs/Transitions")]
    public Transition transitionToGameOver;
    
    [TabGroup("main", "Prefabs")]
    public Collection playerSpawnPoints;

    [TabGroup("main", "Events"), DrawWithUnity]
    public UnityEvent onStageLoaded;

    public Stage startingStage;
    [ReadOnly]
    public Stage currentStage;
    
    [System.NonSerialized, ShowInInspector, ReadOnly]
    public List<Stage> recentStages = new List<Stage>();

    public static GameObject HackInstance => Get().hackPrefab.Instance();
    
    static GameMaster _instance;

    GameObject _rewiredInputInstance;
    GameObject _hackInstance;
    GameObject _mainCameraInstance;
    GameObject _defaultVirtualCamInstance;

    Stage MostRecentStage
    {
        get
        {
            if (recentStages == null) return null;
            if (recentStages.Count < 1) return null;
            return recentStages[0];
        }
    }

    public static GameMaster Get()
    {
        if (_instance) return _instance;

        _instance = Resources.Load<GameMaster>("game master");
        return _instance;
    }

    [Button()]
    void PlaceHackInStage()
    {
        string debugString = "Placing hack in stage.";
        if (MostRecentStage != null)
            debugString += " Most recent stage: " + MostRecentStage.name;
        Debug.Log(debugString);
        
        var allSpawnPoints = playerSpawnPoints.GetElementsOfType<PlayerSpawnPoint>();

        // Check that there are spawn points in the stage
        if (allSpawnPoints.Count < 1)
        {
            Debug.LogError("No spawn points were found while attempting to spawn Hack!");
            return;
        }
       
        foreach (PlayerSpawnPoint spawnPoint in allSpawnPoints)
        {
            // Check for the spawn point linked to the stage we just came from
            if (MostRecentStage != null)
            {
                if (spawnPoint.connectedStage != MostRecentStage) continue;
                spawnPoint.Spawn();
                return;
            }
            
            // check for default spawn point
            if (!spawnPoint.defaultSpawnPoint) continue;
            spawnPoint.Spawn();
            return;
        }
        
        allSpawnPoints[0].Spawn();
    }

    IEnumerator LoadNextStage(Stage stage, float loadDelay)
    {
        yield return new WaitForSecondsRealtime(loadDelay);
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(stage.sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        yield return new WaitForSecondsRealtime(.1f);
        
        onStageLoaded.Invoke();
        
        Get().InstantiateBasics();
    }

    IEnumerator ReloadGame(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Instantiate(transitionToGameOver);
        yield return new WaitForSecondsRealtime(transitionToGameOver.duration);
        BeginGame();
    }

    public static void LoadStage(Stage stage, float loadDelay)
    {        
        Get().recentStages.Insert(0, Get().currentStage);
        if (Get().recentStages.Count > 5) 
            Get().recentStages.RemoveAt(5);
        Get().currentStage = stage;
        
        CoroutineHelper.NewCoroutine(Get().LoadNextStage(stage, loadDelay));
    }

    [Button()]
    public void BeginGame()
    {
        recentStages.Clear();
        currentStage = startingStage;
        InstantiateBasics();
        onStageLoaded.Invoke();
    }

    void InstantiateBasics()
    {
        PlaceHackInStage();
        cameraPrefab.Instance();
    }

    public void ReloadGame()
    {
        CoroutineHelper.NewCoroutine(ReloadGame(2));
    }

    /// <summary>
    /// Returns the rewired input player
    /// </summary>
    public static Player Player()
    {
        Get().rewiredInputPrefab.Instance();
        return ReInput.players.GetPlayer(0);
    }
}
