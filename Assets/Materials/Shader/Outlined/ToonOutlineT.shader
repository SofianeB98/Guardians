Shader "Custom/Outlined/ToonTransparent"
{
    Properties {

        [Header(MainTex properties)]
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _Color ("Main Color", Color) = (.5,.5,.5,1)
		[Toggle] _UseLight ("Use Light", Int) = 1
		_Alpha ("Alpha", Range(0,1)) = 1

        [Header(Emissive Properties)]
        [Toggle] _UseEmissive("Use Emissive", Float) = 1
        _EmissiveTex ("EmissiveTex", 2D) = "white" {}
        [HDR] _ColorEmissive ("Color Emissive", Color) = (1,1,1,1)
		_EmissiveIntensity ("Intensity", Range(0,20)) = 1
        _RangeEmissive ("Range alpha missive", Range(0,2)) = 0.5

		[Header(NormalMap properties)]
        [Toggle] _UseNormal ("Use Normal", Int) = 1
        _NormalIntensity ("Normal Intensity", Range(-2, 2)) = 1
		[NoScaleOffset] _NormalMap("NormalMap", 2D) = "white" {}

		[Header(Outline properties)]
        [Toggle] _UseOutline ("Use Outline", Float) = 1
        [Toggle] _UseTransparencyOutline ("Use transparency on Outline", Float) = 1
		[HDR] _OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (0.0, 0.15)) = .005
		_OutlineOffset ("Outline Offset", Vector) = (0, 0, 0)
	}
 
	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Lighting.cginc"

		struct appdata 
		{
			half4 vertex : POSITION;
			half3 normal : NORMAL;
			half2 texcoord : TEXCOORD0;
			float3 tangent : TANGENT;
		};
		 
		struct v2f 
		{
			half4 pos : POSITION;
			half2 uv : TEXCOORD0;
			half3 normalDir : NORMAL;

			float3 viewDir : TEXCOORD1;
            float3 lightDir : TEXCOORD2;
                
            float3 T : TEXCOORD3;
			float3 B : TEXCOORD4;
			float3 N : TEXCOORD5;
		};
		 
		uniform half4 _Color;
		uniform half _Outline;
		uniform half4 _OutlineColor;
        uniform uint _UseOutline;
		uniform half _UseTransparencyOutline;

        //Emissive
        sampler2D _EmissiveTex;
        uniform uint _UseEmissive;
        uniform float4 _ColorEmissive;
        uniform float _RangeEmissive;
		uniform float _EmissiveIntensity;


		//NormalMap
        uniform sampler2D _NormalMap;
        uniform uint _UseNormal;
        uniform float _NormalIntensity;

		//Light
		uniform uint _UseLight;
	ENDCG
 
	SubShader {
		Tags { "Queue" = "Transparent" }
 
 		Pass {
			Name "STENCIL"
			ZWrite Off
			ZTest Always
			ColorMask 0

			Stencil {
                Ref 2
                Comp always
                Pass replace
                ZFail decrWrap
            }
			
			CGPROGRAM

			#pragma vertex vert2
			#pragma fragment frag
						
			v2f vert2 (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
		
			half4 frag (v2f i) : COLOR
			{
				return _Color;
			}
			ENDCG
		}
 		
		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Off
			ZWrite Off
			ColorMask RGB
 
			Blend SrcAlpha OneMinusSrcAlpha
			
			Stencil {
                Ref 2
                Comp NotEqual
                Pass replace
                ZFail decrWrap
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			half3 _OutlineOffset;
			
			v2f vert(appdata v) 
			{
				v2f o;
				half3 vertex = v.vertex.xyz;
				vertex -= _OutlineOffset;
				vertex.xyz *= _Outline +1 * _UseOutline;
				vertex += _OutlineOffset;
				o.pos = UnityObjectToClipPos(half4(vertex, v.vertex.w));
				return o;
			}
			
			half _Alpha;
            
			half4 frag(v2f i) :COLOR {
                if (_UseTransparencyOutline == 1)
                {
                    return half4(_OutlineColor.rgb, _Alpha);
                }
                else 
                {
                    return half4 (_OutlineColor);
                }
			}
			ENDCG
		}
 
		Pass {
			Name "BASE"
			ZWrite Off
			ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert2
			#pragma fragment frag
			#pragma multi_compile_fwdbase //Forward base rendering
						
			v2f vert2 (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.normalDir = UnityObjectToWorldNormal(v.normal);

				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.lightDir = normalize(_WorldSpaceLightPos0.xyz);

				float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
				float3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.tangent);

				float3 binormal = cross(v.normal, v.tangent.xyz);
				float3 worldBinormal = mul((float3x3)unity_ObjectToWorld, binormal);

				// and, set them
				o.N = normalize(worldNormal);
				o.T = normalize(worldTangent);
				o.B = normalize(worldBinormal);
				return o;
			}
			
			uniform sampler2D _MainTex;
			half _Alpha;
			
			half4 frag (v2f i) : COLOR
			{
				//Light 
				float3 normal = normalize(i.normalDir);
            	float NdotL = dot(_WorldSpaceLightPos0, normal); // Permet de déterminer à quel point t'es exposé à la dirLight
            	float lightIntensity = smoothstep(0, 0.01, NdotL);
            	float4 light = lightIntensity * _LightColor0;

				//Normal
				float3 tangentNormal = tex2D(_NormalMap, i.uv);
				tangentNormal.r /= _NormalIntensity;
				tangentNormal = normalize(tangentNormal *2 -1);
				float3x3 TBN = float3x3(normalize(i.T), normalize(i.B), normalize(i.N));
				TBN = transpose(TBN);
				float3 worldNormal = mul(TBN, tangentNormal);
				//DIFFUSE
           	 	float3 lightDirection = normalize(i.lightDir);
            	float4 diffuse = saturate(dot(worldNormal, -lightDirection)); //NORMAL MAP
				
				half3 tex = tex2D(_MainTex, i.uv).rgb;
				half3 color =  tex * _Color.rgb;

				//Emissive 
            	fixed4 emissive = tex2D(_EmissiveTex, i.uv);
            	emissive.rgb *= _ColorEmissive.rgb * _EmissiveIntensity;
				
				if (_UseNormal == 1)
				{
					color *= diffuse;
				}
				if(_UseLight == 1)
				{
					color *= light;
				}
				if (emissive.a >= _RangeEmissive && _UseEmissive == 1) //Filtre de la texture emissive via l'alpha
            	{
					color.rgb += emissive.rgb;
				}

			return half4 (color.rgb, _Alpha);
			}			
			ENDCG
		}
	}
	Fallback Off
}