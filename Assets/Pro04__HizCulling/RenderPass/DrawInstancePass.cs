using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DrawInstancePass : ScriptableRenderPass
{
    struct MeshP
    {
        public Matrix4x4 matrix;
        public Vector4 color;
    }

    class PassData
    {
        public Material material;
        public Mesh mesh;
        public int instanceCount;
        public ComputeShader shader;
        public BufferHandle argsBuffer;
        public BufferHandle resultBuffer;
    }
    private Material material;
    private Mesh mesh;
    private int instanceCount;
    private ComputeShader shader;
    private MeshP[] meshPs;
    private ComputeBuffer matricesBuffer;
    private GraphicsBuffer argsBuffer;

    public void Init(Material material, Mesh mesh, int instanceCount, ComputeShader shader)
    {
        this.material = material;
        this.mesh = mesh;
        this.instanceCount = instanceCount;
        this.shader = shader;
        matricesBuffer = new ComputeBuffer(instanceCount, 16 * 4 + 4 * 4, ComputeBufferType.Append);
        InitArgs();
        renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    private void InitArgs()
    {
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)instanceCount;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, sizeof(uint) * 5);
        argsBuffer.SetData(args);
    }

    public void Dispose()
    {
        if (matricesBuffer != null)
        {
            matricesBuffer.Dispose();
            matricesBuffer = null;
        }

        if (argsBuffer != null)
        {
            argsBuffer.Dispose();
            argsBuffer = null;
        }
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddComputePass<PassData>("ComputeCulling", out var passData))
        {
            
            passData.shader = shader;
            passData.resultBuffer = renderGraph.ImportBuffer(argsBuffer);

            builder.UseBuffer(passData.resultBuffer, AccessFlags.Write);
            builder.SetRenderFunc((PassData data, ComputeGraphContext context) =>
            {
                ComputePass(data, context);
            });
        }

        using(var builder = renderGraph.AddRasterRenderPass<PassData>("DrawInstanceCompute", out var passData))
        {
            passData.material = material;
            passData.mesh = mesh;
            passData.instanceCount = instanceCount;
            passData.shader = shader;
            //passData.argsBuffer = renderGraph.ImportBuffer(argsBuffer);

            var color = frameData.Get<UniversalResourceData>().activeColorTexture;
            var depth = frameData.Get<UniversalResourceData>().activeDepthTexture;
            builder.SetRenderAttachment(color, 0, AccessFlags.ReadWrite);
            builder.SetRenderAttachmentDepth(depth);
            builder.AllowGlobalStateModification(true);
            
            //builder.UseBuffer(passData.argsBuffer, AccessFlags.ReadWrite);
            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                ExecutePass(data, context);
            });
        }
    }

    private void ComputePass(PassData data, ComputeGraphContext context)
    {
        var cmd = context.cmd;
        var result = data.resultBuffer;
        cmd.SetBufferCounterValue(matricesBuffer, 0);
       
        var shader = data.shader;
        var kernelId = shader.FindKernel("CSMain");
        Vector4[] frustumPlanes = CullTool.GetFrustumPlane(Camera.main);
        cmd.SetComputeIntParam(shader, "Count", instanceCount);
        cmd.SetComputeVectorArrayParam(shader, "Planes", frustumPlanes);
        cmd.SetComputeBufferParam(shader, kernelId, "Result", matricesBuffer);
        cmd.DispatchCompute(shader, kernelId, 1 + instanceCount / 64, 1, 1);
    }

    private void ExecutePass(PassData data, RasterGraphContext context)
    {
        var cmd = context.cmd;
        Material material = data.material;
        Mesh mesh = data.mesh;
        int instanceCount = data.instanceCount;

        cmd.SetGlobalBuffer("_MeshProps", matricesBuffer);
        GraphicsBuffer.CopyCount(matricesBuffer, argsBuffer, sizeof(int));
        var arg = new uint[5];
        argsBuffer.GetData(arg);

        var result = new MeshP[arg[1]];
        matricesBuffer.GetData(result);
        Debug.Log(arg[1]);
        cmd.DrawMeshInstancedIndirect(mesh, 0, material, 0, argsBuffer);
        //context.ExecuteCommandBuffer(cmd);
    }
}
