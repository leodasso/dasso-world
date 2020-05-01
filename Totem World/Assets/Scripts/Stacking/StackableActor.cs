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

    public UnityEvent OnStackChanged;

    static float unitsPerPixel = 0.1f;

    Vector2 _prevPosition;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Top(), .05f );
    }

    protected override void Start()
    {
        base.Start();
        _prevPosition = transform.position;
    }

    void LateUpdate()
    {
        // The bottom of the stack always starts the update chain. Then the order of the update 
        // is from bottom to top. Having them update in order is important to prevent jittery movement.
        if (stackPosition == StackPosition.Bottom) 
            StackUpdate(belowMe);

        _prevPosition = transform.position;
        
        // Horizontal collision needs to happen last, so it's called here
        if (_platformBody)
            _platformBody.ProcessHorizontalCollision();
    }

    public Vector2 DeltaPosition()
    {
        return (Vector2)transform.position - _prevPosition;
    }

    public void StackUpdate(IStackable blockBelowMe)
    {
        if (blockBelowMe != null)
            transform.position = blockBelowMe.Top() + StackOffset();
        
        if (aboveMe != null) 
            aboveMe.StackUpdate(this);
    }

    public Vector2 StackOffset()
    {
        if (belowMe == null) return Vector2.zero;
        if (belowMe.DeltaPosition().x < -.05f) return Vector2.right * unitsPerPixel;
        if (belowMe.DeltaPosition().x > .05f) return Vector2.left * unitsPerPixel;
        return Vector2.zero;
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
        
        StackChanged();
    }

    public void StackChanged()
    {
        if (aboveMe != null) 
            aboveMe.StackChanged();
        
        OnStackChanged.Invoke();
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
        var belowStackables = belowMe.GetAllBelow();
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

    public IStackable BottomOfStack()
    {
        var stackBelow = GetAllBelow();
        if (stackBelow.Count < 1) return this;
        return stackBelow[0];
    }


    public IStackable TopOfStack()
    {
        var stackAbove = GetAllAbove();
        if (stackAbove.Count < 1) return this;
        return stackAbove[0];
    }

    public GameObject MyGameObject()
    {
        return gameObject;
    }
}
