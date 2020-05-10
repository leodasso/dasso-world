using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimationPlayer))]
public class CharacterAnimator : MonoBehaviour
{
    public CharacterSpriteAnim idle;
    
    public CharacterSpriteAnim currentAnim;
    public PlatformBody platformBody;

    AnimationPlayer _animPlayer;
    
    // Start is called before the first frame update
    void Start()
    {
        _animPlayer = GetComponent<AnimationPlayer>();
        SetAnim(idle);
    }

    void SetAnim(CharacterSpriteAnim newAnim)
    {
        currentAnim = newAnim;
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentAnim || !platformBody) return;

        var animClip = currentAnim.AnimForFacingDirection(platformBody.facingDirection);
        _animPlayer.ApplyAnimationClip(animClip);
    }
}
