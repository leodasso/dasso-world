using Arachnid;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(PlatformBody))]
public class Walker : PlatformBodyActor
{
	[ToggleLeft]
	public bool allowAirMovement;

	[ToggleLeft, Tooltip("Sets the graphic to face left or right depending on the input")]
	public bool setFacingDirection = true;
	
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
			_platformBody.SetFacingDirection( walkInput > 0 ? PlatformBody.FacingDirection.Right : PlatformBody.FacingDirection.Left);
		
		if (_platformBody.Grounded)
		{
			_speedThisFrame = acceleration.Value * _platformBody.CurrentFriction * walkInput * Time.deltaTime * Vector2.right;
			_platformBody.AddRelativeVelocity(ClampedSpeed(_speedThisFrame));
		}

		else if (allowAirMovement)
		{
			_speedThisFrame = airAcceleration.Value * walkInput * Time.deltaTime * Vector2.right;
			_platformBody.velocity += ClampedSpeed(_speedThisFrame);
		}
	}

	
	/// <summary>
	/// Returns a speed clamped so that when added to the platform body, it will stay within max speed
	/// </summary>
	Vector2 ClampedSpeed(Vector2 inputSpeed)
	{
		float currentVelocity = Mathf.Abs(_platformBody.RelativeVelocity.x);
		float allowedVelocty = maxSpeed.Value - currentVelocity;
		return Vector2.ClampMagnitude(inputSpeed, allowedVelocty);
	}
}
