Shader "Custom/DeathOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // general values
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _AnimationLength;

            // animation activation values
            int _AnimateType = 0;
            float _AnimateLength = 0.5;
            float _BaseTime = 0;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 result = fixed4(0, 0, 0, 0);

                if (_AnimateType == 1)
                {
                    fixed alpha = ((_Time[1] - _BaseTime) > _AnimateLength * i.uv[1])
                        ? 1 : 0;
                    result = fixed4(0, 0, 0, alpha);
                }
                else if (_AnimateType == 2)
                {
                    float deltaT = _Time[1] - _BaseTime;
                    fixed alpha = ((deltaT > _AnimateLength * 2 * i.uv[1])
                        || (deltaT > _AnimateLength * 2 * (1 - i.uv[1])))
                        ? 1 : 0;
                    result = fixed4(0, 0, 0, alpha);
                }
                return result;
            }
            ENDCG
        }
    }
}
