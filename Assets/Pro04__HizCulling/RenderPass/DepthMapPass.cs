using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DepthMapPass : ScriptableRenderPass
{
    public Material material;

    class PassData
    {
        public TextureHandle screenColor;
        public Material material;
    }

    public void Init(Material mat)
    {
        renderPassEvent = RenderPassEvent.BeforeRendering;
        material = mat;
    }

    public void Dispose()
    {
        material = null;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out PassData data))
        {
            data.screenColor = frameData.Get<UniversalResourceData>().activeColorTexture;
            data.material = material;

            //builder.UseTexture(data.screenColor);

            builder.SetRenderAttachment(data.screenColor, 0, AccessFlags.ReadWrite);

            builder.SetRenderFunc<PassData>((PassData data, RasterGraphContext context) =>
            {
                ExecutePass(data, context);
            });
        }

        //using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName + "aa", out PassData data))
        //{
        //    data.screenColor = frameData.Get<UniversalResourceData>().activeColorTexture;
        //    data.material = material;

        //    //builder.UseTexture(data.screenColor);

        //    builder.SetRenderAttachment(data.screenColor, 0, AccessFlags.ReadWrite);

        //    builder.SetRenderFunc<PassData>((PassData data, RasterGraphContext context) =>
        //    {
        //        ExecutePass1(data, context);
        //    });
        //}
    }

    private void ExecutePass(PassData data, RasterGraphContext context)
    {
        //var cmd = CommandBufferPool.Get("DepthMapPass");
        //cmd.Blit(data.screenColor, data.screenColor, data.material);
        //Graphics.ExecuteCommandBuffer(cmd);
        //CommandBufferPool.Release(cmd);
        Blitter.BlitTexture(context.cmd, data.screenColor, new Vector4(1,1,0,0), data.material, 0);
    }

    private void ExecutePass1(PassData data, RasterGraphContext context)
    {
        //var cmd = CommandBufferPool.Get("DepthMapPass");
        //cmd.Blit(data.screenColor, data.screenColor, data.material);
        //Graphics.ExecuteCommandBuffer(cmd);
        //CommandBufferPool.Release(cmd);
        Blitter.BlitTexture(context.cmd, data.screenColor, new Vector4(1, 1, 0, 0), data.material, 0);
    }
}