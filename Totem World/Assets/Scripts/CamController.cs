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
    public Vector2 difference;

    [Tooltip("To prevent jittering when the character is near the edge boundaries. The amount of time the character " +
             "will be locked to a boundary.")]
    public float lockTime = .5f;

    float _lockTimerX;
    float _lockTimery;

    float _xLockPos;
    float _yLockPos;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!target) return;
        Vector2 targetPos = (Vector2)target.position + offset;
        Vector2 myFlatPos = transform.position;

        float xPos = myFlatPos.x;
        float yPos = myFlatPos.y;

        difference = targetPos - myFlatPos;

        if (difference.x < -cushion.x)
        {
            xPos = targetPos.x + cushion.x;
        }
        else if (difference.x > cushion.x)
        {
            xPos = targetPos.x - cushion.x;

        }

        if (WithinCushionX)
            if (_lockTimerX > 0)
                _lockTimerX -= Time.unscaledDeltaTime;
            else
                _lockTimerX = lockTime;
        

        if (difference.y < -cushion.y)
            yPos = targetPos.y + cushion.y;
        else if (difference.y > cushion.y)
            yPos = targetPos.y - cushion.y;
        
        transform.position = new Vector3(xPos, yPos, zOffset);
    }

    bool WithinCushionX => Mathf.Abs(difference.x) < cushion.x;
}
