using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Object = UnityEngine.Object;

namespace NodeEngine
{
	public class NodeConnection : SerializedMonoBehaviour
	{
		Node originNode
		{
			get { return _originNode; }
			set
			{
				if (value == null)
				{
					_originNode = null;
					_origin = null;
					return;
				}

				_originNode = value;
				_origin = _originNode.gameObject;
			}
		}

		public GameObject packetEffectPrefab;

		[ShowIf("ShowIndexSlider"), MinValue(0), OnValueChanged("ProcessIndex"), CustomValueDrawer("IndexDrawer")]
		public int eventIndex;

		[DisplayAsString, HideLabel, Indent()] public string eventDescription = "event description";

		Spline _spline;

		[OdinSerialize, HideInInspector] Node _originNode;

		[ShowInInspector, ReadOnly, HorizontalGroup("endPoints", LabelWidth = 50), OdinSerialize]
		GameObject _origin;

		[ShowInInspector, ReadOnly, HorizontalGroup("endPoints"), OdinSerialize]
		GameObject _destination;

		static GameObject _splinePrefab;

		GameObject SplinePrefab()
		{
			if (_splinePrefab) return _splinePrefab;
			_splinePrefab = Resources.Load<GameObject>("prefabs/default node spline");
			return _splinePrefab;
		}

		void OnDrawGizmos()
		{
			if (!_destination || !_origin) return;
			Gizmos.color = Color.green;
			Gizmos.DrawLine(_origin.transform.position, _destination.transform.position);
		}

		public void Initialize(Node node, int index)
		{
			originNode = node;
			eventIndex = index;
			ProcessIndex();
			SetPosition();
			CreateVisuals();
		}

		void CreateVisuals()
		{
			_spline = Instantiate(SplinePrefab()).GetComponent<Spline>();
			if (!_spline) return;
			_spline.start = _origin.transform;
			_spline.end = _destination.transform;
			_spline.Generate();
		}

		public void SendPacketEffect(float time)
		{
			if (!_spline) return;
			GameObject _packet = Instantiate(packetEffectPrefab);
			_packet.transform.position = _spline.PointOnSpline(0);
			StartCoroutine(AnimateSendNodeEffect(time, _packet));
		}

		IEnumerator AnimateSendNodeEffect(float time, GameObject packetEffectInstance)
		{
			float progress = 0;
			while (progress < 1)
			{
				progress += Time.deltaTime / time;
				packetEffectInstance.transform.position = _spline.PointOnSpline(progress);
				yield return null;
			}

			Destroy(packetEffectInstance, 3);
		}

		[Button]
		void Refresh()
		{
			eventIndex = Mathf.Clamp(eventIndex, 0, MaxIndexValue());
			ProcessIndex();
		}

		bool IndexIsValid(int index)
		{
			if (!originNode) return false;
			if (index < 0) return false;
			return index <= MaxIndexValue();
		}

		int MaxIndexValue()
		{
			if (!originNode) return 0;
			return originNode.nodeEvent.GetPersistentEventCount() - 1;
		}

		/// <summary>
		/// Gets all the relevant information from the event at the given index and stores / displays it.
		/// </summary>
		void ProcessIndex()
		{
			if (!IndexIsValid(eventIndex)) return;
			Object destinationObject = originNode.nodeEvent.GetPersistentTarget(eventIndex);
			if (destinationObject == null)
			{
				Debug.LogWarning("No target object is defined for index " + eventIndex + " of node " + originNode.name);
				eventDescription = "invalid";
				return;
			}

			GameObject go = destinationObject as GameObject;
			if (go)
			{
				SetEventValues(go);
				return;
			}

			Component component = destinationObject as Component;
			if (component == null)
			{
				Debug.LogWarning("The target object for index " + eventIndex + " + of node " + originNode.name +
				                 " is not a component or game object.");
				eventDescription = "invalid";
				return;
			}

			SetEventValues(component.gameObject);
		}

		void SetPosition()
		{
			if (!_destination || !_origin) return;
			transform.position = (_destination.transform.position + _origin.transform.position) / 2;
		}

		void SetEventValues(GameObject newDestination)
		{
			eventDescription = originNode.nodeEvent.GetPersistentTarget(eventIndex) + ": " +
			                   originNode.nodeEvent.GetPersistentMethodName(eventIndex);
			_destination = newDestination;
		}

		bool ShowIndexSlider()
		{
			return MaxIndexValue() > 0;
		}


#if UNITY_EDITOR
		// custom drawer for the index slider
		int IndexDrawer(int value, GUIContent label)
		{
			return UnityEditor.EditorGUILayout.IntSlider(label, value, 0, MaxIndexValue());
		}
#endif
	}
}