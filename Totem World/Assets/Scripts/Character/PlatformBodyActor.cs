using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlatformBody))]
public class PlatformBodyActor : MonoBehaviour 
{
	protected PlatformBody _platformBody;

	protected bool CanAct()
	{
		if (_platformBody == null) return false;
		return !_platformBody.AbilitiesDisabled;
	}
	
	// Use this for initialization
	protected virtual void Start () 
	{
		_platformBody = GetComponent<PlatformBody>();
		_platformBody.onGrounded += OnGrounded;
		_platformBody.onUngrounded += OnUngrounded;
		_platformBody.onNormalChange += OnNormalChanged;
		_platformBody.onWallHit += OnWallHit;
		_platformBody.onEdgeReached += OnEdgeReached;
	}

	protected virtual void Update()
	{
		if (!CanAct()) return;
		ActorUpdate();
	}

	protected virtual void ActorUpdate()
	{
		
	}

	protected virtual void OnGrounded()
	{
		
	}

	protected virtual void OnUngrounded()
	{
		
	}

	protected virtual void OnNormalChanged(Vector2 newNormal)
	{
		
	}

	protected virtual void OnWallHit(RaycastHit2D hit)
	{
		
	}

	protected virtual void OnEdgeReached()
	{
		
	}
}
