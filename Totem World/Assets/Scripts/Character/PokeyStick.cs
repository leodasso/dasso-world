using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokeyStick : MonoBehaviour
{
    public Vector2 direction;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.TransformVector(direction));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IPokeable otherPokeable = other.GetComponent<IPokeable>();
        otherPokeable?.OnPoke(gameObject, transform.TransformVector(direction));
    }
}
