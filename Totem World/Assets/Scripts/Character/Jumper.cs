using System.Collections;
using System.Collections.Generic;
using Arachnid;
using UnityEngine;
using Sirenix.OdinInspector;

public class Jumper : PlatformBodyActor
{
	public enum JumpType
	{
		WorldUp, GroundNormal, OppositeOfGravity
	}

	public FloatReference jumpCooldown;

	[EnumToggleButtons]
	public JumpType jumpType;
	
	public CurveObject jumpVelocityCurve;
	[ReadOnly]
	public bool canJump;

	[ReadOnly, System.NonSerialized, ShowInInspector]
	public bool jumping;

	[ReadOnly]
	public float jumpTime;

	float _velocity;
	float _cooldown;

	protected override void Update()
	{
		base.Update();
		if (_cooldown > 0 && canJump) 
			_cooldown -= Time.deltaTime;
	}

	protected override void ActorUpdate()
	{
		base.ActorUpdate();
		if (jumping)
		{
			jumpTime += Time.deltaTime;
			_velocity = jumpVelocityCurve.ValueFor(jumpTime);
			Vector2 totalVelocity = JumpDirection() * _velocity * Time.deltaTime;
			_platformBody.velocity += totalVelocity;
		}
	}

	Vector2 JumpDirection()
	{
		switch (jumpType)
		{
				case JumpType.GroundNormal:
					return _platformBody.GroundNormal.normalized;
				
				case JumpType.OppositeOfGravity:
					return -_platformBody.gravity.normalized;
				
				case JumpType.WorldUp:
					return Vector2.up;
				
				default: return Vector2.up;
		}
	}

	protected override void OnGrounded()
	{
		base.OnGrounded();
		EndJump();
		ResetJump();
	}

	protected override void OnUngrounded()
	{
		base.OnUngrounded();
		canJump = false;
	}

	void ResetJump()
	{
		canJump = true;
		jumpTime = 0;
	}

	public void BeginJump()
	{
		if (!CanAct()) return;
		if (!canJump) return;
		if (_cooldown > 0) return;
		canJump = false;
		jumping = true;
		_platformBody.SetNoGroundingTime(.25f);
		_platformBody.UnGround();
		_cooldown = jumpCooldown.Value;

	}

	public void EndJump()
	{
		jumping = false;
	}
}
