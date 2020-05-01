using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CamController : MonoBehaviour
{
    public Vector2 cushion;
    public Vector2 offset;
    public float zOffset = -10;
    public Transform target;
    public StackableActor targetStack;
    public Vector2 difference;

    float _xLockPos;
    float _yLockPos;

    Transform _finalTarget
    {
        get
        {
            if (!target) return null;
            if (!targetStack) return target;
            return targetStack.BottomOfStack().MyGameObject().transform;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
    }
 
    // Update is called once per frame
    void LateUpdate()
    {
        if (!target) return;
        Vector2 targetPos = (Vector2)_finalTarget.position + offset;
        Vector2 myFlatPos = transform.position;

        float xPos = myFlatPos.x;
        float yPos = myFlatPos.y;

        difference = targetPos - myFlatPos;

        if (difference.x < -cushion.x)
            xPos = targetPos.x + cushion.x;
        else if (difference.x > cushion.x)
            xPos = targetPos.x - cushion.x;
        

        if (difference.y < -cushion.y)
            yPos = targetPos.y + cushion.y;
        else if (difference.y > cushion.y)
            yPos = targetPos.y - cushion.y;
        
        transform.position = new Vector3(xPos, yPos, zOffset);
    }

    bool WithinCushionX => Mathf.Abs(difference.x) < cushion.x;
}
