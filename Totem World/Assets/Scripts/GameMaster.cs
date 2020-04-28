using System.Collections;
using System.Collections.Generic;
using Arachnid;
using Rewired;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Totem/Game Master")]
public class GameMaster : ScriptableObject
{
    [TabGroup("main", "Prefabs")]
    [Tooltip("The prefab for our main character, Hack")]
    [HideLabel, BoxGroup("main/Prefabs/Player", centerLabel:true)]
    public GlobalGameObject playerPrefab;
    
    [HideLabel, BoxGroup("main/Prefabs/Rewired Input Manager", centerLabel:true)]
    public GlobalGameObject rewiredInputPrefab;

    public Collection players;

    [TabGroup("main", "Prefabs"), AssetList(Path = "Prefabs/Transitions")]
    public Transition transitionToGameOver;
    
    [TabGroup("main", "Prefabs")]
    public Collection playerSpawnPoints;

    [TabGroup("main", "Events"), DrawWithUnity]
    public UnityEvent onStageLoaded;

    public Stage startingStage;
    [ReadOnly]
    public Stage currentStage;

    public static GameObject playerInstance => Get().playerPrefab.Instance();
    
    static GameMaster _instance;

    GameObject _rewiredInputInstance;
    GameObject _hackInstance;
    GameObject _mainCameraInstance;
    GameObject _defaultVirtualCamInstance;

    public static GameMaster Get()
    {
        if (_instance) return _instance;

        _instance = Resources.Load<GameMaster>("game master");
        return _instance;
    }

    public static Collection PlayerCharacterCollection()
    {
        return Get().players;
    }
    
    
    IEnumerator LoadNextStage(Stage stage, float loadDelay)
    {
        yield return new WaitForSecondsRealtime(loadDelay);
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(stage.sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        yield return new WaitForSecondsRealtime(.1f);
        
        onStageLoaded.Invoke();
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
        Get().currentStage = stage;
        CoroutineHelper.NewCoroutine(Get().LoadNextStage(stage, loadDelay));
    }

    [Button()]
    public void BeginGame()
    {
        currentStage = startingStage;
        onStageLoaded.Invoke();
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
