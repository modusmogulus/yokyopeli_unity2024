﻿Shader "PSX/Vertex Lit Divine"
{
    Properties
    {
        _Color("Color (RGBA)", Color) = (1, 1, 1, 1)
        _EmissionColor("Emission Color (RGBA)", Color) = (0,0,0,0)
        _CubemapColor("Cubemap Color (RGBA)", Color) = (0,0,0,0)
        _MainTex("Texture", 2D) = "white" {}
        _EmissiveTex("Emissive", 2D) = "black" {}
        _Cubemap("Cubemap", Cube) = "" {}
        _ReflectionMap("Reflection Map", 2D) = "white" {}
        _ObjectDithering("Per-Object Dithering Enable", Range(0,1)) = 1
        _FlatShading("Flat Shading", Range(0,1)) = 0
        _CustomDepthOffset("Custom Depth Offset", Float) = 0
    }
        SubShader
    {
        Tags {"RenderType" = "Opaque" }
        ZWrite On
        LOD 100
		
		//Vertex lighting requires two passes to be defined. One used with lightmaps and one without.
        Pass
        {
            Tags { "LightMode" = "VertexLM" }
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_geometry __ PSX_ENABLE_CUSTOM_VERTEX_LIGHTING
            #pragma multi_compile_geometry __ PSX_FLAT_SHADING_MODE_CENTER
            #pragma multi_compile PSX_TRIANGLE_SORT_OFF PSX_TRIANGLE_SORT_CENTER_Z PSX_TRIANGLE_SORT_CLOSEST_Z PSX_TRIANGLE_SORT_CENTER_VIEWDIST PSX_TRIANGLE_SORT_CLOSEST_VIEWDIST PSX_TRIANGLE_SORT_CUSTOM

            #include "UnityCG.cginc"
            #include "PSX-Utils.cginc"

			samplerCUBE _Cubemap;
            sampler2D _ReflectionMap;
			float4 _CubemapColor;
			
            #define PSX_VERTEX_LIT
			#define PSX_CUBEMAP _Cubemap
			#define PSX_CUBEMAP_COLOR _CubemapColor
			
            #include "PSX-ShaderSrc.cginc"

        ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "Vertex" }
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_geometry __ PSX_ENABLE_CUSTOM_VERTEX_LIGHTING
            #pragma multi_compile_geometry __ PSX_FLAT_SHADING_MODE_CENTER
            #pragma multi_compile PSX_TRIANGLE_SORT_OFF PSX_TRIANGLE_SORT_CENTER_Z PSX_TRIANGLE_SORT_CLOSEST_Z PSX_TRIANGLE_SORT_CENTER_VIEWDIST PSX_TRIANGLE_SORT_CLOSEST_VIEWDIST PSX_TRIANGLE_SORT_CUSTOM

            #include "UnityCG.cginc"
            #include "PSX-Utils.cginc"

			samplerCUBE _Cubemap;
            sampler2D _ReflectionMap;
			float4 _CubemapColor;
			
            #define PSX_VERTEX_LIT
			#define PSX_CUBEMAP _Cubemap
			#define PSX_CUBEMAP_COLOR _CubemapColor
			
            #include "PSX-ShaderSrc.cginc"
        ENDCG
        }
    }
        Fallback "PSX/Lite/Vertex Lit"
}