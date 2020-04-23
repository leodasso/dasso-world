using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlatformBodyCollisionSettings : ScriptableObject
{
    public LayerMask terrain;
    public LayerMask walls;
    public LayerMask oneWayPlatforms;
}
