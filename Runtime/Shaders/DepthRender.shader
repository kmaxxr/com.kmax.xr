Shader "Custom/CustomDepthTexture" {
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata {
				float4 vertex : POSITION;				
			};
			struct v2f {
				float4 vertex : SV_POSITION;
				float3 view : TEXCOORD2;
				float depth: TEXCOORD1;
				
			};
		
			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.view = UnityObjectToViewPos(v.vertex);
				o.depth = o.vertex.z / o.vertex.w;
				
				return o;
			}
			fixed4 frag(v2f i) : SV_Target {
				
				float4 depth = EncodeFloatRGBA(i.depth);
				return depth;
			}
			ENDCG
		}
	}
}

