using Sirenix.OdinInspector;
using UnityEngine;

public class Logger : MonoBehaviour
{

	public string thingToLog = "dickbutt.";

	[Button]
	public void Log()
	{
		Debug.Log(thingToLog, gameObject);
	}
}
