// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TransparentShader"
{
   Properties{
		_Color ("Tint", Color) = (0, 0, 0, 1)
		_MainTex ("Texture", 2D) = "white" {}
        _ShadowIntensity ("Shadow Intensity", Range (0, 1)) = 0.6
	}

	SubShader{
		Tags{ "RenderType"="Transparent" "Queue"="AlphaTest+50"}

		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;

			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v){
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET{
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Color;
				return col;
			}

			ENDCG
		}
		 Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
 
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            uniform float _ShadowIntensity;
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                LIGHTING_COORDS(0,1)
            };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
               
                return o;
            }
            fixed4 frag(v2f i) : COLOR
            {
                float attenuation = LIGHT_ATTENUATION(i);
                return fixed4(0,0,0,(1-attenuation)*_ShadowIntensity);
            }
            ENDCG
        }
	}
        Fallback "VertexLit"
}
