using System.Collections;
using System.Collections.Generic;
using NodeEngine;
using UnityEngine;

public class NodeHitter : MonoBehaviour 
{

	void OnTriggerEnter2D(Collider2D other)
	{
		Node node = other.GetComponent<Node>();
		if (!node) return;
		node.CallEvent();
	}
}
