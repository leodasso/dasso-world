using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class RaycastGroup
{
    public enum CastingDirection
    {
        Vertical, Horizontal
    }
    
    [HorizontalGroup("a"), LabelWidth(50), ToggleLeft]
    public bool enabled = true;

    [HorizontalGroup("a"), LabelWidth(40), ToggleLeft]
    public bool gizmos = true;
    
    [HorizontalGroup("a"), LabelWidth(70)]
    public int rayCount = 5;
    
    [HorizontalGroup("a"), LabelWidth(60)]
    public float distance = .1f;
    
    
    // TODO change to vec2
    [HorizontalGroup("b"), LabelWidth(60)]
    public float offset = .1f;
    
    // TODO change to vec2
    [HorizontalGroup("b"), LabelWidth(60)]
    public float padding = .05f;

    public Vector2 CastingLeft(CapsuleCollider2D capsuleCollider)
    {
        return capsuleCollider.Left() + (offset + padding) * (Vector2) capsuleCollider.transform.right;
    }

    public Vector2 CastingRight(CapsuleCollider2D capsuleCollider)
    {
        return capsuleCollider.Right() + (offset - padding) * (Vector2) capsuleCollider.transform.right;
    }

    public Vector2 CastingTop(CapsuleCollider2D capsuleCollider)
    {
        return capsuleCollider.Top() + (offset - padding) * (Vector2) capsuleCollider.transform.up;
    }

    public Vector2 CastingBottom(CapsuleCollider2D capsuleCollider)
    {
        return capsuleCollider.Bottom() + (offset + padding) * (Vector2) capsuleCollider.transform.up;
    }
    
    public Vector2 CastingBottomLeft(CapsuleCollider2D capsuleCollider)
    {
        return new Vector2(CastingLeft(capsuleCollider).x, CastingBottom(capsuleCollider).y);
    }
    
    public Vector2 CastingBottomRight(CapsuleCollider2D capsuleCollider)
    {
        return new Vector2(CastingRight(capsuleCollider).x, CastingBottom(capsuleCollider).y);
    }
    
    public Vector2 CastingTopLeft(CapsuleCollider2D capsuleCollider)
    {
        return new Vector2(CastingLeft(capsuleCollider).x, CastingTop(capsuleCollider).y);
    }
    
    public Vector2 CastingTopRight(CapsuleCollider2D capsuleCollider)
    {
        return new Vector2(CastingRight(capsuleCollider).x, CastingTop(capsuleCollider).y);
    }

    public void DrawCornerGizmos(CapsuleCollider2D collider)
    {
        if (!enabled || !gizmos) return;
        float radius = .1f;
        Gizmos.DrawWireSphere(CastingTopLeft(collider), radius);
        Gizmos.DrawWireSphere(CastingTopRight(collider), radius);
        Gizmos.DrawWireSphere(CastingBottomLeft(collider), radius);
        Gizmos.DrawWireSphere(CastingBottomRight(collider), radius);
    }

    public void DrawGizmos(Color color, float additionalDistance, CastingDirection castingDirection, Vector2 castingVector, CapsuleCollider2D collider)
    {
        if (!enabled || !gizmos) return;
        Vector2 startPoiont = castingDirection == CastingDirection.Horizontal?
            CastingTop(collider) : CastingLeft(collider);

        Vector2 endPoint = castingDirection == CastingDirection.Horizontal? 
            CastingBottom(collider) : CastingRight(collider);

        Gizmos.color = color;
        
        DrawRaycastGizmos(castingVector, startPoiont, endPoint, additionalDistance + distance);
    }
    
    void DrawRaycastGizmos(Vector2 direction, Vector2 beginCastPoint, Vector2 endCastPoint, float rayLength)
    {
        for (int i = 0; i < rayCount; i++)
        {
            var loopProgress = i / ( rayCount - 1f);
            var castPoint = Vector2.Lerp(beginCastPoint, endCastPoint, loopProgress);
            Gizmos.DrawLine(castPoint, castPoint + direction.normalized * rayLength);
        }
    }
    
    // Overload for just using a single collider
    public RaycastHit2D RaycastDirection(Vector2 direction, Vector2 beginCastPoint, Vector2 endCastPoint, 
        float additionalLength, LayerMask layerMask, Collider2D myCollider)
    {
        List<Collider2D> myColliders = new List<Collider2D> {myCollider};
        return RaycastDirection(direction, beginCastPoint, endCastPoint, additionalLength, layerMask, myColliders);
    }
    
    public RaycastHit2D RaycastDirection(Vector2 direction, Vector2 beginCastPoint, Vector2 endCastPoint, 
        float additionalLength, LayerMask layerMask, List<Collider2D> myColliders)
    {
        List<RaycastHit2D> hits =
            RaycastDirectionAll(direction, beginCastPoint, endCastPoint, additionalLength, layerMask, myColliders);
        return hits.Count < 1 ? new RaycastHit2D() : hits[0];
    }
    
    public List<RaycastHit2D> RaycastDirectionAll(Vector2 direction, Vector2 beginCastPoint, Vector2 endCastPoint, 
        float additionalLength, LayerMask layerMask, List<Collider2D> myColliders)
    {
        List<RaycastHit2D> allHits = new List<RaycastHit2D>();
		
        for (int i = 0; i < rayCount; i++)
        {           
            var loopProgress = i / ( rayCount - 1f);
            var castPoint = Vector2.Lerp(beginCastPoint, endCastPoint, loopProgress);

            RaycastHit2D[] hits = new RaycastHit2D[10];
            Vector2 finalDir = direction.normalized;
            Physics2D.RaycastNonAlloc(castPoint, finalDir, hits, additionalLength + distance, layerMask);
            
            Debug.DrawRay(castPoint, finalDir, Color.cyan, 30);

            List<RaycastHit2D> orderedHits = hits.Where(x => x.collider != null && !myColliders.Contains(x.collider)).ToList();
            
            if (orderedHits.Count < 1) continue;
            orderedHits = orderedHits.OrderBy(x => x.distance).ToList();
            RaycastHit2D firstHit = orderedHits.First();
            if (firstHit.collider != null)
                allHits.Add(firstHit);
        }

        return allHits.OrderBy(x => x.distance).ToList();
    }
}
