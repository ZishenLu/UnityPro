Shader "Custom/DepthVisualizer" {
    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        //     struct Attributes {
        // #if SHADER_API_GLES
		      //   float4 positionOS : POSITION;
		      //   float2 uv : TEXCOORD0;
        // #else
		      //   uint vertexID : SV_VertexID;
        // #endif
        //     };

            // struct Varyings {
            //     float2 uv : TEXCOORD0;
            //     float4 positionCS : SV_POSITION;
            // };

            

            half4 frag(Varyings input) : SV_Target {
                // 采样深度纹理
                float depth = SampleSceneDepth(input.texcoord.xy);
                // 转换为线性深度（根据摄像机远近裁剪平面）
                depth = Linear01Depth(depth, _ZBufferParams);
                // 将深度值映射为灰度颜色
                return half4(depth, depth, depth, 1);
            }
            ENDHLSL
        }
    }
}