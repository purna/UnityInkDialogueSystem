Shader "Custom/SennaarToonShader2D"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayTiling ("Overlay Tiling", Vector) = (1,1,0,0)
        _OverlayBlend ("Overlay Blend Factor", Range(0,1)) = 0.5
        [Enum(Multiply,0,Overlay,1,Screen,2,Additive,3,SoftLight,4)] _OverlayBlendMode ("Overlay Blend Mode", float) = 1
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.2, 0.2, 0.2, 1)
        _MidtoneColor ("Midtone Color", Color) = (0.5, 0.5, 0.5, 1)
        _Threshold1 ("Shadow Threshold 1", Range(0,1)) = 0.3
        _Threshold2 ("Shadow Threshold 2", Range(0,1)) = 0.6
        _Smoothness ("Shadow Softness", Range(0,1)) = 0.1
        _OverlayMaxSize ("Max Overlay Size", Range(0, 100)) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "LightMode" = "Universal2D" }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 overlayUV : TEXCOORD1;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_OverlayTex);
            SAMPLER(sampler_OverlayTex);
            float4 _OverlayTiling;
            float _OverlayBlend;
            float _OverlayBlendMode;
            float _OverlayMaxSize;
            
            float4 _Color;
            float4 _ShadowColor;
            float4 _MidtoneColor;
            float _Threshold1;
            float _Threshold2;
            float _Smoothness;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                
                // Apply tiling to overlay UVs, without stretching
                o.overlayUV = v.uv * _OverlayTiling.xy;
                
                // Tile the overlay texture correctly
                o.overlayUV = frac(o.overlayUV); // This ensures the overlay texture repeats

                // Optionally scale overlay UVs based on the maximum size
                o.overlayUV *= _OverlayMaxSize; // Limit the maximum size of the overlay texture
                
                return o;
            }
            
            float3 BlendOverlay(float3 base, float3 overlay)
            {
                return (base < 0.5) ? (2.0 * base * overlay) : (1.0 - 2.0 * (1.0 - base) * (1.0 - overlay));
            }
            
            float3 BlendScreen(float3 base, float3 overlay)
            {
                return 1.0 - ((1.0 - base) * (1.0 - overlay));
            }
            
            float3 BlendSoftLight(float3 base, float3 overlay)
            {
                return (overlay < 0.5) ? (base - (1.0 - 2.0 * overlay) * base * (1.0 - base)) : (base + (2.0 * overlay - 1.0) * (sqrt(base) - base));
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;
                float4 overlayColor = SAMPLE_TEXTURE2D(_OverlayTex, sampler_OverlayTex, i.overlayUV);
                
                if (texColor.a == 0) discard;
                
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normal = float3(0, 0, 1);
                float diff = saturate(dot(normal, lightDir));
                
                float shadowFactor = smoothstep(_Threshold1 - _Smoothness, _Threshold1 + _Smoothness, diff);
                float midtoneFactor = smoothstep(_Threshold2 - _Smoothness, _Threshold2 + _Smoothness, diff);
                
                float3 shadedColor = lerp(_ShadowColor.rgb, _MidtoneColor.rgb, shadowFactor);
                float3 finalColor = lerp(shadedColor, texColor.rgb, midtoneFactor);
                
                float3 blendedOverlay = finalColor;
                if (_OverlayBlendMode == 0) blendedOverlay = finalColor * overlayColor.rgb;
                else if (_OverlayBlendMode == 1) blendedOverlay = BlendOverlay(finalColor, overlayColor.rgb);
                else if (_OverlayBlendMode == 2) blendedOverlay = BlendScreen(finalColor, overlayColor.rgb);
                else if (_OverlayBlendMode == 3) blendedOverlay = finalColor + overlayColor.rgb;
                else if (_OverlayBlendMode == 4) blendedOverlay = BlendSoftLight(finalColor, overlayColor.rgb);
                
                finalColor = lerp(finalColor, blendedOverlay, _OverlayBlend);
                
                return float4(finalColor, texColor.a);
            }
            ENDHLSL
        }
    }
}
