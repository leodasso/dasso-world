using System.Collections;
using System.Collections.Generic;
using Arachnid;
using UnityEngine;

[RequireComponent(typeof(PlatformBody))]
public class AiCrawler : PlatformBodyActor, IPokeable
{
    public FloatReference speedFromPoke;
    public FloatReference disabledFromPokeTime;
    
    protected override void OnWallHit(RaycastHit2D hit)
    {
        base.OnWallHit(hit);
        PlatformBody body = hit.collider.GetComponent<PlatformBody>();
        if (body)
        {
            TurnAround();
        }
    }

    void TurnAround()
    {
        Walker w = GetComponent<Walker>();
        if (w) w.walkInput = -w.walkInput;
    }

    public void OnPoke(GameObject poker, Vector2 pokeDirection)
    {
        Vector2 vel = speedFromPoke.Value * pokeDirection.normalized;
        _platformBody.velocity += vel;
        _platformBody.DisableAbilitiesForTime(disabledFromPokeTime.Value);
        _platformBody.SetGravityDirection(Vector2.down);
        _platformBody.SetNoGroundingTime(.1f);
    }
}
