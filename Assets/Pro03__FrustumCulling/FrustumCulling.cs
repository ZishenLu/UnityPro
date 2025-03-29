using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FrustumCulling : MonoBehaviour
{
    public Material mat;
    public GameObject go;
    public int count = 10000;
    public ComputeShader cullingShader;

    private Mesh mesh;
    private Bounds bounds;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer cullingBuffer;
    private int kernel;

    void Start()
    {
        mesh = go.GetComponent<MeshFilter>().sharedMesh;
        bounds = go.GetComponent<MeshRenderer>().bounds;
        kernel = cullingShader.FindKernel("CSMain");
        Debug.Log("bounds: " + bounds);
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)count;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        cullingBuffer = new ComputeBuffer(count, 16 * sizeof(float) + 4 * sizeof(float), ComputeBufferType.Append);
        
        UpdateShader();
    }

    private void UpdateShader()
    {
        Vector4[] frustumPlanes = CullTool.GetFrustumPlane(Camera.main);
        cullingBuffer.SetCounterValue(0);
        //for (int i = 0; i < 6; i++)
        //{
        //    Debug.Log(i + ": " + frustumPlanes[i]);
        //}
        cullingShader.SetInt("Count", count);
        cullingShader.SetBuffer(kernel, "Result", cullingBuffer);
        cullingShader.SetVectorArray("Panel", frustumPlanes);
        cullingShader.Dispatch(kernel, 1 + count / 640, 1, 1);
        mat.SetBuffer("_MeshProps", cullingBuffer);
        ComputeBuffer.CopyCount(cullingBuffer, argsBuffer, 0);
    }

    void Update()
    {
        UpdateShader();
        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, 
            new Bounds(new Vector3(0, 0, 0), new Vector3(51, 2, 51)), argsBuffer);
    }

    void OnDisable()
    {
        argsBuffer.Release();
        argsBuffer = null;
        cullingBuffer.Release();
        cullingBuffer = null;
    }
}
