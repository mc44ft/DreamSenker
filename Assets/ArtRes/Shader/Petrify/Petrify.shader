Shader "Custom/MedusaGaze"
{
    Properties
    {
        // ---------------- 改动 1：加上 [HDR] 标签 ----------------
        [HDR] _MainColor ("Color", Color) = (0, 2.5, 0.5, 0.5) // 默认亮度给高点
        _Angle ("Angle View (0-180)", Range(0, 180)) = 60
        _GradientSpeed ("Flow Speed", Float) = 1.0
        _MainTex ("Noise Texture (Optional)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // 注意：这里变量声明不需要改，float4 足够容纳 HDR 颜色
            float4 _MainColor;
            float _Angle;
            float _GradientSpeed;
            sampler2D _MainTex;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // ---------------- 改动 2：返回值改为 half4 以支持高动态范围 ----------------
            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - float2(0.5, 0); 
                float dist = length(uv);
                float angle = degrees(atan2(uv.x, uv.y));
                float halfAngle = _Angle * 0.5;

                if (abs(angle) > halfAngle || dist > 1.0)
                {
                    return half4(0,0,0,0);
                }

                float fade = smoothstep(halfAngle, halfAngle - 5, abs(angle)); 
                fade *= (1.0 - dist);

                float2 scrollUV = i.uv;
                scrollUV.y -= _Time.y * _GradientSpeed;
                fixed4 noise = tex2D(_MainTex, scrollUV);

                half4 col = _MainColor; // 使用 half4
                col.a *= fade * noise.r;

                return col;
            }
            ENDCG
        }
    }
}