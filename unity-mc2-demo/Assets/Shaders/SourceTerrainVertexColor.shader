Shader "MC2Demo/Source Terrain Vertex Color"
{
    Properties
    {
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Terrain Composite", 2D) = "white" {}
        _TextureStrength ("Texture Strength", Range(0, 1)) = 0
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
                fixed4 color : COLOR;
            };

            float4 _Tint;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TextureStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float light = saturate(dot(normal, normalize(float3(-0.35, 0.78, 0.52)))) * 0.24 + 0.82;
                fixed4 terrain = tex2D(_MainTex, i.uv);
                float3 detail = lerp(float3(1, 1, 1), saturate(terrain.rgb * 1.35), saturate(_TextureStrength));
                return fixed4(i.color.rgb * detail * _Tint.rgb * light, i.color.a * _Tint.a);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
