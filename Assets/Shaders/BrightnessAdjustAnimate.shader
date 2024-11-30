Shader "Custom/BrightnessAdjustAnimate"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}
			Pass
				{
			ZTest Always
			Cull Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			sampler2D _MainTex;
			half _Brightness;
			half _Saturation;
			half _Contrast;

			//vert和frag函数
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"


			struct appdata_t
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			//从vertex shader传入pixel shader的参数
			struct v2f
			{
				float4 pos : SV_POSITION; //顶点位置
				half2  uv : TEXCOORD0;	  //UV坐标
				half4 color : COLOR;
			};

			//vertex shader
			v2f vert(appdata_t v)
			{
				v2f o;
				//从自身空间转向投影空间
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				//uv坐标赋值给output
				o.uv = v.texcoord;
				return o;
			}

			//fragment shader
			fixed4 frag(v2f i) : COLOR
			{
				//从_MainTex中根据uv坐标进行采样
				fixed4 renderTex = tex2D(_MainTex, i.uv)*i.color;
				float brightness = 1;
				brightness = clamp( sin(_Time * 260) * 0.2, 0, 1);
				//brigtness亮度直接乘以一个系数，也就是RGB整体缩放，调整亮度
				fixed3 finalColor = renderTex + brightness;
				//返回结果，alpha通道不变
				return fixed4(finalColor, renderTex.a);
			}
			ENDCG
		}
	}
	//防止shader失效的保障措施
	FallBack Off
}