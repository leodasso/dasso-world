using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arachnid;
using UnityEngine;
using Sirenix.OdinInspector;


public class PlatformBody : StackBehavior
{
	public enum FacingDirection
	{
		Left, Right
	}
	
	[Tooltip("The dimensions of this capsule collider are used for collision.")]
	public CapsuleCollider2D capsuleCollider;

	[Tooltip("If this is going to act as a stack controller, must be linked here")]
	public StackableActor stackable;
	
	[Space]
	public RaycastSettings raycastSettings;
	public PlatformBodyCollisionSettings collisionSettings;

	[Tooltip("Resistance to movement while in the air")]
	public float drag = 2;

	[Tooltip("Resistance to movement on the ground")]
	public float friction = 5;
	
	[Tooltip("There's a raycast to check for upcoming cliffs in the direction of momement to call event OnEdgeReached. " +
	         "This value determines how far the drop has to be before it's determined a cliff")]
	public float minimumCliffHeight = .5f;
	
	[ToggleLeft, LabelText("Gravity"), HorizontalGroup("Gravity")]
	public bool useGravity;
	
	[ShowIf("useGravity"), HorizontalGroup("Gravity"), HideLabel]
	public Vector2 gravity;
	
	[ReadOnly]
	public Vector2 velocity;

	[Tooltip("The movement gained from what I'm standing on. Not affected by drag or friction"), ReadOnly]
	public Vector2 inheritedTranslation;

	[EnumToggleButtons, ReadOnly]
	public FacingDirection facingDirection = FacingDirection.Right;
	
	[ReadOnly, LabelText("Current"), LabelWidth(80)]
	public float groundRotation;

	[ ReadOnly]
	public Collider2D groundImOn;
	
	[ ReadOnly]
	public Collider2D wallImAgainst;
	
	Vector2 _thisFrameTranslation;

	[ReadOnly, ToggleLeft]
	public bool frictionEnabled = true;

	public bool AbilitiesDisabled => _abilityDisabledTimer > 0;
	public bool Grounded => groundImOn != null;
	public Vector2 GroundNormal => _groundHitNormal;

	[ToggleLeft, ReadOnly]
	public bool penetratingGround;

	public Action<GameObject> onGrounded;
	public Action onUngrounded;
	public Action<RaycastHit2D> onWallHit;
	/// <summary>
	/// Called when the ground normal changes. Parameter is the new normal
	/// </summary>
	public Action<Vector2> onNormalChange;
	public Action onEdgeReached;

	public float _horizontalCollisionRayLength => capsuleCollider.size.x / 2;

	CapsuleCollider2D ColliderForRoofCollision =>
		stackable ? stackable.TopOfStack().MyGameObject().GetComponent<CapsuleCollider2D>() : capsuleCollider;

	public Vector2 RelativeVelocity
	{
		get {return transform.InverseTransformDirection(velocity);}
		private set {velocity = transform.TransformDirection(value);}
	}

	public bool IsAgainstWall()
	{
		return wallImAgainst != null;
	}
	

	// If this is above 0, grounding isn't possible. Used to prevent immediate re-grounding after jumping
	float _noGroundingTime;
	float _abilityDisabledTimer;
	Vector2 _groundHitNormal;
	Vector2 _initGraphicScale;
	bool _atEdge;

	void OnDrawGizmos()
	{
		if (capsuleCollider == null) capsuleCollider = GetComponent<CapsuleCollider2D>();
		if (!raycastSettings) return;
		
		raycastSettings.verticalCollisions.DrawGizmos(Color.yellow, capsuleCollider.size.y/2, 
			RaycastGroup.CastingDirection.Vertical, transform.up, capsuleCollider);
		
		raycastSettings.verticalCollisions.DrawGizmos(Color.yellow, capsuleCollider.size.y/2, 
			RaycastGroup.CastingDirection.Vertical, -transform.up, capsuleCollider);
		
		raycastSettings.horizontalCollisions.DrawGizmos(Color.magenta, capsuleCollider.size.x / 2, 
			RaycastGroup.CastingDirection.Horizontal, -transform.right, capsuleCollider);
		
		raycastSettings.horizontalCollisions.DrawGizmos(Color.magenta, capsuleCollider.size.x / 2, 
			RaycastGroup.CastingDirection.Horizontal, transform.right, capsuleCollider);
		
		raycastSettings.groundSnapCasting.DrawGizmos(Color.green, capsuleCollider.size.y/2, 
			RaycastGroup.CastingDirection.Vertical, -transform.up, capsuleCollider);

		raycastSettings.normalProbeCasting.DrawGizmos(Color.blue, capsuleCollider.size.y / 2,
			RaycastGroup.CastingDirection.Vertical, -transform.up, capsuleCollider);
	}

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{		
		if (_noGroundingTime > 0)
			_noGroundingTime -= Time.deltaTime;

		if (_abilityDisabledTimer > 0)
			_abilityDisabledTimer -= Time.deltaTime;
	}

	void LateUpdate()
	{
		// Note: The order of these functions is important, so even though it's kind of messy to have
		// multiple 'behavior allowed by stack' checks, it's necessary.
		if (BehaviorAllowedByStack())
		{
			ProcessGravity();
			ProcessDrag();
			ProcessFriction();
		}
		else velocity = Vector2.zero;

		_thisFrameTranslation = velocity * Time.deltaTime + inheritedTranslation;
		inheritedTranslation = Vector2.zero;
		
		if (BehaviorAllowedByStack())
		{
			ProcessRoofCollision();
			ProcessGroundCollision();
			//CheckForCliffs();
			//ProcessGroundNormal();
		}

		transform.Translate(_thisFrameTranslation, Space.World);
		
		if (BehaviorAllowedByStack())
			ProcessHorizontalCollision();
	}

	public void DisableAbilitiesForTime(float time)
	{
		_abilityDisabledTimer = time;
	}

	public void SetFacingDirection(FacingDirection newDirection)
	{
		if (newDirection == facingDirection) return;
		facingDirection = newDirection;
	}

	public void AddRelativeVelocity(Vector2 newVelocity)
	{
		Vector2 finalVel = transform.TransformDirection(newVelocity);
		velocity += finalVel;
	}

	public void ClampVelocityX(float max)
	{
		float clampedSpeed = Mathf.Clamp(velocity.x, -max, max);
		velocity = new Vector2(clampedSpeed, velocity.y);
	}
	

	public Vector2 GroundDirection()
	{
		if (!Grounded) return Vector2.zero;
		float dir = RelativeVelocity.x > 0 ? 1 : -1;
		Vector3 cross = Vector3.Cross(Vector3.back, _groundHitNormal);
		return cross * dir;
	}

	public void SetNoGroundingTime(float time)
	{
		_noGroundingTime = Mathf.Clamp(_noGroundingTime, time, 99);
	}
	
	void ProcessDrag()
	{
		if (Grounded) return;
		velocity = Vector2.Lerp(velocity, Vector2.zero, drag * Time.deltaTime);
	}

	void ProcessFriction()
	{
		if (!Grounded || !frictionEnabled) return;
		
		velocity = Vector2.Lerp(velocity, Vector2.zero, friction * Time.deltaTime);
	}

	void ProcessGravity()
	{
		if (!useGravity) return;
		if (Grounded) return;

		velocity += Time.deltaTime * gravity;
	}


	void ProcessGroundCollision()
	{
		// Get left and right start points for raycasting. We'll be casting down from these points 
		// and intervals in between them. The total amount of rays depends on the raycast settings.
		Vector2 castingLeft = raycastSettings.verticalCollisions.CastingLeft(capsuleCollider);
		Vector2 castingRight = raycastSettings.verticalCollisions.CastingRight(capsuleCollider);
		
		var groundHit = raycastSettings.verticalCollisions.RaycastDirection(
			-transform.up, castingLeft, castingRight, 
			capsuleCollider.size.y / 2 + Mathf.Abs(RelativeVelocity.y * Time.deltaTime), 
			collisionSettings.terrain | collisionSettings.oneWayPlatforms, capsuleCollider);
		
		// If there's no raycast hit, check if we should be ungrounded, and return
		if (!groundHit)
		{
			if (groundImOn != null) OnUngrounded();
			groundImOn = null;
			return;
		}
		
		//Debug.DrawRay(groundHit.point, Vector3.up, Color.red, .25f);
		
		if (groundHit.collider)
		{
			bool isOneWayPlatform = LayerIsInLayerMask(groundHit.collider.gameObject.layer, collisionSettings.oneWayPlatforms);

			float hitPosition = groundHit.point.y;
			float myFeetPosition = capsuleCollider.Bottom().y;
			float neededPushUp = hitPosition - myFeetPosition;
			
			// If the raycast hit a one-way platform, we want to ignore it unless we're falling onto it
			if (isOneWayPlatform)
				if (RelativeVelocity.y > .1f || neededPushUp < 0) return;
			
			float yTranslation = Mathf.Clamp(_thisFrameTranslation.y, neededPushUp, 99);
			penetratingGround = neededPushUp < 0;
			_thisFrameTranslation = new Vector2(_thisFrameTranslation.x, yTranslation);
		}

		if (groundImOn == null && groundHit.collider != null && _noGroundingTime <= 0)
		{
			groundImOn = groundHit.collider;
			OnGrounded();
		}
			
		else if (groundImOn != null && groundHit.collider == null)
		{
			groundImOn = null;
			OnUngrounded();
		}
	}

	void ProcessGroundNormal()
	{
		var groundHits = raycastSettings.normalProbeCasting.RaycastDirectionAll(
			-transform.up, 
			raycastSettings.normalProbeCasting.CastingLeft(capsuleCollider), 
			raycastSettings.normalProbeCasting.CastingRight(capsuleCollider), 
			capsuleCollider.size.y / 2, 
			collisionSettings.terrain | collisionSettings.oneWayPlatforms, 
			new List<Collider2D>{capsuleCollider});

		Vector2 avgNormal = AverageNormalOfHits(groundHits);
		Debug.DrawRay(capsuleCollider.Bottom(), avgNormal, Color.cyan, .2f);
		
		if (_groundHitNormal.normalized != avgNormal.normalized) 
			onNormalChange?.Invoke(avgNormal.normalized);

		_groundHitNormal = avgNormal;
		groundRotation = Arachnid.Math.AngleFromVector2(avgNormal, -90);
	}

	Vector2 AverageNormalOfHits(List<RaycastHit2D> hits)
	{
		Vector2 averageNormal = Vector2.up;

		if (hits.Count > 0)
		{
			averageNormal = Vector2.zero;
			foreach (var hit in hits) averageNormal += hit.normal;
			averageNormal = new Vector2(averageNormal.x / hits.Count, averageNormal.y / hits.Count);
		}

		return averageNormal;
	}
	
	void ProcessRoofCollision()
	{		
		var roofHit = raycastSettings.verticalCollisions.RaycastDirection(transform.up, 
			raycastSettings.verticalCollisions.CastingLeft(ColliderForRoofCollision), 
			raycastSettings.verticalCollisions.CastingRight(ColliderForRoofCollision), 
			ColliderForRoofCollision.size.y / 2 + Mathf.Abs(RelativeVelocity.y * Time.deltaTime), 
			collisionSettings.terrain, ColliderForRoofCollision);

		if (!roofHit)return;
		if (!roofHit.collider) return;

		// clamp y velocity
		float yVel = Mathf.Clamp(velocity.y, -9999, 0);
		velocity = new Vector2(velocity.x, yVel);
		
		float distToCollider = roofHit.distance - capsuleCollider.size.y / 2;
		float yTranslation = Mathf.Clamp(_thisFrameTranslation.y, -99, distToCollider);
		_thisFrameTranslation = new Vector2(_thisFrameTranslation.x, yTranslation);
	}

	List<Collider2D> AllMyColliders()
	{
		List<Collider2D> colliders = new List<Collider2D>();
		if (stackable)
		{
			foreach (var stackItem in stackable.GetFullStack())
			{
				colliders.Add(stackItem.MyGameObject().GetComponent<Collider2D>());
			}

			return colliders;
		}
		
		colliders.Add(capsuleCollider);
		return colliders;
	}

	public void ProcessHorizontalCollision()
	{
		if (!raycastSettings.horizontalCollisions.enabled) return;

		var settings = raycastSettings.horizontalCollisions;
		float colliderWidth = capsuleCollider.size.x / 2;
		
		var leftWallHit = settings.RaycastDirection(Vector2.left, 
			settings.CastingTop(capsuleCollider) , 
			settings.CastingBottom(capsuleCollider),
			colliderWidth, 
			collisionSettings.walls | collisionSettings.terrain, AllMyColliders());
			
		var rightWallHit = settings.RaycastDirection(Vector2.right, 
			settings.CastingTop(capsuleCollider) , 
			settings.CastingBottom(capsuleCollider),
			colliderWidth, 
			collisionSettings.walls | collisionSettings.terrain, AllMyColliders());

		// If the left side has no hits, assume it's the right side.
		int direction;
		RaycastHit2D finalWallHit;
		
		if (leftWallHit.collider && !rightWallHit.collider)
		{
			direction = -1;
			finalWallHit = leftWallHit;
		}
		else if (!leftWallHit.collider && rightWallHit.collider)
		{
			direction = 1;
			finalWallHit = rightWallHit;
		}
		else 
		{
			wallImAgainst = null;
			return;
		}

		if (!wallImAgainst) OnWallHit(finalWallHit);
		wallImAgainst = finalWallHit.collider;
		
		if (wallImAgainst)
			velocity = new Vector2(0, velocity.y);
		
		float xTranslation = 0;
		float distToWall = finalWallHit.distance - capsuleCollider.size.x/2;
						
		// If dist to wall is less than 0, we assume the collider is penetrating the wall
		if (distToWall < 0)
			xTranslation = distToWall * direction;
		else
			xTranslation = Mathf.Clamp(xTranslation, -distToWall, distToWall);
		
		transform.Translate(new Vector2(xTranslation, 0));
	}

	bool LayerIsInLayerMask(int layer, LayerMask layerMask)
	{
		return layerMask == (layerMask | (1 << layer));
	}

	public void UnGround()
	{
		groundImOn = null;
		OnUngrounded();
		groundRotation = 0;
	}



	/// <summary>
	/// Casts ahead in the direction of velocity to check if there's any sharp falloffs (i.e. cliffs) in the ground
	/// </summary>
	void CheckForCliffs()
	{
		if (!Grounded) return;
		Vector2 startCastPoint = RelativeVelocity.x > 0 ? 
			raycastSettings.verticalCollisions.CastingRight(capsuleCollider) : raycastSettings.verticalCollisions.CastingLeft(capsuleCollider);
		float rayLength = capsuleCollider.size.y / 2 + minimumCliffHeight;
		
		RaycastHit2D[] hits = new RaycastHit2D[5];
		Physics2D.RaycastNonAlloc(startCastPoint, -transform.up, hits, rayLength, collisionSettings.oneWayPlatforms | collisionSettings.terrain);

		foreach (var hit in hits)
		{
			if (hit.collider != null)
			{
				_atEdge = false;
				return;
			}
		}

		if (_atEdge) return;
		_atEdge = true;
		OnEdgeReached();
	}


	void OnWallHit(RaycastHit2D hit)
	{
		onWallHit?.Invoke(hit);
		velocity = new Vector2(0, velocity.y);
	}
 
	void OnGrounded()
	{	
		float yVel = Mathf.Clamp(RelativeVelocity.y, 0, 9999);
		RelativeVelocity = new Vector2(RelativeVelocity.x, yVel);
		onGrounded?.Invoke(groundImOn.gameObject);
	}

	void OnUngrounded()
	{
		groundRotation = 0;
		penetratingGround = false;
		onUngrounded?.Invoke();
	}

	void OnEdgeReached()
	{
		onEdgeReached?.Invoke();
	}

	/// <summary>
	/// Changes direction but keeps the intensity the same.
	/// </summary>
	/// <param name="newDirection">New direction of gravity. It gets normalized so vector scale doesn't matter.</param>
	public void SetGravityDirection(Vector2 newDirection)
	{
		float gravMagnitude = gravity.magnitude;
		gravity = gravMagnitude * newDirection.normalized;
	}
}