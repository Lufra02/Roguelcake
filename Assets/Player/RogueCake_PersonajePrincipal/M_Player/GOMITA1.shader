Shader "GOMITA1"
{
    Properties
    {
        [MainTexture] _BaseMap ("Gradient / Mask Map (RGB)", 2D) = "white" {}
        _GummyColor ("Body Color (Base)", Color) = (0.2, 0.65, 1.0, 1.0)
        _SecondaryColor ("Extremities Color (Gradient)", Color) = (1.0, 0.4, 0.7, 1.0)
        _IOR ("Index of Refraction (Distortion)", Range(1.0, 1.5)) = 1.15
        _TranslucencyIntensity ("Transmission Intensity", Range(0.0, 3.0)) = 1.5
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.8
        _RimPower ("Edge Definition", Range(0.5, 6.0)) = 2.0
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Name "TransmissionPass"
            Tags { "LightMode" = "UniversalForward" }

            Blend Off
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float4 screenPos    : TEXCOORD2;
                float2 uv           : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _GummyColor;
                float4 _SecondaryColor;
                float _IOR;
                float _TranslucencyIntensity;
                float _Smoothness;
                float _RimPower;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);

                // Lectura de textura para el gradiente
                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // Mezcla los dos colores usando la luminosidad o tono de la textura como máscara
                float mask = texColor.r; 
                float3 blendedGummyColor = lerp(_GummyColor.rgb, _SecondaryColor.rgb, mask);

                // Distorsión por refracción (IOR)
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 distortion = normalWS.xy * (_IOR - 1.0) * 0.15;
                float2 refractedUV = screenUV + distortion;

                // Muestreo del fondo/esqueleto distorsionado
                float3 sceneBehind = SampleSceneColor(refractedUV);

                // Transmisión teñida por el color con gradiente
                float3 transmittedLight = sceneBehind * blendedGummyColor * _TranslucencyIntensity;

                // Especular / brillo exterior
                Light mainLight = GetMainLight();
                float3 halfDir = normalize(normalize(mainLight.direction) + viewDirWS);
                float spec = pow(saturate(dot(normalWS, halfDir)), _Smoothness * 64.0) * _Smoothness;

                // Rim light / borde del líquido
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _RimPower);
                float3 edgeTint = blendedGummyColor * fresnel * 0.8;

                float3 finalResult = transmittedLight + (spec * mainLight.color) + edgeTint;

                return half4(finalResult, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}