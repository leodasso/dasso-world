using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerSpawnPoint : MonoBehaviour
{
	[ToggleLeft, Tooltip("Is this the default spawn point for the stage? This will be used if no spawn point for the previous stage is found." +
	                     " There should really only be one of these per stage.")] 
	public bool defaultSpawnPoint;
	
	[AssetsOnly]
	public Stage connectedStage;

	[EnumToggleButtons]
	public PlatformBody.FacingDirection startingDirection;

	public void Spawn()
	{
		GameObject newHack = GameMaster.HackInstance;
		newHack.transform.position = transform.position;
		PlatformBody body = newHack.GetComponent<PlatformBody>();
		body.enabled = true;
		body.SetFacingDirection(startingDirection);
		
	}
}
