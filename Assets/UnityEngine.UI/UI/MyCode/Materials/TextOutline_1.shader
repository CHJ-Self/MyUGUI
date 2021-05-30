Shader "MyShaders/TextOutline_1" {
	Properties{
		[PerRendererData] _MainTex("Font Texture", 2D) = "white" {}
		_Color("Text Color", Color) = (1,1,1,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_StencilComp("Stencil Comparison", Float) = 3.000000
		_Stencil("Stencil ID", Float) = 1.000000
		_StencilOp("Stencil Operation", Float) = 0.000000
		_StencilWriteMask("Stencil Write Mask", Float) = 0.000000
		_StencilReadMask("Stencil Read Mask", Float) = 1.000000
		_OutlineWidth ("Outline Width", Range(0,5)) = 1
		_ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 1
	}

		SubShader{

			Tags {
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
			}

			Lighting Off
			Cull Off
			ZTest Always
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask [_ColorMask]

			Stencil {
				Ref[_Stencil]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
				Comp[_StencilComp]
				Pass[_StencilOp]
			}
			
			//第一个Pass，实现Text内容背景颜色，并向外扩大_OutlineWidth
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					//UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float4 worldPosition : TEXCOORD1;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform float4 _MainTex_TexelSize;
				uniform fixed4 _Color;
				uniform fixed4 _OutlineColor;
				float _OutlineWidth;
				float4 _ClipRect;

				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.worldPosition = v.vertex;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color * _Color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}
				//确定每个像素周围像素的坐标。
				static const float2 dirList[121] = {
					float2(-5,-5),float2(-4,-5),float2(-3,-5),float2(-2,-5),float2(-1,-5),float2(0,-5),float2(1,-5),float2(2,-5),float2(3,-5),float2(4,-5),float2(5,-5),
					float2(-5,-4),float2(-4,-4),float2(-3,-4),float2(-2,-4),float2(-1,-4),float2(0,-4),float2(1,-4),float2(2,-4),float2(3,-4),float2(4,-4),float2(5,-4),
					float2(-5,-3),float2(-4,-3),float2(-3,-3),float2(-2,-3),float2(-1,-3),float2(0,-3),float2(1,-3),float2(2,-3),float2(3,-3),float2(4,-3),float2(5,-3),
					float2(-5,-2),float2(-4,-2),float2(-3,-2),float2(-2,-2),float2(-1,-2),float2(0,-2),float2(1,-2),float2(2,-2),float2(3,-2),float2(4,-2),float2(5,-2),
					float2(-5,-1),float2(-4,-1),float2(-3,-1),float2(-2,-1),float2(-1,-1),float2(0,-1),float2(1,-1),float2(2,-1),float2(3,-1),float2(4,-1),float2(5,-1),
					float2(-5,0),float2(-4,0),float2(-3,0),float2(-2,0),float2(-1,0),float2(0,0),float2(1,0),float2(2,0),float2(3,0),float2(4,0),float2(5,0),
					float2(-5,1),float2(-4,1),float2(-3,1),float2(-2,1),float2(-1,1),float2(0,1),float2(1,1),float2(2,1),float2(3,1),float2(4,1),float2(5,1),
					float2(-5,2),float2(-4,2),float2(-3,2),float2(-2,2),float2(-1,2),float2(0,2),float2(1,2),float2(2,2),float2(3,2),float2(4,2),float2(5,2),
					float2(-5,3),float2(-4,3),float2(-3,3),float2(-2,3),float2(-1,3),float2(0,3),float2(1,3),float2(2,3),float2(3,3),float2(4,3),float2(5,3),
					float2(-5,4),float2(-4,4),float2(-3,4),float2(-2,4),float2(-1,4),float2(0,4),float2(1,4),float2(2,4),float2(3,4),float2(4,4),float2(5,4),
					float2(-5,5),float2(-4,5),float2(-3,5),float2(-2,5),float2(-1,5),float2(0,5),float2(1,5),float2(2,5),float2(3,5),float2(4,5),float2(5,5),
				};
				//谋取dirList第dirIndex个方位的透明度值。
				float getDirPosAlpha(float index, float2 xy) {
					float2 curPos = xy;
					float2 dir = dirList[index];
					float2 dirPos = curPos + dir * _MainTex_TexelSize.xy;
					return tex2D(_MainTex, dirPos).a;
				};
				//对于每个像素，传入片元参数v2f i ，获取次像素周围和自身的共9个像素进行透明度叠加。
				//那么得出的结果就是非透明的区域被放大了，形成了黑边。
				float getShadowAlpha(float2 xy) {
					float a = 0;
					int dirWidth = _OutlineWidth * 2 + 1;
					int iMin = 5 - _OutlineWidth;
					int iMax = dirWidth + iMin;
					for(int i = iMin;i < iMax;i++)
					{
						for(int j = iMin;j < iMax;j++)
						{
							a += getDirPosAlpha(11 * i + j, xy);
						}
					}
					a = clamp(a,0,1);
					return a;
				}


				//由于渲染Text内容时，Text字上没有被渲染的区域是透明的，也就是透明度a值是0，
				//所以只要将有内容的区域往外透明度为0的区域扩展一些像素将就能够形成描边效果。
				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = _OutlineColor;
					float2 xy = i.texcoord.xy;
					if(_OutlineWidth > 0)
					{
						col.a *= getShadowAlpha(xy);
					}
					else
					{
						col.a = 0;
					}
					col.a *= UnityGet2DClipping(i.worldPosition, _ClipRect);
					return col;
				}
				ENDCG
			}
			//第二个Pass，常规渲染Text内容。
			
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					//UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float4 worldPosition : TEXCOORD1;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform float4 _MainTex_TexelSize;
				uniform fixed4 _Color;
				float4 _ClipRect;

				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.worldPosition = v.vertex;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color * _Color;
					step(v.texcoord, v.vertex.xy);
					o.texcoord = TRANSFORM_TEX(v.texcoord.xy,_MainTex);
					return o;
				}
				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = i.color;
					col.a = tex2D(_MainTex, i.texcoord).a * UnityGet2DClipping(i.worldPosition, _ClipRect);;
					return col;
				}
				ENDCG
			}
			
		}
}