Shader "Custom/Sprite-Lit-CustomTile"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _OverlayTex("Overlay", 2D) = "white" {}
        _OverlayTiling("Overlay Tiling", Vector) = (1, 1, 0, 0)
        _OverlayBlend("Overlay Blend Factor", Range(0, 1)) = 0.5
        //_OverlayBlendMode("Overlay Blend Mode", Float) = 1 // Use float to represent different blend modes
        [Enum(Multiply,0,Overlay,1,Screen,2,Additive,3,SoftLight,4)] _OverlayBlendMode ("Overlay Blend Mode", float) = 1
        _Color("Tint", Color) = (1, 1, 1, 1)
        _RendererColor("Renderer Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 overlayUV : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_OverlayTex);
            SAMPLER(sampler_OverlayTex);
            half4 _OverlayTiling;
            float _OverlayBlend;
            float _OverlayBlendMode; // Using float to store the blend mode
            float4 _Color;
            half4 _RendererColor;

            // Define blend modes using integers, which we will map in the fragment shader
            const int Multiply = 0;
            const int Overlay = 1;
            const int Screen = 2;
            const int Additive = 3;
            const int SoftLight = 4;

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Position transformation (no need for _MainTex_ST here)
                o.positionCS = TransformObjectToHClip(v.positionOS);

                // Use original UV for the main texture
                o.uv = v.uv;

                // Apply tiling for the overlay texture
                o.overlayUV = v.uv * _OverlayTiling.xy;

                return o;
            }

            half4 BlendOverlayMode(half4 baseColor, half4 overlayColor, float blendFactor, float mode)
            {
                // Map the mode to the appropriate blend logic
                if (mode == Multiply)
                {
                    return baseColor * overlayColor * blendFactor;
                }
                else if (mode == Overlay)
                {
                    return lerp(baseColor, overlayColor, blendFactor);
                }
                else if (mode == Screen)
                {
                    return baseColor + overlayColor * (1.0 - baseColor) * blendFactor;
                }
                else if (mode == Additive)
                {
                    return baseColor + overlayColor * blendFactor;
                }
                else if (mode == SoftLight)
                {
                    return lerp(baseColor, overlayColor, blendFactor * 0.5);
                }
                else
                {
                    return baseColor; // Default to base color if no valid mode is selected
                }
            }

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                // Sample the main texture
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;

                // Sample the overlay texture using the custom tiled UV
                half4 overlayTexColor = SAMPLE_TEXTURE2D(_OverlayTex, sampler_OverlayTex, i.overlayUV);

                // Apply the selected blend mode
                half4 finalColor = BlendOverlayMode(mainTexColor, overlayTexColor, _OverlayBlend, _OverlayBlendMode);

                return finalColor;
            }

            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
