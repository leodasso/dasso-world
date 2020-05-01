using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PixelPerfectMover : MonoBehaviour
{
    public int pixelsPerUnit = 24;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = PixelSnappedPosition(transform.position);
    }

    float PixelSnappedPosition(float input)
    {
        float scaledUpInput = input * pixelsPerUnit;
        float rounded = Mathf.Round(scaledUpInput);
        return rounded / pixelsPerUnit;
    }

    Vector3 PixelSnappedPosition(Vector3 pos)
    {
        return new Vector3(PixelSnappedPosition(pos.x), PixelSnappedPosition(pos.y), pos.z);
    }
}
