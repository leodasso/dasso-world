using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "HacknodE/Raycast Settings")]
public class RaycastSettings : ScriptableObject 
{
    [BoxGroup("Vertical Raycasts"), HideLabel, GUIColor(1, 1, 0)]
    public RaycastGroup verticalCollisions;
    [BoxGroup("Horizontal Raycasts"), HideLabel, GUIColor(1, 0, 1)]
    public RaycastGroup horizontalCollisions;
    [BoxGroup("Normal Probes"), HideLabel, GUIColor(0, 0, 1)]
    public RaycastGroup normalProbeCasting;
    [BoxGroup("Ground Snapping"), HideLabel, GUIColor(0, 1, 0)]
    public RaycastGroup groundSnapCasting;
}
