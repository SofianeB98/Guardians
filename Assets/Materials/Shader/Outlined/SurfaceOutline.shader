Shader "Custom/Outlined/Surface"
{
    
    Properties 
    {    
        [Header(MainTex properties)]
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        [Header(Metallic properties)]
        _MetallicGlossMap("Metallic", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        [Header(NormalMap properties)]
        [Toggle] _UseNormal ("Use Normal", Int) = 1
        _BumpMap("Normal Map", 2D) = "bump" {}
        _NormalIntensity ("Normal intensity", Range(0.01,10)) = 1

        [Header(Emissive properties)]
        [Toggle] _UseEmissive("Use Emissive", Float) = 1
        _EmissiveMap("Emissive map", 2D) = "white" {}
        [HDR] _ColorEmissive ("Emissive Color", Color) = (1,1,1,1)
        _EmissiveIntensity ("Intensity", Range(0,20)) = 1


        [Header(Outline properties)]
        [Toggle] _UseOutline ("Use Outline", Float) = 1
        [HDR] _OutlineColor ("Outline color", Color) = (0,0,0,1)
		_OutlineWidth ("Outlines width", Range (0.0, 2.0)) = 1.1
    }
    CGINCLUDE
    #include "UnityCG.cginc"
    
    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };
    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 pos : SV_POSITION;
    };

    sampler2D _MainTex;
    sampler2D _BumpMap;
    sampler2D _MetallicGlossMap;
    sampler2D _EmissiveMap;
    uint _UseOutline;
    fixed4 _OutlineColor;
    float _OutlineWidth;
    half _Smoothness;
    fixed4 _Color;
    fixed4 _ColorEmissive;
    uint _UseEmissive;
    uniform float _EmissiveIntensity;
    float _NormalIntensity;   
    uint _UseNormal;
    ENDCG

    SubShader 
    {
        Pass
        { //OUTLINE PASS
        Tags{"RenderType"="Opaque"}
        Cull Front            
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v)
			{
				v.vertex.xyz += _OutlineWidth * normalize(v.vertex.xyz) * _UseOutline;
				v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(v.vertex);
              //  o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			half4 frag(v2f i) : COLOR
			{
				return _OutlineColor;
			}
			ENDCG
        }

        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
      
        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_MetallicGlossMap;
            float3 viewDir;
        };
      
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
 
        void surf (Input IN, inout SurfaceOutputStandard o) 
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
          
            //METALLIC
            fixed4 cMetal = tex2D(_MetallicGlossMap, IN.uv_MetallicGlossMap);
            o.Metallic = cMetal.rgb;
            o.Smoothness = cMetal.a * _Smoothness; 
                    
            //NORMAL
            if(_UseNormal == 1)
            {
                fixed3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
                normal.xy *= _NormalIntensity;
                o.Normal = normal;
            }

            //EMISSIVE
            if(_UseEmissive == 1)
            {
                o.Emission = tex2D(_EmissiveMap, IN.uv_MainTex) * _ColorEmissive *_EmissiveIntensity;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
 