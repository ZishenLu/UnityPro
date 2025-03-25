using System.Collections.Generic;
using UnityEngine;

class QuadTree : ISpace
{
    private List<GameObject> _objects;
    private int _max;
    private List<QuadTree> _children;
    private Rect _rect;

    public QuadTree()
    {

    }

    public QuadTree(Rect rect, int max)
    {
        Init(rect, max);
    }

    public void Init(Rect rect, int max)
    {
        _rect = rect;
        _max = max;
        _objects = new List<GameObject>();
        _children = new List<QuadTree>();
    }

    private bool _isDivide => _children.Count > 0;

    public override void Build(List<GameObject> objects)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            AddGameObject(objects[i]);
        }
    }

    public void AddGameObject(GameObject go)
    {
        var pos = go.transform.localPosition;
        if (!_rect.Contains(new Vector2(pos.x, pos.z)))
            return;

        if (_objects.Count < _max)
        {
            _objects.Add(go);
        }
        else
        {
            if(!_isDivide)
            {
                Divide();
            }

            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].AddGameObject(go);
            }
        }
    }

    public override void DebugDraw()
    {
        if (_isDivide)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].DebugDraw();
            }
        }
        //else
        {
            Gizmos.DrawLine(new Vector3(_rect.x, 0, _rect.y), new Vector3(_rect.x + _rect.width, 0, _rect.y));
            Gizmos.DrawLine(new Vector3(_rect.x + _rect.width, 0, _rect.y), new Vector3(_rect.x + _rect.width, 0, _rect.y + _rect.height));
            Gizmos.DrawLine(new Vector3(_rect.x + _rect.width, 0, _rect.y + _rect.height), new Vector3(_rect.x, 0, _rect.y + _rect.height));
            Gizmos.DrawLine(new Vector3(_rect.x, 0, _rect.y + _rect.height), new Vector3(_rect.x, 0, _rect.y));
        }
    }

    private void Divide()
    {
        float halfWidth = _rect.width / 2;
        float halfHeight = _rect.height / 2;
        _children.Add(new QuadTree(new Rect(_rect.x, _rect.y, halfWidth, halfHeight), _max));
        _children.Add(new QuadTree(new Rect(_rect.x + halfWidth, _rect.y, halfWidth, halfHeight), _max));
        _children.Add(new QuadTree(new Rect(_rect.x, _rect.y + halfHeight, halfWidth, halfHeight), _max));
        _children.Add(new QuadTree(new Rect(_rect.x + halfWidth, _rect.y + halfHeight, halfWidth, halfHeight), _max));
    }

    public override List<GameObject> GetGameObjectsByNear(Rect range)
    {
        List<GameObject> result = new List<GameObject>();
        if (_rect.Overlaps(range))
        {
            for (int i = 0; i < _children.Count; i++)
            {
                result.AddRange(_children[i].GetGameObjectsByNear(range));
            }
            
            
            for (int i = 0; i < _objects.Count; i++)
            {
                var pos = _objects[i].transform.localPosition;
                if (range.Contains(new Vector2(pos.x, pos.z)))
                {
                    result.Add(_objects[i]);
                }
            }
        }
        return result;
    }
}
