Shader "Custom/Outlined/ToonOutline"
{
    Properties
    {
        [Header(MainTex Properties)]
        _MainTex ("Texture", 2D) = "white" {}
        _ColorMain ("Color MainTex", Color) = (1,1,1,1)

        [Header(Emissive Properties)]
        [Toggle] _UseEmissive("Use Emissive", Float) = 0
        _EmissiveTex ("EmissiveTex", 2D) = "white" {}
        [HDR] _ColorEmissive ("Color Emissive", Color) = (1,1,1,1)
        _RangeEmissive ("Range alpha missive", Range(0,2)) = 0.5
        
        [Header(Light Properties)]
        [HDR] _AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        _SmoothnessLightEdge ("Smoothness light edge", Range(0,1)) = 0
        [Space(5)]
        [Toggle] _UseSpecular("Use Specular", Float) = 1
        [HDR] _SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
        _Glossiness("Glossiness", Float) = 32

        [Header(Rim light Properties)]
        [Toggle] _UseRim("Use Rim", Float) = 0
        [HDR] _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimAmount("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1

        [Header(NormalMap properties)]
        [Toggle] _UseNormal ("Use Normal", Float) = 1
        _NormalIntensity ("Normal Intensity", Range(0, 2)) = 1
		[NoScaleOffset] _NormalMap("NormalMap", 2D) = "white" {}

        [Header(Outline properties)]
        [Toggle] _UseOutline ("Use Outline", Float) = 1
        _OutlineColor ("Outline color", Color) = (0,0,0,1)
		_OutlineWidth ("Outlines width", Range (0.0, 2.0)) = 1.1
        _OutlineOffset ("Outline Offset", Vector) = (0, 0, 0)
        
    }
            CGINCLUDE
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
				float3 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : NORMAL;

                float3 viewDir : TEXCOORD1;
                float3 lightDir : TEXCOORD2;
                
                float3 T : TEXCOORD3;
				float3 B : TEXCOORD4;
				float3 N : TEXCOORD5;
                SHADOW_COORDS(6)
            };

            sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform float4 _ColorMain;

            //Ambient light
            uniform float4 _AmbientColor;
            uniform float _SmoothnessLightEdge;

            //Specular light
            uniform float _Glossiness;
            uniform float4 _SpecularColor;
            uniform float _UseSpecular;

            //NormalMap
            uniform sampler2D _NormalMap;
            uniform float _UseNormal;
            uniform float _NormalIntensity;

            //Rim light
            uniform float4 _RimColor;
            uniform float _RimAmount;
            uniform float _RimThreshold;
            uniform float _UseRim;

            //Emissive
            sampler2D _EmissiveTex;
            uniform float _UseEmissive;
            uniform float4 _ColorEmissive;
            uniform float _RangeEmissive;

            //Outline
            uniform float _OutlineWidth;
            half3 _OutlineOffset;
	        uniform float4 _OutlineColor;
            uniform float _UseOutline;
            ENDCG

    SubShader
    {   
        Tags{"RenderType"="Opaque"}
		Pass //Outline
		{
            Cull Front            
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v)
			{
				//v.vertex.xyz += _OutlineWidth * normalize(v.vertex.xyz) * _UseOutline;
                half3 vertex = v.vertex.xyz;
				vertex -= _OutlineOffset;
				vertex.xyz *= _OutlineWidth +1;
				vertex += _OutlineOffset;
                vertex.xyz *= _UseOutline;

				v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(half4(vertex, v.vertex.w));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			half4 frag(v2f i) : COLOR
			{
                return _OutlineColor;
			}
			ENDCG
        }


        Pass //TT le rest
        {   
            Tags { "RenderType"="Opaque"
                "LightMode" = "ForwardBase"
                "PassFlags" = "OnlyDirectional" 
            } 
            Cull Back
			ZWrite On
			ZTest LEqual
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma multi_compile_fwdbase //Forward base rendering

        v2f vert (appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            o.worldNormal = UnityObjectToWorldNormal(v.normal);

            o.viewDir = WorldSpaceViewDir(v.vertex);
			o.lightDir = normalize(_WorldSpaceLightPos0.xyz);

            // calc Normal, Binormal, Tangent vector in world space
			// cast 1st arg to 'float3x3' (type of input.normal is 'float3')
			float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
			float3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.tangent);

			float3 binormal = cross(v.normal, v.tangent.xyz);
			float3 worldBinormal = mul((float3x3)unity_ObjectToWorld, binormal);

			// and, set them
			o.N = normalize(worldNormal);
			o.T = normalize(worldTangent);
			o.B = normalize(worldBinormal);

            TRANSFER_SHADOW(o);
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {   
            //Sample main texture
            fixed4 col = tex2D(_MainTex, i.uv) * _ColorMain;

            //Light simple (directionnale + ombre)
            float shadow = SHADOW_ATTENUATION(i);
            float3 normal = normalize(i.worldNormal);
            float NdotL = dot(_WorldSpaceLightPos0, normal); // Permet de déterminer à quel point t'es exposé à la dirLight
            float lightIntensity = smoothstep(0, _SmoothnessLightEdge, NdotL * shadow);
            float4 light = lightIntensity * _LightColor0;

            //NORMAL
            // obtain a normal vector on tangent space
			float3 tangentNormal = tex2D(_NormalMap, i.uv);
            tangentNormal.r /= _NormalIntensity;
			// and change range of values (0 ~ 1)
			tangentNormal = normalize(tangentNormal *2 -1);
			// 'TBN' transforms the world space into a tangent space
			// we need its inverse matrix
			// Tip : An inverse matrix of orthogonal matrix is its transpose matrix
			float3x3 TBN = float3x3(normalize(i.T), normalize(i.B), normalize(i.N));
			TBN = transpose(TBN);
			// finally we got a normal vector from the normal map
			float3 worldNormal = mul(TBN, tangentNormal);
            //DIFFUSE
            float3 lightDirection = normalize(i.lightDir);
            float4 diffuse = saturate(dot(worldNormal, -lightDirection));
           // diffuse *= _NormalIntensity;
			diffuse = light * diffuse; //NORMAL MAP
            
            //Specular & Rim lightning
            float4 specular = 0;
            float4 rim = 0;
            float3 viewDir = normalize(i.viewDir);
            
            if(_UseSpecular ==1 )
            {
                float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
                float NdotH = dot(normal, halfVector);
                float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
                float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
                specular = specularIntensitySmooth * _SpecularColor;
            }

            if(_UseRim == 1)
            {
                float4 rimDot = saturate(1 - dot(viewDir, normal));
                float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
                rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
                rim = rimIntensity * _RimColor;
            }

            //Emissive 
            fixed4 emissive = tex2D(_EmissiveTex, i.uv);
            emissive.rgb *= _ColorEmissive.rgb;


                if (emissive.a >= _RangeEmissive && _UseEmissive == 1) //Filtre de la texture emissive via l'alpha
                {
                    return col *(_AmbientColor + diffuse + specular + rim + emissive);
                }
                else
                {
                    if (_UseNormal == 1)
                    {
                        return col *(_AmbientColor + diffuse + specular + rim);
                    }
                    else
                    {
                        return col *(_AmbientColor + light + specular + rim);
                    }
                }   
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER" //Utilise un Pass déjà défini pour générer des ombres
    }
}