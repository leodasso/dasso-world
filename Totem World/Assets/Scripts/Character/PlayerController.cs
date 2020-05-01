using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public StackableActor myStackableActor;
	
	List<IControllable> _controllables = new List<IControllable>();
	Player _player;

	Vector2 _leftStickInput;

	// Use this for initialization
	void Start ()
	{
		_player = GameMaster.Player();
		RefreshControllablesList();
	}

	/// <summary>
	/// Searches through the full stack and my own game object for controllables.
	/// </summary>
	public void RefreshControllablesList()
	{
		_controllables.Clear();
		foreach (var stackable in myStackableActor.GetFullStack())
		{
			_controllables.AddRange(stackable.MyGameObject().GetComponents<IControllable>());
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		bool jumpPressed = _player.GetButtonDown("jump");
		bool jumpReleased = _player.GetButtonUp("jump");
		
		bool alphaPressed = _player.GetButtonDown("alpha");
		bool alphaReleased = _player.GetButtonUp("alpha");
		
		_leftStickInput = new Vector2(_player.GetAxis("moveH"), _player.GetAxis("moveV"));
		
		
		foreach (var controllable in _controllables)
		{
			controllable.ApplyLeftStickInput(_leftStickInput);
			
			if (jumpPressed) controllable.JumpPressed();
			if (jumpReleased) controllable.JumpReleased();
			
			if (alphaPressed) controllable.AlphaPressed();
			if (alphaReleased) controllable.AlphaReleased();
		}
	}
}
