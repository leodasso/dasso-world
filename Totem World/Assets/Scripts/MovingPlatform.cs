using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

	public Vector2 velocity;
	List<PlatformBody> _affectedBodies = new List<PlatformBody>();

	Vector2 _prevPosition;
	
	
	// Use this for initialization
	void Start ()
	{
		_prevPosition = transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate()
	{
		velocity = (Vector2)transform.position - _prevPosition;
		_prevPosition = transform.position;

		foreach (var body in _affectedBodies)
		{
			if (body.Grounded)
			{
				body.inheritedTranslation += velocity;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		PlatformBody otherBody = other.GetComponent<PlatformBody>();
		if (!otherBody) return;
		if (_affectedBodies.Contains(otherBody)) return;
		_affectedBodies.Add(otherBody);
	}

	void OnTriggerExit2D(Collider2D other)
	{
		PlatformBody otherBody = other.GetComponent<PlatformBody>();
		if (!otherBody) return;
		_affectedBodies.Remove(otherBody);
		otherBody.velocity += velocity / Time.deltaTime;
	}
}
