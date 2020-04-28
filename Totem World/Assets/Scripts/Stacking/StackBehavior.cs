using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Any behavior which is influenced by it's position in the stack. For example, a walker won't behave if it's
/// at the bottom of the stack. There may be multiple of these components on a single stackable object.
/// </summary>
public class StackBehavior : MonoBehaviour
{
    [BoxGroup("Behavior Allowed When"), ToggleLeft] public bool top;
    [BoxGroup("Behavior Allowed When"), ToggleLeft] public bool middle;
    [BoxGroup("Behavior Allowed When"), ToggleLeft] public bool bottom = true;
    [BoxGroup("Behavior Allowed When"), ToggleLeft] public bool single = true;

    // There is only ever 1 stackable component on an object. All the stack behaviors will reference that 1 stackable.
    IStackable _stackable;

    protected virtual void Awake()
    {
        _stackable = GetComponent<IStackable>();
        if (_stackable == null)
        {
            Debug.LogError("No stackable component could be found on " + name);
        }
    }

    /// <summary>
    /// Returns whether or not this behavior can act based on its position in the stack.
    /// </summary>
    protected bool BehaviorAllowedByStack()
    {
        if (_stackable == null) return false;
        var stackPos = _stackable.MyStackPosition();
        if (stackPos == StackPosition.Bottom && bottom) return true;
        if (stackPos == StackPosition.Middle && middle) return true;
        if (stackPos == StackPosition.Top && top) return true;
        if (stackPos == StackPosition.Single && single) return true;
        return false;
    }
}
