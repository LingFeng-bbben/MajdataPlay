Shader "UI/Rect Mask"
{
    Properties
    {
        [PerRendererData]_MainTex ("Texture",2D)="white"{}
        _Color ("Tint", Color) = (1,1,1,1)

        _Width ("Width", Float) = 200
        _Height ("Height", Float) = 100

        _Radius ("Corner Radius", Float) = 20
        _Feather ("Feather", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            fixed4 _Color;

            float _Width;
            float _Height;
            float _Radius;
            float _Feather;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 localPos : TEXCOORD1;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.localPos = v.vertex.xy;
                o.color = v.color * _Color;

                return o;
            }

            float sdRoundRect(float2 p,float2 halfSize,float radius)
            {
                float2 q = abs(p) - halfSize + radius;

                return
                    length(max(q,0))
                    + min(max(q.x,q.y),0)
                    - radius;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex,i.uv) * i.color;

                float2 halfSize =
                    float2(_Width,_Height) * 0.5;

                float d =
                    sdRoundRect(
                        i.localPos,
                        halfSize,
                        _Radius);

                float alpha =
                    saturate((-d) / max(_Feather,0.0001));

                col.a *= alpha;

                return col;
            }

            ENDCG
        }
    }
}