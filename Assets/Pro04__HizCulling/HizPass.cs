using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class HizPass : ScriptableRenderPass
{
    public void Init(Mesh mesh, int count, ComputeShader shader, Material mat, ComputeShader hizShader)
    {
        HizManager.Inst.Init(count, mesh, mat, shader, hizShader);
        renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        HizManager.Inst.UpdateOcclude(renderGraph, frameData);

        HizManager.Inst.DrawInstance(renderGraph, frameData);

        HizManager.Inst.OccludeTest(renderGraph, frameData);
    }

    public void Dispose()
    {
        HizManager.Inst.Dispose();
    }
}
