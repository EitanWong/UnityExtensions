Shader "MagicLightProbes/VolumeBounds2" {
   Properties
    {
        _Color ("Color", Color) = (0, 1, 0, 0.5)
        _Color2 ("Color Inside Geometry", Color) = (0, 0.7, 0, 0.5)
    }
    SubShader
    {
     Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
	 //inner cube faces outside geometry
      Pass
        {
		Stencil {
		  Ref 4
		  Comp always
		  Pass replace
		  ZFail keep
		}

			Blend SrcAlpha OneMinusSrcAlpha
			Cull front // draw front faces
 			ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float3 cameraVector : TEXCOORD1;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = (UnityObjectToWorldNormal(v.normal));
				o.cameraVector = (WorldSpaceViewDir(v.vertex));	
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 cameraVector = normalize(i.cameraVector);
				float3 normal = normalize(i.normal);
 				float view = saturate(dot(-normal, cameraVector));
				view = lerp(view, 1, 0.3);


                return fixed4(_Color.rgb * view, _Color.a)  * 0.7; // make color darker
            }
            ENDCG
        } 

	 //outer cube faces outside geometry    
		Pass
        {
		Stencil {
		  Ref 4
		  Comp always
		  Pass replace
		  ZFail keep
		}

			Blend SrcAlpha OneMinusSrcAlpha
			Cull back // draw front faces
 			ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float3 cameraVector : TEXCOORD1;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = (UnityObjectToWorldNormal(v.normal));
				o.cameraVector = (WorldSpaceViewDir(v.vertex));	
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 cameraVector = normalize(i.cameraVector);
				float3 normal = normalize(i.normal);
 				float view = saturate(dot(normal, cameraVector));
				view = lerp(view, 1, 0.3);


                return fixed4(_Color.rgb * view, _Color.a); // make color darker
            }
            ENDCG
        }
	 //inner side cube faces inside geometry    
      Pass
        {
			Stencil 
			{
			  Ref 2
			  Comp Greater
			  Fail keep
			  Pass replace
			}

			Cull front // draw front faces
			ZWrite Off
			//ZTest Off
			Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

           struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float3 cameraVector : TEXCOORD1;
            };

            float4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = (UnityObjectToWorldNormal(v.normal));
				o.cameraVector = (WorldSpaceViewDir(v.vertex));	
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 cameraVector = normalize(i.cameraVector);
				float3 normal = normalize(i.normal);
 				float view = saturate(dot(-normal, cameraVector));
				view = lerp(view, 1, 0.3);


                return fixed4(_Color2.rgb * view, _Color2.a)  * 0.7; // make color darker
            }
            ENDCG
        } 

		//outer side cube faces inside geometry    
		Pass
        {
			Stencil
			{
			  Ref 3
			  Comp Greater
			  Fail keep
			  Pass replace
			}

			Cull back // draw front faces
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

           struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float3 cameraVector : TEXCOORD1;
            };

            float4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = (UnityObjectToWorldNormal(v.normal));
				o.cameraVector = (WorldSpaceViewDir(v.vertex));	
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 cameraVector = normalize(i.cameraVector);
				float3 normal = normalize(i.normal);
 				float view = saturate(dot(normal, cameraVector));
				view = lerp(view, 1, 0.3);


                return fixed4(_Color2.rgb * view, _Color2.a); // make color darker
            }
            ENDCG
        }
    }
}
