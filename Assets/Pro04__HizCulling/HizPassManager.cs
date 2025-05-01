using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HizPassManager : MonoBehaviour
{
    private HizPass hizPass;
    public GameObject go;
    public Material material;
    public int count;
    public ComputeShader shader;
    public ComputeShader hizShader;

    private void Awake()
    {
        hizPass = new HizPass();
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        hizPass.Init(mesh, count, shader, material, hizShader);

    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
    }

    private void BeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        //if(cam.cameraType != CameraType.Game) return;

        cam.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(hizPass);
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
    }

    private void OnDestroy()
    {
        hizPass.Dispose();
    }
}
