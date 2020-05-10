using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Anim Sprite/Sprite Animation")]

public class SpriteAnim : ScriptableObject
{
    public List<Sprite> sprites;
    public int framerate = 12;
}
