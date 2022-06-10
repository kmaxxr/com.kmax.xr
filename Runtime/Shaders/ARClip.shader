// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ARClip"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		   _Color("Color",Color) = (1,1,1,1)
		 _LeftTop("leftTop",Vector) = (0,0,0,0)
		 _RightTop("rightTop",Vector) = (0,0,0,0)
		 _LeftBottom("leftBottom",Vector) = (0,0,0,0)
		 _RightBottom("rightBottom",Vector) = (0,0,0,0)
		 _LeftTop_uv("ltu",Vector) = (0,0,0,0)
		 _RightTop_uv("tru",Vector) = (0,0,0,0)
		 _LeftBottom_uv("lbu",Vector) = (0,0,0,0)
		 _RightBottom_uv("rbu",Vector) = (0,0,0,0)
		 _WindowPos("wp",Vector) = (0,0,0,0)
	}
		SubShader{
			Pass{
			  ZTest Always Cull Off ZWrite Off

			   CGPROGRAM


							 #pragma vertex vert
							 #pragma fragment frag
							 #include "UnityCG.cginc"
							 sampler2D _MainTex;
							 sampler2D _CustomDepthTexture;
								 float4 _LeftTop;
								 float4 _RightTop;
								 float4 _LeftBottom;
								 float4 _RightBottom;
								 float4 _LeftTop_uv;
								 float4 _RightTop_uv;
								 float4 _LeftBottom_uv;
								 float4 _RightBottom_uv;
								 float4x4 _InvProjectionMatrix; //通过'camera.projectionMatrix.inverse'传入
								 float4x4 _ViewToWorld; //通过'camera.cameraToWorldMatrix'传递它
								 float4x4 _WindowMatrix;
								 struct v2f {
									 float4 pos : SV_POSITION;
									 half2 uv: TEXCOORD0;
									 float4 viewDir : TEXCOORD5;
								 };

								 float Cross(float2 a, float2 b)
								 {
									 return a.x * b.y - b.x * a.y;
								 }

								 float IsPointInRectangle(float2 P, float2 A, float2 B, float2 C, float2 D)
								 {
									 float2 AB = A - B;
									 float2 AP = A - P;
									 float2 CD = C - D;
									 float2 CP = C - P;

									 float2 DA = D - A;
									 float2 DP = D - P;
									 float2 BC = B - C;
									 float2 BP = B - P;

									 float isBetweenAB_CD = 1 - step(Cross(AB,AP) * Cross(CD,CP),0);
									 float isBetweenDA_BC = 1 - step(Cross(DA,DP) * Cross(BC,BP),0);
									 return isBetweenAB_CD * isBetweenDA_BC;
								 }



								 v2f vert(appdata_img v) {
									 v2f o;
									 o.pos = UnityObjectToClipPos(v.vertex);
									 o.uv = v.texcoord;
									 o.viewDir = mul(_InvProjectionMatrix, float4 (o.uv * 2.0 - 1.0, 1.0, 1.0));
									 return o;
								 }




			fixed4 frag(v2f i) :COLOR{

							float depth = Linear01Depth(DecodeFloatRGBA(tex2D(_CustomDepthTexture, i.uv)));
							fixed4 final;
							//Perspective divide and scale by depth to get view-space position
							float3 viewPos = (i.viewDir.xyz / i.viewDir.w) * depth;							
							float3 worldPos = mul(_ViewToWorld, float4 (viewPos, 1));					
							float3 worldPos_ = mul(_WindowMatrix, float4(worldPos, 1));
							float one = step(worldPos_.z ,0);//(worldPos_.z < 0)
							float two = IsPointInRectangle(i.uv.xy, _LeftTop_uv.xy, _RightTop_uv.xy, _RightBottom_uv.xy, _LeftBottom_uv.xy);
							final = one * tex2D(_MainTex, i.uv) + (1 - one)*two* tex2D(_MainTex, i.uv);
						   float4 col = float4(depth, depth, depth, 1);
						   return   final;
					   }

					   ENDCG
				   }


		   }
		   }
			   // FallBack "Diffuse"
			
