using UnityEngine;

struct MeshP
{
    public Matrix4x4 matrix;
    public Vector4 color;
}

public class InstanceDirectDemo : MonoBehaviour
{
    public Material mat;
    public GameObject go;
    public int count = 10000;

    private Mesh mesh;
    private MeshP[] meshPs;
    private Bounds bounds;

    private ComputeBuffer matricesBuffer;
    private ComputeBuffer argsBuffer;

    void Start()
    {
        mesh = go.GetComponent<MeshFilter>().sharedMesh;
        bounds = new Bounds(Vector3.zero, Vector3.one * 102);
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)count;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        meshPs = new MeshP[count];
        for (int i = 0; i < count; i++)
        {
            meshPs[i].matrix = Matrix4x4.TRS(
                new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-10f, 10f)),
                Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)),
                Vector3.one * Random.Range(0.5f, 1f)
            );
            meshPs[i].color = Color.Lerp(Color.red, Color.blue, Random.value);
        }

        matricesBuffer = new ComputeBuffer(count, 16 * 4 + 4 * 4);
        matricesBuffer.SetData(meshPs);
        mat.SetBuffer("_MeshProps", matricesBuffer);
    }


    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, argsBuffer);

    }
}
