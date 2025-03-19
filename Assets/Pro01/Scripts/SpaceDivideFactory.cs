using System.Collections.Generic;
using UnityEngine;

abstract class ISpace
{
    public abstract void AddGameObject(GameObject go);

    public abstract List<GameObject> GetGameObjectsByNear(Rect range);
}

class SpaceDivideFactory
{
    ISpace _space;

    public SpaceDivideFactory(ISpace space)
    {
        _space = space;
        Debug.Log("Space Type : " + space.GetType().FullName);
    }

    public void AddGameObject(GameObject go)
    {
        _space.AddGameObject(go);
    }

    public List<GameObject> GetGameObjectsByNear(Rect rect)
    {
        return _space.GetGameObjectsByNear(rect);
    }
}
