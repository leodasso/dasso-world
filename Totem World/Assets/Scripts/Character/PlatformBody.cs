using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arachnid;
using UnityEngine;
using Sirenix.OdinInspector;


public class PlatformBody : MonoBehaviour
{
	public enum FacingDirection
	{
		Left, Right
	}
	
	public CapsuleCollider2D capsuleCollider;
	public GameObject graphics;
	
	[Space]
	public RaycastSettings raycastSettings;
	public PlatformBodyCollisionSettings collisionSettings;
	[Tooltip("Resistance to movement while in the air")]
	public CurveObject drag;
	
	[Tooltip("Resistance to movement on the ground")]
	public CurveObject friction;
	
	public float groundPenetration;
	
	[Tooltip("There's a raycast to check for upcoming cliffs in the direction of momement to call event OnEdgeReached. " +
	         "This value determines how far the drop has to be before it's determined a cliff")]
	public float minimumCliffHeight = .5f;
	
	[ToggleLeft, LabelText("Gravity"), HorizontalGroup("Gravity")]
	public bool useGravity;
	
	[ShowIf("useGravity"), HorizontalGroup("Gravity"), HideLabel]
	public Vector2 gravity;
	
	[ToggleLeft, BoxGroup("Rotation")]
	public bool limitRotation = true;
	
	[HorizontalGroup("Rotation/rot", Title = "Rotation"), LabelText("Max"), LabelWidth(80), ShowIf("limitRotation")]
	public float maxRotationAngle = 30;

	[HorizontalGroup("Rotation/rot"), ReadOnly, LabelText("Current"), LabelWidth(80)]
	public float groundRotation;

	[BoxGroup("Rotation")]
	public float rotationSpeed;
	
	[ReadOnly]
	public Vector2 velocity;

	[Tooltip("The movement gained from what I'm standing on. Not affected by drag or friction"), ReadOnly]
	public Vector2 inheritedTranslation;

	[EnumToggleButtons, ReadOnly]
	public FacingDirection facingDirection = FacingDirection.Right;

	[ ReadOnly]
	public Collider2D groundImOn;
	
	[ ReadOnly]
	public Collider2D wallImAgainst;
	
	Vector2 _thisFrameTranslation;

	public bool AbilitiesDisabled => _abilityDisabledTimer > 0;
	public bool Grounded => groundImOn != null;
	public float CurrentFriction => friction.ValueFor(RelativeVelocity.x);
	public Vector2 GroundNormal => _groundHitNormal;

	[ToggleLeft, ReadOnly]
	public bool penetratingGround;

	public Action onGrounded;
	public Action onUngrounded;
	public Action<RaycastHit2D> onWallHit;
	/// <summary>
	/// Called when the ground normal changes. Parameter is the new normal
	/// </summary>
	public Action<Vector2> onNormalChange;
	public Action onEdgeReached;

	public Vector2 RelativeVelocity
	{
		get {return transform.InverseTransformDirection(velocity);}
		private set {velocity = transform.TransformDirection(value);}
	}

	Vector2 RelativeThisFrameTranslation
	{
		get { return transform.InverseTransformDirection(_thisFrameTranslation); }
		set { _thisFrameTranslation = transform.TransformDirection(value); }
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
		if (graphics) _initGraphicScale = graphics.transform.localScale;
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
		ProcessGravity();
		ProcessDrag();
		ProcessFriction();

		_thisFrameTranslation = velocity * Time.deltaTime + inheritedTranslation;
		inheritedTranslation = Vector2.zero;
		
		ProcessGroundSnapping();
		ProcessHorizontalCollision();
		ProcessRoofCollision();
		ProcessGroundCollision();
		CheckForCliffs();
		ProcessGroundNormal();

		transform.Translate(_thisFrameTranslation, Space.World);

		float rotation = groundRotation;
		if (limitRotation)
			rotation = Mathf.Clamp(groundRotation, -maxRotationAngle, maxRotationAngle);
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, rotation), Time.deltaTime * rotationSpeed);
	}

	public void DisableAbilitiesForTime(float time)
	{
		_abilityDisabledTimer = time;
	}

	public void SetFacingDirection(FacingDirection newDirection)
	{
		if (newDirection == facingDirection) return;
		if (!graphics) return;
		facingDirection = newDirection;

		Vector2 scale = _initGraphicScale;
		if (newDirection == FacingDirection.Left) scale = new Vector2(-_initGraphicScale.x, _initGraphicScale.y);

		graphics.transform.localScale = scale;
	}

	public void AddRelativeVelocity(Vector2 newVelocity)
	{
		Vector2 finalVel = transform.TransformDirection(newVelocity);
		velocity += finalVel;
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
		if (!drag || Grounded) return;
		velocity = Vector2.Lerp(velocity, Vector2.zero, drag.ValueFor(velocity.magnitude) * Time.deltaTime);
	}

	void ProcessFriction()
	{
		if (!Grounded || !friction) return;
		
		velocity = Vector2.Lerp(velocity, Vector2.zero, CurrentFriction * Time.deltaTime);
	}

	void ProcessGravity()
	{
		if (!useGravity) return;
		if (Grounded) return;

		velocity += Time.deltaTime * gravity;
	}

	void ProcessGroundSnapping()
	{
		if (!raycastSettings.groundSnapCasting.enabled || !Grounded) return;

		var groundSnapHit = raycastSettings.groundSnapCasting.RaycastDirectionAll(-transform.up, 
			raycastSettings.groundSnapCasting.CastingLeft(capsuleCollider), 
			raycastSettings.groundSnapCasting.CastingRight(capsuleCollider),
			capsuleCollider.size.y / 2 + Mathf.Abs(RelativeVelocity.y * Time.deltaTime),
			collisionSettings.terrain, capsuleCollider);

		if (groundSnapHit.Count < 1) return;

		// order by distance
		groundSnapHit = groundSnapHit.OrderBy(x => x.distance).ToList();
		
		// choose the hit with the farthest distance - so we dont cause penetration
		var farthestHit = groundSnapHit.Last();
		
		float distToCollider = farthestHit.distance - capsuleCollider.size.y / 2;

		if (distToCollider < 0) return;

		Vector2 vectorToHitPoint = -AverageNormalOfHits(groundSnapHit).normalized * distToCollider;
		_thisFrameTranslation += vectorToHitPoint;
	}

	void ProcessGroundCollision()
	{
		Vector2 castingLeft = raycastSettings.verticalCollisions.CastingLeft(capsuleCollider);
		Vector2 castingRight = raycastSettings.verticalCollisions.CastingRight(capsuleCollider);
		Vector2 startCasting = RelativeVelocity.x < 0 ? castingLeft: castingRight;
		Vector2 endCasting = RelativeVelocity.x < 0 ? castingRight : castingLeft;
		
		var groundHit = raycastSettings.verticalCollisions.RaycastDirection(
			-transform.up, startCasting, endCasting, 
			capsuleCollider.size.y / 2 + Mathf.Abs(RelativeVelocity.y * Time.deltaTime), 
			collisionSettings.terrain | collisionSettings.oneWayPlatforms, capsuleCollider);
		
		// If there's no raycast hit, check if we should be ungrounded, and return
		if (!groundHit)
		{
			if (groundImOn != null) OnUngrounded();
			groundImOn = null;
			return;
		}
		
		if (groundHit.collider)
		{
			bool isOneWayPlatform = LayerIsInLayerMask(groundHit.collider.gameObject.layer, collisionSettings.oneWayPlatforms);
			float distToCollider = groundHit.distance - capsuleCollider.size.y / 2;
			
			// If the raycast hit a one-way platform, we want to ignore it unless we're falling onto it
			if (isOneWayPlatform)
				if (RelativeVelocity.y > .1f || distToCollider < 0) return;
			
			float yTranslation = Mathf.Clamp(RelativeThisFrameTranslation.y, -distToCollider - groundPenetration, 99);
			penetratingGround = distToCollider < 0;
			RelativeThisFrameTranslation = new Vector2(RelativeThisFrameTranslation.x, yTranslation);
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
			collisionSettings.terrain | collisionSettings.oneWayPlatforms, capsuleCollider);

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
			raycastSettings.verticalCollisions.CastingLeft(capsuleCollider), 
			raycastSettings.verticalCollisions.CastingRight(capsuleCollider), 
			capsuleCollider.size.y / 2 + Mathf.Abs(RelativeVelocity.y * Time.deltaTime), 
			collisionSettings.terrain, capsuleCollider);
		
		if (!roofHit)return;
		if (!roofHit.collider) return;

		// clamp y velocity
		float yVel = Mathf.Clamp(velocity.y, -9999, 0);
		velocity = new Vector2(velocity.x, yVel);
		
		float distToCollider = roofHit.distance - capsuleCollider.size.y / 2;
		float yTranslation = Mathf.Clamp(RelativeThisFrameTranslation.y, -99, distToCollider);
		RelativeThisFrameTranslation = new Vector2(RelativeThisFrameTranslation.x, yTranslation);
	}


	void ProcessHorizontalCollision()
	{
		if (!raycastSettings.horizontalCollisions.enabled) return;
		Vector2 movementDir = transform.right;
		if (RelativeVelocity.x < 0) movementDir = -transform.right;
		
		var wallHit = raycastSettings.horizontalCollisions.RaycastDirection(movementDir, 
			raycastSettings.horizontalCollisions.CastingTop(capsuleCollider), 
			raycastSettings.horizontalCollisions.CastingBottom(capsuleCollider),
			(capsuleCollider.size.x / 2) + Mathf.Abs(RelativeVelocity.x) * Time.deltaTime, 
			collisionSettings.walls | collisionSettings.terrain, capsuleCollider);

		if (!wallHit.collider)
		{
			wallImAgainst = null;
			return;
		}

		if (!wallImAgainst) OnWallHit(wallHit);
		wallImAgainst = wallHit.collider;
		
		if (wallImAgainst)
			velocity = new Vector2(0, velocity.y);
		
		float xTranslation = 0;
		float distToWall = wallHit.distance - capsuleCollider.size.x / 2;

		float direction = 1;
		if (RelativeVelocity.x < 0) direction = -1;
		xTranslation = distToWall < 0 ?
			distToWall * direction : Mathf.Clamp(xTranslation, -distToWall, distToWall);
		
		RelativeThisFrameTranslation = new Vector2(xTranslation, RelativeThisFrameTranslation.y);
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
		onGrounded?.Invoke();
	}

	void OnUngrounded()
	{
		groundRotation = 0;
		penetratingGround = false;
		onUngrounded?.Invoke();
	}

	void OnEdgeReached()
	{
		Debug.Log("Edge reached!");
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