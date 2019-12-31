Shader "ClusterVR/InternalSDK/CRTMainScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BackGroundColor ("BackGroundColor", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BackGroundColor;

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.uv.x += (rand(floor(i.uv.y * 400.0) + fmod(_Time.y, 10.0)) - 0.5) * 0.005;

                float ep = 0.01;
                fixed4 col = tex2D(_MainTex, clamp(i.uv, ep, 1 - ep));
                if (length(max(abs(i.uv - 0.5) - 0.5 ,0.0)) > ep)
                {
                    col.a = 0;
                }
                else
                {
                    col.rgb = lerp(float3(1, 1, 1), col.rgb, col.a);
                }

                col.rgb *= sin(i.uv.y * 400 + _Time.y * 20.0) * 0.2 + 0.9;
                col.rgb *= frac(-i.uv.y - _Time.y * 0.3) * 0.3 + 0.7;

                UNITY_APPLY_FOG(i.fogCoord, col);

                return float4(col.rgb, 1);
            }
            ENDCG
        }
    }
}
