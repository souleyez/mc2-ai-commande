Shader "MC2Demo/Private Reference Team Color"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TeamColor ("Team Color", Color) = (0.25, 0.78, 1.0, 1.0)
        _TeamStrength ("Team Strength", Range(0, 1)) = 0.86
        _BaseTint ("Base Tint", Color) = (1, 1, 1, 1)
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
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TeamColor;
            float4 _BaseTint;
            float _TeamStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                float strongestNonBlue = max(tex.r, tex.g);
                float blueLead = tex.b - strongestNonBlue;
                float teamMask = smoothstep(0.08, 0.26, blueLead) * smoothstep(0.18, 0.40, tex.b);
                float shade = saturate(dot(tex.rgb, float3(0.30, 0.59, 0.11)) * 1.15 + 0.20);
                float3 teamRgb = _TeamColor.rgb * shade;
                float3 detailRgb = tex.rgb * _BaseTint.rgb;
                float3 color = lerp(detailRgb, teamRgb, saturate(teamMask * _TeamStrength));

                float3 normal = normalize(i.normal);
                float light = saturate(dot(normal, normalize(float3(-0.35, 0.78, 0.52)))) * 0.32 + 0.78;
                return fixed4(color * light, tex.a * _BaseTint.a);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}
