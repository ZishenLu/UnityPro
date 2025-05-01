using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthMapManager : MonoBehaviour
{
    public Material depthMapMaterial;
    public Mesh mesh;
    public int instanceCount = 1000;
    public ComputeShader shader;
    private DepthMapPass dpass;
    public DrawInstancePass pass;

    private void Awake()
    {
        pass = new DrawInstancePass();
        pass.Init(depthMapMaterial, mesh, instanceCount, shader);

        dpass = new DepthMapPass();
        dpass.Init(depthMapMaterial);

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(dpass);
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        pass.Dispose();
        dpass.Dispose();
    }
}
