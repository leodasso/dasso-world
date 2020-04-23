using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class GlobalGameObject
{
    [HorizontalGroup(), HideLabel, AssetsOnly]
    public GameObject prefab;
    
    [ToggleLeft, HorizontalGroup()]
    public bool dontDestroyOnLoad;
    
    public GameObject Instance()
    {
        if (_instance) return _instance;

        _instance = GameObject.Instantiate(prefab);
        if (dontDestroyOnLoad) GameObject.DontDestroyOnLoad(_instance);
        return _instance;
    }

    GameObject _instance;
}
