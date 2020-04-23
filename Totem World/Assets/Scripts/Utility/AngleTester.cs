using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class AngleTester : MonoBehaviour
{

	[OnValueChanged("UpdateAngle")]
	public Vector2 coords;
	[ReadOnly]
	public float angle;

	public float angleOffset = -90;

	void OnDrawGizmos()
	{
		Gizmos.DrawLine(transform.position, transform.position + (Vector3)coords);
	}

	void UpdateAngle()
	{
		angle = Arachnid.Math.AngleFromVector2(coords, angleOffset);
	}
}
