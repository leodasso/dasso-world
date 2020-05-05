using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StackableActor : PlatformBodyActor, IStackable
{

    public Vector2 popVelocity;
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
    }

    public Vector2 DeltaPosition()
    {
        return (Vector2)transform.position - _prevPosition;
    }

    public void StackUpdate(IStackable blockBelowMe)
    {
        // at this point, the position will still be left over from the previous correct collision check
        if (blockBelowMe != null)
        {
            Vector2 gotoPos = blockBelowMe.Top() + StackOffset();
            
            // Make sure we wont pop through other colliders
            float dist = Mathf.Abs(gotoPos.x - transform.position.x);
            if (dist >= _platformBody._horizontalCollisionRayLength - .1f && _platformBody.IsAgainstWall())
            {
                PopOffStack();
                return;
            }
            
            transform.position = gotoPos;
        }
        
        if (aboveMe != null) 
            aboveMe.StackUpdate(this);
        
        // Horizontal collision needs to happen last
        if (_platformBody)
            _platformBody.ProcessHorizontalCollision();
    }

    void PopOffStack()
    {
        Debug.Log("POP!");
        belowMe.BreakAbove();
        BreakBelow();
        
        if (_platformBody)
        {
            _platformBody.AddRelativeVelocity(popVelocity);
        }
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
        Debug.Log(name + " is stacking on to " + under.MyGameObject().name);
        
        belowMe = under;
        under.GetStacked(this);
        RecalculateStackPosition();
        
        StackChanged();
    }

    /// <summary>
    /// Invokes the onStackChanged method. Calls StackChanged on all stackables above me
    /// </summary>
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

    /// <summary>
    /// I am now the top of a new stack, and whatever is above me snaps off and is its own stack now.
    /// </summary>
    public void BreakAbove()
    {
        aboveMe = null;
        RecalculateStackPosition();
        
        // stack changed always propogates from the bottom
        BottomOfStack().StackChanged();
    }

    /// <summary>
    /// I am not the bottom of a new stack, and whatever is below me is its own stack now.
    /// </summary>
    public void BreakBelow()
    {
        belowMe = null;
        RecalculateStackPosition();
        StackChanged();
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
