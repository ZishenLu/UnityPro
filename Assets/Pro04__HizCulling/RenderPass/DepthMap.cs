using System;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DepthMap : MonoBehaviour
{
    public Material depthMapMaterial;

    private void Awake()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Save();
        }
    }

    private void Save()
    {
        var savePath = Path.Combine(Application.dataPath, "/Pro04_HizCulling/DepthMap.png");
        Texture2D tex = Shader.GetGlobalTexture("_CameraDepthTexture") as Texture2D;

        Debug.Log(tex.height + " " + tex.width + " " + tex.name);
        // 将Texture2D保存为PNG文件
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);

        Debug.Log("Depth map saved to: " + savePath);
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType == CameraType.Game)
        {
            var cmd = CommandBufferPool.Get("DepthMap");

            cmd.Blit(null, BuiltinRenderTextureType.CameraTarget, depthMapMaterial);

            context.ExecuteCommandBuffer(cmd);
            context.Submit();
            CommandBufferPool.Release(cmd);
        }
    }

    private void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }
}
