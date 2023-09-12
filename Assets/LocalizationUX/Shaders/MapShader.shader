Shader "Custom/Ripple" 
{
        Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Time ("Time", Range(0,1)) = 0
        _Resolution ("Resolution", Vector) = (1, 1, 0, 0)
        _RippleOrigin ("Ripple Origin", Vector) = (0, 0, 0, 0)
        _RippleFrequency ("Ripple Frequency", Range(0, 50)) = 10
        _RippleAmplitude ("Ripple Amplitude", Range(0, 1)) = 0.01
        _RippleSpeed ("Ripple Speed", Range(0, 1000)) = 10
        _RippleAttenuation ("Attenuation Speed", Range(0, 100)) = 1
        _RippleActivity ("Ripple Activity", Range(0,1)) = 0
        
    }
    SubShader
    {
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Resolution;
            float4 _RippleOrigin;
            float _RippleFrequency;
            float _RippleAmplitude;
            float _RippleSpeed;
            float _RippleAttenuation;
            float _RippleActivity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 rippleOrigin = _RippleOrigin.xy / _Resolution.xy;
                rippleOrigin.y *= _Resolution.y / _Resolution.x;

                float dist = distance(uv, rippleOrigin);
                float attenuation = max(0.0, 1.0 - dist * _RippleAttenuation);
                float ripple = _RippleActivity * sin(dist * _RippleFrequency - _Time * _RippleSpeed) * _RippleAmplitude * attenuation;

                uv += normalize(uv - rippleOrigin) * ripple;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}