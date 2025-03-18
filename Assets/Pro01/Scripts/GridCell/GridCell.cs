using System.Collections.Generic;
using UnityEngine;

class GridCell
{
    private int _cellSize;
    private Dictionary<Vector2Int, List<GameObject>> _dic;

    public GridCell(int cellSize)
    {
        _cellSize = cellSize;
        _dic = new Dictionary<Vector2Int, List<GameObject>>();
    }

    public void AddGameObject(GameObject go)
    {
        Vector2Int cell = GetCell(go.transform.localPosition);
        if (!_dic.ContainsKey(cell))
        {
            _dic[cell] = new List<GameObject>();
        }
        _dic[cell].Add(go);
    }

    public Vector2Int GetCell(Vector3 pos)
    {
        return new Vector2Int((int)pos.x / _cellSize, (int)pos.z / _cellSize);
    }

    public List<GameObject> GetGameObjects(Vector2Int cell)
    {
        if (_dic.ContainsKey(cell))
        {
            return _dic[cell];
        }
        return new List<GameObject>();
    }

    public List<GameObject> GetGameObjectsByNear(Vector2Int cell, int range)
    {
        List<GameObject> list = new List<GameObject>();
        List<Vector2Int> cells = GetVector2Ints(cell, range);
        for (int i = 0; i < cells.Count; i++)
        {
            list.AddRange(GetGameObjects(cells[i]));
        }
        return list;
    }

    private List<Vector2Int> GetVector2Ints(Vector2Int cell, int range)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        for (int i = 0; i <= Mathf.CeilToInt(range / _cellSize); i++)
        {
            for (int j = 0; j <= Mathf.CeilToInt(range / _cellSize); j++)
            {
                list.Add(new Vector2Int(cell.x + i, cell.y + j));
            }
        }
        return list;
    }
}
