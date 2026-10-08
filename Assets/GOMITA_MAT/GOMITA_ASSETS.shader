Shader "Custom/GOMITA_ASSETS"

{
    Properties
    {
        _GummyColor ("Gummy Tint (Transmission Color)", Color) = (0.2, 0.65, 1.0, 1.0)
        _Opacity ("Gummy Density / Opacity", Range(0.0, 1.0)) = 0.5
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
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float4 screenPos    : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _GummyColor;
                float _Opacity;
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
                return output;
            }

            half4 frag(Varyings input, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                // Si se dibuja la cara interior, invertimos la normal hacia la cámara
                normalWS = isFrontFace ? normalWS : -normalWS;

                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);

                // Cálculo de refracción
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 distortion = normalWS.xy * (_IOR - 1.0) * 0.15;
                float2 refractedUV = screenUV + distortion;

                // Leer el esqueleto y fondo
                float3 sceneBehind = SampleSceneColor(refractedUV);

                // Mezcla de transparencia:
                // Si _Opacity es 0, deja pasar el fondo intacto (agua clara).
                // Si _Opacity es 1, absorbe el fondo y predomina el color de la gomita.
                float3 coloredTransmission = sceneBehind * _GummyColor.rgb * _TranslucencyIntensity;
                float3 transmittedLight = lerp(coloredTransmission, _GummyColor.rgb * _TranslucencyIntensity * 0.5, _Opacity * 0.5);

                // Luz principal y reflejo exterior
                Light mainLight = GetMainLight();
                float3 halfDir = normalize(normalize(mainLight.direction) + viewDirWS);
                float spec = pow(saturate(dot(normalWS, halfDir)), _Smoothness * 64.0) * _Smoothness;

                // Borde Fresnel más o menos denso según la opacidad
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _RimPower);
                float3 edgeTint = _GummyColor.rgb * fresnel * (_Opacity * 0.8 + 0.2);

                float3 finalResult = transmittedLight + (spec * mainLight.color) + edgeTint;

                return half4(finalResult, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}