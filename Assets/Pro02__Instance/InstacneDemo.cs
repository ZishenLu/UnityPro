using UnityEngine;

public class InstacneDemo : MonoBehaviour
{
    public Material mat;
    public GameObject go;
    public int count = 1000;

    private Mesh mesh;
    private MaterialPropertyBlock mpb;
    
    private Matrix4x4[] matrices;
    private Vector4[] colors;

    private void Start()
    {
        mesh = go.GetComponent<MeshFilter>().sharedMesh;
        mpb = new MaterialPropertyBlock();

        matrices = new Matrix4x4[count];
        colors = new Vector4[count];

        for (int i = 0; i < count; i++)
        {
            matrices[i] = Matrix4x4.TRS(
                new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f)),
                Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)),
                Vector3.one * Random.Range(0.5f, 1f)
            );
            colors[i] = Color.Lerp(Color.red, Color.blue, Random.value);
        }

        mpb.SetVectorArray("_Colors", colors);
    }

    private void Update()
    {
        Graphics.DrawMeshInstanced(mesh, 0, mat, matrices, count, mpb);
    }
}
