using System.Drawing;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class HizManager
{
    private static HizManager _inst;
    public static HizManager Inst
    {
        get
        {
            if(_inst == null)
                _inst = new HizManager();
            return _inst;
        }
    }

    struct MeshP
    {
        public float4x4 matrix;
        public float4 color;
    }

    class HizCullingData
    {
        public ComputeShader shader;
        public Matrix4x4 lastVP;
        public int instanceCount;
        public BufferHandle resultBuffer;
        public TextureHandle hizMap;
        public TextureHandle depthTex;
    }

    class HizDrawData
    {
        public Mesh mesh;
        public Material material;
        public TextureHandle hizMap;
    }

    private int _count;
    private Material _material;
    private Mesh _mesh;
    private GraphicsBuffer _argsBuffer;
    private GraphicsBuffer _resultBuffer;
    private ComputeShader _shader;
    private ComputeShader _hizShader;
    private RTHandle _hizMap;
    private int _hizMapSize;
    private Matrix4x4 _lastVP;

    public void Init(int count, Mesh mesh, Material mat, ComputeShader shader, ComputeShader hizShader)
    {
        _count = count;
        _material = mat;
        _mesh = mesh;
        _shader = shader;
        _hizShader = hizShader;
        _hizMapSize = 1024;

        InitArgs();
        InitHizMap();
    }

    private void InitArgs()
    {
        _resultBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, _count, (16 + 4) * sizeof(float));
        _argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, 5 * sizeof(uint));

        var args = new uint[5];
        args[0] = _mesh.GetIndexCount(0);
        args[1] = (uint)_count;
        args[2] = _mesh.GetIndexStart(0);
        args[3] = _mesh.GetBaseVertex(0);
        
        _argsBuffer.SetData(args);
    }

    private void InitHizMap()
    {
        var desc = new RenderTextureDescriptor(_hizMapSize, _hizMapSize / 2, RenderTextureFormat.RFloat, 0, 8);
        desc.useMipMap= true;
        desc.autoGenerateMips = false;
        desc.enableRandomWrite = true;
        _hizMap = RTHandles.Alloc(desc, name: "HizMap");
    }

    private void UpdateHizMap(HizCullingData data, ComputeGraphContext context)
    {
        var cmd = context.cmd;
        int size = _hizMapSize;
        var kernel = _hizShader.FindKernel("ComputeHizMap");
        var kernel_copy = _hizShader.FindKernel("SampleDepth");
        var depth = data.depthTex;
        var proj = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, true);
        _lastVP = proj * Camera.main.worldToCameraMatrix;

        cmd.SetComputeTextureParam(_hizShader, kernel_copy, "Input", depth);
        cmd.SetComputeTextureParam(_hizShader, kernel_copy, "Output", data.hizMap);
        cmd.SetComputeVectorParam(_hizShader, "_outSize", new Vector4(size, size / 2, 0, 0));
        cmd.DispatchCompute(_hizShader, kernel_copy, size / 8, size / 16, 1);

        for (int i = 0; i < 8; i++)
        {
            size /= 2;
            cmd.SetComputeTextureParam(_hizShader, kernel, "Source", data.hizMap, i);
            cmd.SetComputeTextureParam(_hizShader, kernel, "Result", data.hizMap, i + 1);
            cmd.SetComputeFloatParam(_hizShader, "_Size", size);
            cmd.DispatchCompute(_hizShader, kernel, (size + 7) / 8, (size + 15) / 16, 1);
        }
    }

    public void OccludeTest(RenderGraph graph, ContextContainer context)
    {
        var type = context.Get<UniversalCameraData>().cameraType;
        if (type != CameraType.Game)
            return;
        var depth = context.Get<UniversalResourceData>().activeDepthTexture;
        using (var bulider = graph.AddComputePass<HizCullingData>("HizCulling", out var passData))
        {
            passData.hizMap = graph.ImportTexture(_hizMap);
            passData.depthTex = depth;
            bulider.UseTexture(passData.depthTex, AccessFlags.Read);
            bulider.UseTexture(passData.hizMap, AccessFlags.ReadWrite);
            bulider.SetRenderFunc((HizCullingData data, ComputeGraphContext context) =>
            {
                UpdateHizMap(data, context);
            });
        }
    }

    public void UpdateOcclude(RenderGraph graph, ContextContainer context)
    {
        var type = context.Get<UniversalCameraData>().cameraType;
        if(type != CameraType.Game)
            return;
        var depth = context.Get<UniversalResourceData>().activeDepthTexture;
        using (var bulider = graph.AddComputePass<HizCullingData>("HizCulling", out var passData))
        {
            passData.shader = _shader;
            passData.instanceCount = _count;
            passData.resultBuffer = graph.ImportBuffer(_resultBuffer);
            passData.hizMap = graph.ImportTexture(_hizMap);
            passData.depthTex = depth;
            passData.lastVP = _lastVP;
            bulider.UseBuffer(passData.resultBuffer, AccessFlags.Write);
            bulider.UseTexture(passData.depthTex, AccessFlags.Read);
            bulider.UseTexture(passData.hizMap, AccessFlags.ReadWrite);
            bulider.SetRenderFunc((HizCullingData data, ComputeGraphContext context) =>
            {
                //UpdateHizMap(data, context);
                ComputePass(data, context);
            });
        }
    }

    private void ComputePass(HizCullingData data, ComputeGraphContext context)
    {
        var cmd = context.cmd;
        var shader = data.shader;
        var lastVP = data.lastVP;
        var kernelId = shader.FindKernel("CSMain");
        Vector4[] frustumPlanes = CullTool.GetFrustumPlane(Camera.main);

        cmd.SetBufferCounterValue(_resultBuffer, 0);
        cmd.SetComputeIntParam(shader, "Count", data.instanceCount);
        cmd.SetComputeVectorArrayParam(shader, "Planes", frustumPlanes);
        cmd.SetComputeBufferParam(shader, kernelId, "Result", data.resultBuffer);
        cmd.SetComputeTextureParam(shader, kernelId, "_HizMap", data.hizMap);
        cmd.SetComputeVectorParam(shader, "ScreenSize", new Vector4(Screen.width, Screen.height, 8, 0));
        cmd.SetComputeMatrixParam(shader, "ViewProjection", lastVP);
        cmd.DispatchCompute(shader, kernelId, 1 + data.instanceCount / 64, 1, 1);
        cmd.CopyCounterValue(_resultBuffer, _argsBuffer, sizeof(uint));
    }

    public void DrawInstance(RenderGraph graph, ContextContainer context)
    {
        using (var bulider = graph.AddRasterRenderPass<HizDrawData>("HizDrawing", out var passData))
        {
            passData.material = _material;
            passData.mesh = _mesh;
            passData.hizMap = graph.ImportTexture(_hizMap);

            var tex = context.Get<UniversalResourceData>().activeColorTexture;
            var depth = context.Get<UniversalResourceData>().activeDepthTexture;

            bulider.SetRenderAttachment(tex, 0, AccessFlags.ReadWrite);
            bulider.SetRenderAttachmentDepth(depth, AccessFlags.ReadWrite);
            bulider.UseTexture(passData.hizMap, AccessFlags.Read);
            bulider.AllowGlobalStateModification(true);
            bulider.SetRenderFunc((HizDrawData data, RasterGraphContext context) =>
            {
                ExecutePass(data, context);
            });
        }
    }

    private void ExecutePass(HizDrawData data, RasterGraphContext context)
    {
        var cmd = context.cmd;
        var mesh = data.mesh;
        var mat = data.material;

        //cmd.SetGlobalTexture("_MainTex", data.hizMap);
        //Blitter.BlitTexture(cmd, _hizMap, new Vector4(1, 1, 0, 0), mat, 0);
        cmd.SetGlobalBuffer("_MeshProps", _resultBuffer);
        //GraphicsBuffer.CopyCount(_resultBuffer, _argsBuffer, sizeof(uint));

        cmd.DrawMeshInstancedIndirect(mesh, 0, mat, 0, _argsBuffer);
    }

    public void Dispose()
    {
        if (_argsBuffer != null)
        {
            _argsBuffer.Dispose();
            _argsBuffer = null;
        }

        if (_resultBuffer != null)
        {
            _resultBuffer.Dispose();
            _resultBuffer = null;
        }
    }
}
