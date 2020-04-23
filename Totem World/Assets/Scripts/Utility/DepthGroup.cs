using UnityEngine;
using Sirenix.OdinInspector;

public class DepthGroup : MonoBehaviour
{
	[OnValueChanged("SetDepth")]
	public float depth;

	void SetDepth()
	{
		foreach (Transform child in transform)
		{
			child.transform.position = new Vector3(child.transform.position.x, child.transform.position.y, depth);
		}
	}
}
