
Shader "Particles/Additive_2side_off" {
	Properties {
		_TintColor ("TintColor", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Base", 2D) = "white" {}

	}
	
	CGINCLUDE

		#include "UnityCG.cginc"
		fixed4 _TintColor;
		sampler2D _MainTex;
		
		
		half4 _MainTex_ST;
						
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
		};

		v2f vert(appdata_full v) {
			v2f o;
			
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);	
			o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
					
			return o; 
		}
		
		fixed4 frag( v2f i ) : COLOR {	
			return tex2D (_MainTex, i.uv.xy) * _TintColor;
		}
	
	ENDCG
	
	SubShader {
		Tags { "RenderType" = "Transparent" "Reflection" = "RenderReflectionTransparentAdd" "Queue" = "Transparent"}
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend one one
		
	Pass {
	
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}
				
	} 
	FallBack Off
}