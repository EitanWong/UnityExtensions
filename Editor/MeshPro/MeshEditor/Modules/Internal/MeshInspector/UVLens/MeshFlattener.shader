Shader "Hidden/UVLens/MeshFlattener"
{ 
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

		Pass
		{
			Blend off
			ZWrite off
			ZTest off
			Cull off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature _ _UV2

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				#if _UV2
				float2 texcoord : TEXCOORD1;
				#else
				float2 texcoord : TEXCOORD0;
				#endif
			};

			struct v2f {				
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			fixed4 _Color;

			v2f vert (appdata_t v)
			{
				v2f o;
				v.vertex.x = v.texcoord.x;
				v.vertex.y = v.texcoord.y;
				v.vertex.z = 0;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = _Color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG  
		}  
	}
}
