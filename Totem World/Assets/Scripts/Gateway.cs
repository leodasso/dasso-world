using UnityEngine;
using Sirenix.OdinInspector;

public class Gateway : MonoBehaviour
{
	public Stage destination;
	[AssetList(Path = "Prefabs/Transitions"), OnValueChanged("GetTransitionTime")]
	public Transition transition;

	[ReadOnly]
	public float transitionTime;

	void GetTransitionTime()
	{
		if (transition == null)
		{
			transitionTime = 0;
			return;
		}
		transitionTime = transition.duration;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject == GameMaster.HackInstance)
		{
			PlatformBody _hackBody = other.gameObject.GetComponent<PlatformBody>();
			if (!_hackBody.enabled) return;
			_hackBody.enabled = false;
			GameMaster.LoadStage(destination, transitionTime);

			if (transition)
				Instantiate(transition);
		}
	}
}
