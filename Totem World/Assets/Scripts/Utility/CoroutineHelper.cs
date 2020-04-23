using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
	static CoroutineHelper _helperInstance;
	static CoroutineHelper HelperInstance
	{
		get
		{
			if (_helperInstance) return _helperInstance;
			GameObject GO = new GameObject("coroutine helper");
			DontDestroyOnLoad(GO);
			_helperInstance = GO.AddComponent<CoroutineHelper>();
			
			return _helperInstance;
		}
	}

	public static void NewCoroutine(IEnumerator coroutine)
	{
		HelperInstance.StartCoroutine(coroutine);
	}
}
