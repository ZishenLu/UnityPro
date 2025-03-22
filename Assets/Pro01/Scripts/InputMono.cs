using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

enum SpaceType
{
    GridCell = 0,
    QuadTree,
    BVHTree,
}

public class InputMono : MonoBehaviour
{
    [SerializeField] private GameObject _go;
    [SerializeField] private SpaceType _spaceType;

    private SpaceDivideFactory _factory;
    private List<GameObject> _list;
    private int _count;
    private int _seed;
   
    private MaterialPropertyBlock _block;
    private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        _count = 200;
        _seed = 10;
        _block = new MaterialPropertyBlock();
        _list = new List<GameObject>(_count);

        SetSpace();
        InitObjs();
    }

    private void InitObjs()
    {
        Random.InitState(_seed);
        for (int i = 0; i < _count; i++)
        {
            GameObject go = Instantiate(_go);
            go.transform.localPosition = new Vector3(Random.Range(0, 100), 0, Random.Range(0, 100));
            _list.Add(go);
        }

        _factory.Build(_list);
    }

    private void SetSpace()
    {
        switch (_spaceType)
        {
            case SpaceType.GridCell:
                var grid = new GridCell();
                grid.Init(10);
                _factory = new SpaceDivideFactory(grid);
                break;
            case SpaceType.QuadTree:
                var quadTree = new QuadTree();
                quadTree.Init(new Rect(0, 0, 100, 100), 5);
                _factory = new SpaceDivideFactory(quadTree);
                break;
            case SpaceType.BVHTree:
                var bvhTree = new BVHTree();
                _factory = new SpaceDivideFactory(bvhTree);
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            var gos = _factory.GetGameObjectsByNear(new Rect(0, 0, 18, 18));
            Debug.Log(gos.Count);
            RenderColor(gos);
        }
    }

    void OnDrawGizmos()
    {
        if (_factory != null)
        {
            _factory.DebugDraw();
        }
    }

    private void RenderColor(List<GameObject> objects)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            var go = objects[i].GetComponent<MeshRenderer>();
            if(go != null)
            {
                _block.SetColor(ColorId, Color.red);
                go.SetPropertyBlock(_block);
            }
        }
    }
}
