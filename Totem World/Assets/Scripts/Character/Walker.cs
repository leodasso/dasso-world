using Arachnid;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(PlatformBody))]
public class Walker : PlatformBodyActor, IControllable
{
	[ToggleLeft]
	public bool allowAirMovement;

	[ToggleLeft, Tooltip("Sets the graphic to face left or right depending on the input")]
	public bool setFacingDirection = true;

	[ToggleLeft]
	public bool disableFrictionWhenWalking;
	
	[Range(-1, 1)]
	public float walkInput;
	public FloatReference maxSpeed;
	public FloatReference acceleration;
	[ShowIf("allowAirMovement")]
	public FloatReference airAcceleration;

	Vector2 _speedThisFrame;

	protected override void ActorUpdate()
	{
		base.ActorUpdate();
		if (Mathf.Abs(walkInput) > 0.01f)
		{
			_platformBody.SetFacingDirection(walkInput > 0
				? PlatformBody.FacingDirection.Right
				: PlatformBody.FacingDirection.Left);

			if (disableFrictionWhenWalking)
				_platformBody.frictionEnabled = false;
		}
		else _platformBody.frictionEnabled = true;

		if (_platformBody.Grounded)
		{
			_speedThisFrame = acceleration.Value * walkInput * Time.deltaTime * Vector2.right;
			_platformBody.AddRelativeVelocity(_speedThisFrame);
			_platformBody.ClampVelocityX(maxSpeed.Value);
		}

		else if (allowAirMovement)
		{
			_speedThisFrame = airAcceleration.Value * walkInput * Time.deltaTime * Vector2.right;
			_platformBody.velocity += _speedThisFrame;
			_platformBody.ClampVelocityX(maxSpeed.Value);
		}
	}

	public void ApplyLeftStickInput(Vector2 input)
	{
		walkInput = input.x;
	}

	public void JumpPressed()
	{
	}

	public void JumpReleased()
	{
	}

	public void AlphaPressed()
	{
	}

	public void AlphaReleased()
	{
	}
}
