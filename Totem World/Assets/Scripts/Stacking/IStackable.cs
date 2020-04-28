using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StackPosition
{
    Single,
    Top,
    Middle,
    Bottom,
}

public interface IStackable
{
    bool StackingAllowed();

    void GetStacked(IStackable newHat);

    StackPosition MyStackPosition();

    List<IStackable> GetAllAbove();
    List<IStackable> GetAllBelow();
    List<IStackable> GetFullStack();

    Vector2 Top();

    GameObject MyGameObject();
}
