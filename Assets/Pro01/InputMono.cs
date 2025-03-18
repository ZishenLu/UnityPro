using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputMono : MonoBehaviour
{
    private GridCell _gridCell;
    private List<GameObject> _list;
    private int _count;
    private int _seed;

    void Start()
    {
        _count = 200;
        _seed = 10;
        Random.InitState(_seed);
        _gridCell = new GridCell(10);
        _list = new List<GameObject>(_count);
        for (int i = 0; i < _count; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.localPosition = new Vector3(Random.Range(0, 100), 0, Random.Range(0, 100));
            _list.Add(go);
            _gridCell.AddGameObject(go);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            var gos = _gridCell.GetGameObjectsByNear(new Vector2Int(0, 0), 18);
            Debug.Log(gos.Count);
            RenderColor(gos);
        }
    }

    private void RenderColor(List<GameObject> objects)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            var go = objects[i].GetComponent<MeshRenderer>();
            if(go != null)
            {
                go.material.color = Color.red;
            }
        }
    }
}
