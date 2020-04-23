using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	Player _player;

	Walker Walker
	{
		get
		{
			if (_walker) return _walker;
			_walker = GetComponent<Walker>();
			return _walker;
		}
	}

	Jumper Jumper
	{
		get
		{
			if (_jumper) return _jumper;
			_jumper = GetComponent<Jumper>();
			return _jumper;
		}
	}
	
	Walker _walker;
	Jumper _jumper;

	// Use this for initialization
	void Start ()
	{
		_player = GameMaster.Player();
	}
	
	// Update is called once per frame
	void Update ()
	{
		Walker.walkInput = _player.GetAxis("moveH");
		if (_player.GetButtonDown("jump"))
			Jumper?.BeginJump();
		if (_player.GetButtonUp("jump"))
			Jumper?.EndJump();
	}
}
