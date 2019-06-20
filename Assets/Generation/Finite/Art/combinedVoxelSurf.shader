﻿Shader "Custom/combinedVoxelSurf"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_GrassColor("Grass Color", Color) = (1,1,1,1)
		_DirtColor("Dirt Color", Color) = (1,1,1,1)
		_StoneColor("Stone Color", Color) = (1,1,1,1)
		_BedrockColor("Bedrock Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		#include "Assets\Packages\RetroAA\RetroAA.cginc"

        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
		float4 _MainTex_ST;
		float4 _MainTex_TexelSize;

		struct VertIn {
			float4 vertex    : POSITION;
			float3 normal    : NORMAL;
			float4 texcoord  : TEXCOORD0;
			float texcoord1 : TEXCOORD1;
		};
        struct Input
        {
			float2 block_uv;
			fixed4 block_col;
        };

        half _Glossiness, _Metallic;
        fixed4 _GrassColor, _DirtColor, _StoneColor, _BedrockColor;

		void vert(inout VertIn v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			fixed block_id = round(v.texcoord1);

			switch (block_id) {
				case 1: o.block_col = _GrassColor; break;
				case 2: o.block_col = _DirtColor; break;
				case 3: o.block_col = _StoneColor; break;
				case 4: o.block_col = _BedrockColor; break;
			}

			float3 absNorm = abs(v.normal);
			float2 uv = 0.5;
			if (absNorm.y > 0.5) uv += v.vertex.xz;
			else if (absNorm.x > 0.5) uv += v.vertex.yz;
			else uv += v.vertex.xy;
			o.block_uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;
		}

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
			fixed4 c = IN.block_col * RetroAA(_MainTex, IN.block_uv, _MainTex_TexelSize);
			
			//c = fixed4(IN.normal,1);

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
