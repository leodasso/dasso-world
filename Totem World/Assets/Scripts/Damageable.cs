using System.Collections;
using System.Collections.Generic;
using Arachnid;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class Damageable : MonoBehaviour
{
	public IntReference hitPoints;
	[Tooltip("How long will I be invulnerable after I've been damaged")]
	public FloatReference invulnerableTime;
	public int remainingHitPoints;

	[AssetsOnly]
	public GameObject damagedEffect;

	[AssetsOnly]
	public GameObject destroyedModel;

	[DrawWithUnity]
	public UnityEvent onDamaged;
	[DrawWithUnity]
	public UnityEvent onKilled;

	float _invulnerableTimer;

	
	void Start()
	{
		remainingHitPoints = hitPoints.Value;
	}

	void Update()
	{
		if (_invulnerableTimer > 0) _invulnerableTimer -= Time.deltaTime;
	}

	public void Damage(int damage, Vector2 damagePos)
	{
		if (_invulnerableTimer > 0) return;
		_invulnerableTimer = invulnerableTime.Value;
		
		if (damagedEffect)
			Destroy(Instantiate(damagedEffect, damagePos, Quaternion.identity), 5);
		
		onDamaged.Invoke();

		remainingHitPoints -= damage;
		remainingHitPoints = Mathf.Clamp(remainingHitPoints, 0, hitPoints.Value);
		if (remainingHitPoints < 1)
			Die();
	}

	public void Die()
	{
		onKilled.Invoke();
		if (destroyedModel)
			Instantiate(destroyedModel, transform.position, transform.rotation);
		Destroy(gameObject);
	}
}
