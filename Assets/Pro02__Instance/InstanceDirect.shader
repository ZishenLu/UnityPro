Shader "Custom/InstancedDirect" {
    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
            }; 

            struct MeshP
            {
                float4x4 mat;
                float4 color;
            };

            StructuredBuffer<MeshP> _MeshProps;

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                v2f o;
                float4 pos = mul(_MeshProps[instanceID].mat, i.vertex);
                o.vertex = UnityObjectToClipPos(pos);
                o.color = _MeshProps[instanceID].color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return i.color;
            }

            ENDCG
        }
    }
}