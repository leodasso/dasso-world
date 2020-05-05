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
    void StackChanged();

    StackPosition MyStackPosition();

    void BreakAbove();
    void BreakBelow();

    List<IStackable> GetAllAbove();
    List<IStackable> GetAllBelow();
    List<IStackable> GetFullStack();

    Vector2 Top();
    IStackable BottomOfStack();
    GameObject MyGameObject();

    Vector2 DeltaPosition();
    void StackUpdate(IStackable belowMe);
}
