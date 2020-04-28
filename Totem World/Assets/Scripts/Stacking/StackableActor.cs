using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StackableActor : PlatformBodyActor, IStackable
{

    public Vector2 topOffset = Vector2.up;
    [Tooltip("Where do I reside within the stack?")]
    public StackPosition stackPosition;
    
    public IStackable aboveMe;
    public IStackable belowMe;

    public UnityEvent OnStackAbove;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Top(), .05f );
    }

    protected override void Update()
    {
        base.Update();


    }

    void LateUpdate()
    {
        if (belowMe != null)
        {
            transform.position = belowMe.Top();
        }
    }

    protected override void OnGrounded(GameObject newGround)
    {
        base.OnGrounded(newGround);

        IStackable stackable = newGround.GetComponent<IStackable>();
        if (stackable != null && stackable.StackingAllowed())
            StackOnTo(stackable);
    }

    
    void StackOnTo(IStackable under)
    {
        belowMe = under;
        under.GetStacked(this);
        RecalculateStackPosition();
        
        OnStackAbove.Invoke();
    }

    
    public bool StackingAllowed()
    {
        // Allow something to stack on top of me only if I don't already have something on top.
        return aboveMe == null;
    }

    public void GetStacked(IStackable newHat)
    {
        aboveMe = newHat;
        RecalculateStackPosition();
    }
    
    void RecalculateStackPosition()
    {
        stackPosition = GetStackPosition();
    }

    StackPosition GetStackPosition()
    {
        if (belowMe == null && aboveMe == null) return StackPosition.Single;
        if (belowMe != null && aboveMe == null) return StackPosition.Top;
        if (belowMe == null && aboveMe != null) return StackPosition.Bottom;
        return StackPosition.Middle;
    }

    public StackPosition MyStackPosition()
    {
        return stackPosition;
    }

    public List<IStackable> GetAllAbove()
    {
        if (aboveMe == null) return new List<IStackable>();
        var aboveStackables = aboveMe.GetAllAbove();
        aboveStackables.Add(aboveMe);
        return aboveStackables;
    }

    public List<IStackable> GetAllBelow()
    {
        if (belowMe == null) return new List<IStackable>();
        var belowStackables = belowMe.GetAllAbove();
        belowStackables.Add(belowMe);
        return belowStackables;
    }

    public List<IStackable> GetFullStack()
    {
        List<IStackable> fullList = new List<IStackable>();
        fullList.AddRange(GetAllAbove());
        fullList.Add(this);
        fullList.AddRange(GetAllBelow());
        return fullList;
    }

    public Vector2 Top()
    {
        return (Vector2)transform.position + topOffset;
    }

    public GameObject MyGameObject()
    {
        return gameObject;
    }
}
