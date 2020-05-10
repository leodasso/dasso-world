using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Anim Sprite/Character Sprite Animation")]
public class CharacterSpriteAnim : ScriptableObject
{
    public SpriteAnim facingLeft;
    public SpriteAnim facingRight;

    public SpriteAnim AnimForFacingDirection(FacingDirection direction)
    {
        return direction == FacingDirection.Left ? facingLeft : facingRight;
    }
}
