using UnityEngine;

public class HizCulling : MonoBehaviour
{
    public GameObject go;
    public ComputeShader shader;
    public Material material;
    public int count = 10000;

    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _resultBuffer;
    private Mesh _mesh;
    private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    private int _kernel;

    void Start()
    {
        _mesh = go.GetComponent<MeshFilter>().sharedMesh;
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _resultBuffer = new ComputeBuffer(count, 16 * sizeof(float) + 4 * sizeof(float), ComputeBufferType.Append);
        _kernel = shader.FindKernel("CSMain");
        shader.SetInt("Count", count);


        _args[0] = _mesh.GetIndexCount(0);
        _args[1] = (uint)count;
        _args[2] = _mesh.GetIndexStart(0);
        _args[3] = _mesh.GetBaseVertex(0);
        _argsBuffer.SetData(_args);
        UpdateRender();
    }

    void Update()
    {
        UpdateRender();
        Graphics.DrawMeshInstancedIndirect(_mesh, 0, material, new Bounds(Vector3.zero, new Vector3(100, 100, 100)), _argsBuffer);
    }

    private void UpdateRender()
    {
        Vector4[] planes = CullTool.GetFrustumPlane(Camera.main);
        _resultBuffer.SetCounterValue(0);
        shader.SetVectorArray("Planes", planes);
        shader.SetBuffer(_kernel, "Result", _resultBuffer);
        shader.Dispatch(_kernel, 1 + count / 64, 1, 1);
        material.SetBuffer("_MeshProps", _resultBuffer);
        var result = new MeshP[count];
        _resultBuffer.GetData(result);
        ComputeBuffer.CopyCount(_resultBuffer, _argsBuffer, sizeof(int));
    }
    void OnDisable()
    {
        _argsBuffer.Release();
        _argsBuffer = null;
        _resultBuffer.Release();
        _resultBuffer = null;
    }
}
