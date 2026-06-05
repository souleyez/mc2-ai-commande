Shader "MC2Demo/Source Terrain Vertex Color"
{
    Properties
    {
        _Tint ("Tint", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                fixed4 color : COLOR;
            };

            float4 _Tint;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float light = saturate(dot(normal, normalize(float3(-0.35, 0.78, 0.52)))) * 0.24 + 0.82;
                return fixed4(i.color.rgb * _Tint.rgb * light, i.color.a * _Tint.a);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
