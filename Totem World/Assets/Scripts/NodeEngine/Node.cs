using System.Collections;
using System.Collections.Generic;
using Arachnid;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace NodeEngine
{

	public class Node : MonoBehaviour, IPokeable
	{
		[DrawWithUnity] public UnityEvent nodeEvent;

		[Tooltip("The delay between event being triggered and actually invoked. represents the travel time along connections.")]
		public FloatReference signalTravelTime;

		[Tooltip("The delay between the call and being triggered.")]
		public FloatReference eventTriggerDelay;

		public GameObject nodeCalledEffect;

		[ToggleLeft] 
		public bool useDefaultConnectionPrefab = true;

		[ToggleLeft] 
		public bool createConnectionsOnStart = true;

		[HideIf("useDefaultConnectionPrefab"), AssetList(Path = "Prefabs/Node Connections/")]
		public NodeConnection connectionPrefab;

		[ReadOnly, Indent(), PropertyOrder(50)] 
		public List<NodeConnection> connections = new List<NodeConnection>();

		static NodeConnection _defaultConnectionPrefab;

		void OnDrawGizmos()
		{
			for (int i = 0; i < nodeEvent.GetPersistentEventCount(); i++)
			{
				GameObject target = GetEventTargetGameObject(i);
				if (target) Gizmos.DrawLine(transform.position, target.transform.position);
			}
		}

		void Start()
		{
			if (createConnectionsOnStart) CreateConnections();
		}

		GameObject GetEventTargetGameObject(int index)
		{
			if (nodeEvent.GetPersistentTarget(index) == null) return null;

			Object targetObject = nodeEvent.GetPersistentTarget(index);
			GameObject go = targetObject as GameObject;
			if (go) return go;
			
			Component component = targetObject as Component;
			if (component) return component.gameObject;
			return null;
		}

		[Button(ButtonSizes.Medium), GUIColor(0, .7f, 1), PropertyOrder(-99), EnableIf("AppIsPlaying")]
		public void CallEvent()
		{
			Invoke(nameof(TriggerEvent), eventTriggerDelay.Value);
		}

		void TriggerEvent()
		{
			Invoke(nameof(InvokeEvent), signalTravelTime.Value);
			foreach (var c in connections)
				c.SendPacketEffect(signalTravelTime.Value);

			if (nodeCalledEffect)
			{
				Destroy(Instantiate(nodeCalledEffect, transform.position, transform.rotation), 5);
			}
		}
		
		

		void InvokeEvent()
		{
			nodeEvent.Invoke();
		}

		[Button, PropertyOrder(49)]
		void CreateConnections()
		{
			DestroyConnectionObjects();
			connections.Clear();

			for (int i = 0; i < nodeEvent.GetPersistentEventCount(); i++)
			{
				CreateConnection(i);
			}
		}

		void CreateConnection(int index)
		{
			if (!IndexIsValidForConnection(index)) return;

			NodeConnection newConnection = Instantiate(ConnectionPrefab(), transform);
			newConnection.Initialize(this, index);
			connections.Add(newConnection);
		}

		bool IndexIsValidForConnection(int index)
		{
			if (index < 0) return false;
			if (index >= nodeEvent.GetPersistentEventCount()) return false;
			return nodeEvent.GetPersistentTarget(index) != null;
		}

		NodeConnection ConnectionPrefab()
		{
			if (useDefaultConnectionPrefab) return DefaultConnectionPrefab();
			return connectionPrefab;
		}

		NodeConnection DefaultConnectionPrefab()
		{
			if (_defaultConnectionPrefab) return _defaultConnectionPrefab;
			_defaultConnectionPrefab = Resources.Load<NodeConnection>("prefabs/default connection");
			return _defaultConnectionPrefab;
		}

		void DestroyConnectionObjects()
		{
			if (connections.Count <= 0) return;
			for (int i = connections.Count - 1; i >= 0; i--)
			{
				if (connections[i] == null) continue;
				if (Application.isPlaying)
					Destroy(connections[i].gameObject);
				else
					DestroyImmediate(connections[i].gameObject);
			}
		}

		bool AppIsPlaying()
		{
			return Application.isPlaying;
		}

		public void OnPoke(GameObject poker, Vector2 pokeDirection)
		{
			CallEvent();
		}
	}
}