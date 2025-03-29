Shader "Unlit/InstanceShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            struct meshp
            {
				float4x4 mat;
                float4 color;
			};
            
            StructuredBuffer<meshp> _MeshProps;

            v2f vert (appdata v, uint instanceId : SV_InstanceID)
            {
                v2f o;
                float4 pos = mul(_MeshProps[instanceId].mat, v.vertex);
                o.vertex = UnityObjectToClipPos(pos);
                o.color = _MeshProps[instanceId].color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
