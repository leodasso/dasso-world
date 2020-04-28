using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControllable
{
    void ApplyLeftStickInput(Vector2 input);
    void JumpPressed();
    void JumpReleased();

    void AlphaPressed();
    void AlphaReleased();
}
