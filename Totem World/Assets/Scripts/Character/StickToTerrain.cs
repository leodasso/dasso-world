using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StickToTerrain : PlatformBodyActor
{
    [ToggleLeft]
    public bool crawlUpWalls;

    [ShowIf("crawlUpWalls"), MinValue(.1f)]
    public float transitionTime;
    
    protected override void OnNormalChanged(Vector2 newNormal)
    {
        if (_platformBody.AbilitiesDisabled) return;
        base.OnNormalChanged(newNormal);
        _platformBody.SetGravityDirection(-newNormal);
    }


    protected override void OnWallHit(RaycastHit2D hit)
    {
        base.OnWallHit(hit);
        if (_platformBody.AbilitiesDisabled) return;

        // We don't want to crawl up another platform body
        PlatformBody body = hit.collider.GetComponent<PlatformBody>();
        if (body) return;
        
        Debug.DrawLine(hit.point, hit.point + hit.normal, Color.magenta, 5);

        float newAngle = Arachnid.Math.AngleFromVector2(hit.normal, -90);
        if (_platformBody.limitRotation && _platformBody.maxRotationAngle < Mathf.Abs(newAngle))
            return;

        _platformBody.SetGravityDirection(-hit.normal);

        Vector2 newPosition = hit.point + hit.normal.normalized * _platformBody.capsuleCollider.size.y / 2;
        StartCoroutine(LerpToWall(newAngle, newPosition));
    }
    
    

    IEnumerator LerpToWall(float newAngle, Vector2 newPosition)
    {
        _platformBody.enabled = false;
        
        Vector2 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, newAngle);
        float progress = 0;

        while (progress < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, progress);
            transform.position = Vector3.Lerp(startPosition, newPosition, progress);
            progress += Time.deltaTime / transitionTime;
            yield return null;
        }

        transform.position = newPosition;
        transform.rotation = endRotation;
        _platformBody.enabled = true;
    }
}
