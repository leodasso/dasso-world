using System.Collections;
using System.Collections.Generic;
using Arachnid;
using UnityEngine;

public class Hazard : MonoBehaviour
{
	public IntReference damage;
	public FloatReference throwbackForce;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (!enabled) return;
		Damageable d = other.GetComponent<Damageable>();
		if (!d) return;
		Vector2 damageVector = transform.position - other.transform.position;
		DoDamage(d, other.transform.position, -damageVector.normalized);
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (!enabled) return;
		Damageable d = other.gameObject.GetComponent<Damageable>();
		if (!d) return;
		DoDamage(d, other.contacts[0].point, other.contacts[0].normal);
	}
	
	

	void DoDamage(Damageable other, Vector2 damagePosition, Vector2 damageVector)
	{
		other.Damage(damage.Value, damagePosition);
		
		Vector2 damageVelocity = damageVector * throwbackForce.Value;
		Debug.DrawRay(damagePosition, damageVelocity, Color.red, 5);
		
		PlatformBody body = other.GetComponent<PlatformBody>();
		Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
		
		if (body) body.velocity += damageVelocity;
		else if (rb) rb.velocity += damageVelocity;
	}
}
