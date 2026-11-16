Shader "ROAD RUSH/Desert Sky"
{
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct SkyVertex { float4 vertex : POSITION; };
            struct SkyFragment { float4 position : SV_POSITION; float3 direction : TEXCOORD0; };
            SkyFragment vert(SkyVertex input)
            {
                SkyFragment output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.direction = input.vertex.xyz;
                return output;
            }
            half4 frag(SkyFragment input) : SV_Target
            {
                float3 direction = normalize(input.direction);
                float h = saturate(direction.y);
                half3 horizon = half3(.70, .36, .27);
                half3 evening = half3(.22, .27, .44);
                half3 zenith = half3(.055, .105, .21);
                half3 sky = lerp(horizon, evening, smoothstep(0.005, .25, h));
                sky = lerp(sky, zenith, smoothstep(.19, .9, h));
                float solarDistance = distance(direction, normalize(float3(.34, .104, .96)));
                sky += half3(.36, .15, .05) * exp(-solarDistance * 8.0);
                sky = lerp(sky, half3(1, .76, .43), 1 - smoothstep(.037, .042, solarDistance));
                return half4(sky, 1);
            }
            ENDHLSL
        }
    }
}
