Shader "Custom/SennaarToonShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.2, 0.2, 0.2, 1)
        _MidtoneColor ("Midtone Color", Color) = (0.5, 0.5, 0.5, 1)
        _Threshold1 ("Shadow Threshold 1", Range(0,1)) = 0.3
        _Threshold2 ("Shadow Threshold 2", Range(0,1)) = 0.6
        _Smoothness ("Shadow Softness", Range(0,1)) = 0.1
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
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
            };
            
            float4 _Color;
            float4 _ShadowColor;
            float4 _MidtoneColor;
            float _Threshold1;
            float _Threshold2;
            float _Smoothness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float diff = dot(i.normal, lightDir);
                
                // Soft banded shading effect
                float shadowFactor = smoothstep(_Threshold1 - _Smoothness, _Threshold1 + _Smoothness, diff);
                float midtoneFactor = smoothstep(_Threshold2 - _Smoothness, _Threshold2 + _Smoothness, diff);
                
                float3 shadedColor = lerp(_ShadowColor.rgb, _MidtoneColor.rgb, shadowFactor);
                float3 finalColor = lerp(shadedColor, _Color.rgb, midtoneFactor);
                
                return float4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
