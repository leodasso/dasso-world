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
    
    [HorizontalGroup("a"), LabelWidth(70)]
    public int rayCount = 5;
    
    [HorizontalGroup("a"), LabelWidth(60)]
    public float distance = .1f;
    
    [HorizontalGroup("b"), LabelWidth(60)]
    public float offset = .1f;
    
    [HorizontalGroup("b"), LabelWidth(60)]
    public float padding = .05f;

    [HorizontalGroup("b"), LabelWidth(60)]
    [Tooltip("Splay the raycasts apart like spread fingers")]
    public float splay;

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

    public void DrawGizmos(Color color, float additionalDistance, CastingDirection castingDirection, Vector2 castingVector, CapsuleCollider2D collider)
    {
        if (!enabled) return;
        Vector2 startPoiont = castingDirection == CastingDirection.Horizontal?
            CastingTop(collider) : CastingLeft(collider);

        Vector2 endPoint = castingDirection == CastingDirection.Horizontal? 
            CastingBottom(collider) : CastingRight(collider);

        Gizmos.color = color;
        
        DrawRaycastGizmos(castingVector, startPoiont, endPoint, additionalDistance + distance);
    }
    
    void DrawRaycastGizmos(Vector2 direction, Vector2 beginCastPoint, Vector2 endCastPoint, float rayLength)
    {
        Vector2 castPoint = beginCastPoint;

        Vector2 splayThing = Vector3.Cross(direction, Vector3.forward);

        for (int i = 0; i < rayCount; i++)
        {
            Vector2 splayAmount = splay * splayThing * i;
            splayAmount -= splay * splayThing * ((float)rayCount - 1)/ 2;
            
            var loopProgress = i / ( rayCount - 1f);
            castPoint = Vector2.Lerp(beginCastPoint, endCastPoint, loopProgress);
            Gizmos.DrawLine(castPoint, castPoint + (direction + splayAmount).normalized * rayLength);
        }
    }
    
    public RaycastHit2D RaycastDirection(Vector2 direction, Vector2 beginCastPoint, Vector2 endCastPoint, 
        float additionalLength, LayerMask layerMask, Collider2D myCollider)
    {
        List<RaycastHit2D> hits =
            RaycastDirectionAll(direction, beginCastPoint, endCastPoint, additionalLength, layerMask, myCollider);
        return hits.Count < 1 ? new RaycastHit2D() : hits[0];
    }
    
    public List<RaycastHit2D> RaycastDirectionAll(Vector2 direction, Vector2 beginCastPoint, Vector2 endCastPoint, 
        float additionalLength, LayerMask layerMask, Collider2D myCollider)
    {
        List<RaycastHit2D> allHits = new List<RaycastHit2D>();
        Vector2 splayThing = Vector3.Cross(direction, Vector3.forward);
		
        for (int i = 0; i < rayCount; i++)
        {
            Vector2 splayAmount = splay * splayThing * i;
            splayAmount -= splay * splayThing * ((float)rayCount - 1)/ 2;
            
            var loopProgress = i / ( rayCount - 1f);
            var castPoint = Vector2.Lerp(beginCastPoint, endCastPoint, loopProgress);

            RaycastHit2D[] hits = new RaycastHit2D[10];
            Vector2 finalDir = (direction + splayAmount).normalized;
            Physics2D.RaycastNonAlloc(castPoint, finalDir, hits, additionalLength + distance, layerMask);

            List<RaycastHit2D> orderedHits = hits.Where(x => x.collider != null && x.collider != myCollider).ToList();
            
            if (orderedHits.Count < 1) continue;
            orderedHits = orderedHits.OrderBy(x => x.distance).ToList();
            RaycastHit2D firstHit = orderedHits.First();
            if (firstHit.collider != null)
                allHits.Add(firstHit);
        }

        return allHits.OrderBy(x => x.distance).ToList();
    }
}
