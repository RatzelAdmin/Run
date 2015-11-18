Shader "Toon/Lighted Outline_dora" {
Properties {
  _Color ("Main Color", Color) = (0.5,0.5,0.5,1)
  _OutlineColor ("Outline Color", Color) = (0,0,0,1)
  _Outline ("Outline width", Range (.002, 0.05)) = .005
  _MainTex ("Base (RGB)", 2D) = "white" {}
  _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
  _Dist ("Hide OutLine", Float) = 1.5
 }
 
SubShader {
  Tags { "RenderType"="Opaque" }
  UsePass "Toon/Lighted/FORWARD"
        Pass {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
CGPROGRAM
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct appdata members vertex,normal)
#pragma exclude_renderers xbox360
 
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
 
struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};

 
struct v2f {
    float4 pos : POSITION;
    float4 color : COLOR;
    float fog : FOGC;
};
uniform float _Outline;
uniform float4 _OutlineColor;
uniform float 	_Dist;
 
v2f vert(appdata v) {
    v2f o;
    
    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    float3 norm = mul ((float3x3)UNITY_MATRIX_MV, v.normal);
    norm.x *= UNITY_MATRIX_P[0][0];
    norm.y *= UNITY_MATRIX_P[1][1];
    
	float dist = distance(_WorldSpaceCameraPos, mul(_Object2World, v.vertex));		
	if(dist > _Dist)
	{
	    o.pos.xy += norm.xy * _Outline;
	}
	
    o.fog = o.pos.z;
    o.color = _OutlineColor;
    return o;
}

half4 frag (v2f i) : COLOR
{
    return i.color;
}
ENDCG
            Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
            SetTexture [_MainTex] { combine primary }
        }
    }
   
    Fallback "Toon/Lighted Outline"
}
