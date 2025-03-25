using System.Collections.Generic;
using UnityEngine;

abstract class ISpace
{
    public abstract void Build(List<GameObject> objects);

    public abstract List<GameObject> GetGameObjectsByNear(Rect range);

    public abstract void DebugDraw();
}

class SpaceDivideFactory
{
    ISpace _space;

    public SpaceDivideFactory(ISpace space)
    {
        _space = space;
        Debug.Log("Space Type : " + space.GetType().FullName);
    }

    public void Build(List<GameObject> objects)
    {
        _space.Build(objects);
    }

    public List<GameObject> GetGameObjectsByNear(Rect rect)
    {
        return _space.GetGameObjectsByNear(rect);
    }

    public void DebugDraw()
    {
        _space.DebugDraw();
    }
}
