Shader "Custom/Outlined/Dissolve/SurfaceDissolve"
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

        [Header(Outline properties)]
        [Toggle] _UseOutline ("Use Outline", Float) = 1
        [HDR] _OutlineColor ("Outline color", Color) = (0,0,0,1)
		_OutlineWidth ("Outlines width", Range (0.0, 2.0)) = 1.1

        [Header(Dissolve properties)]
        _DirectionDissolve("Direction", vector) = (0,1,0,0)
        _DissolveTexture("Dissolve Tex", 2D) = "white" {} 
        [HDR] _OutlineDissolve ("Outline Dissolve", Color) = (0,0,0,1)
        _Amount("Amount", Range(-1,1)) = 0
        _Step("Step", Float) = 0.01
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
        float3 worldPos : POS;
    };

    sampler2D _MainTex;
    sampler2D _BumpMap;
    sampler2D _MetallicGlossMap;
    sampler2D _EmissiveMap;
    uint _UseOutline;
    half4 _OutlineColor;
    float _OutlineWidth;
    half _Smoothness;
    fixed4 _Color;
    fixed4 _ColorEmissive;
    uint _UseEmissive;
    float _NormalIntensity;   
    uint _UseNormal;
    
    //Dissolve properties
    sampler2D _DissolveTexture;
    float _Amount;
    float _Step; 
    float4 _OutlineDissolve;
    half4 _DirectionDissolve;
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

            //Vertex shader
			v2f vert(appdata v)
			{
				v.vertex.xyz += _OutlineWidth * normalize(v.vertex.xyz) * _UseOutline;
				
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul (unity_ObjectToWorld, v.vertex.xyz); //Position qu'on va utiliser pour le dissolve
				return o;
			}

            //Fragment shader
			half4 frag(v2f i) : COLOR
			{
                //Dissolve function
                float4 dissVal = tex2D(_DissolveTexture, i.uv) * _OutlineColor;
                half4 worldOrientation = mul(unity_ObjectToWorld, _DirectionDissolve);
                half resulDot = (dot(i.worldPos, normalize(worldOrientation))+1)/2;
                clip(resulDot - _Amount); //Discard les pixels positionnés != que dans la direction donnée (dot entre pos du pixels)
                clip(dissVal.r - _Amount); //Discard les pixels selon la textureDissolve
                _OutlineDissolve = _OutlineDissolve * (step(dissVal.r - _Amount, _Step) + step(resulDot - _Amount, _Step)); //Les step déterminent là où la couleur sera utilée, ils renvoient 0 et 1
                if(_OutlineColor.a == 0)
                {
                    discard;
                }
                return _OutlineColor;
			}
			ENDCG
        }

        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0
      
        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_MetallicGlossMap;
            float3 viewDir;
            float3 WorldPos;
        };
      
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input o) 
        {
                UNITY_INITIALIZE_OUTPUT(Input,o);
                o.WorldPos = mul (unity_ObjectToWorld, v.vertex.xyz);
        }
 
        void surf (Input IN, inout SurfaceOutputStandard o) 
        {
            //Dissolve function
            half dissolve_value = tex2D(_DissolveTexture, IN.uv_MainTex).g;
            half4 WorldOrientation = mul(unity_ObjectToWorld, _DirectionDissolve);
            half TestDir = (dot(IN.WorldPos, normalize(WorldOrientation))+1)/2;
            clip(TestDir - _Amount); //Discard les pixels positionnés != que dans la direction donnée (dot entre pos du pixels)
            clip(dissolve_value - _Amount); //Discard les pixels selon la textureDissolve
            _OutlineDissolve = _OutlineDissolve * (step(dissolve_value - _Amount, _Step) + step(TestDir - _Amount, _Step)); //Les step déterminent là où la couleur sera utilée, ils renvoient 0 et 1

            //ALBEDO
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb + _OutlineDissolve;
           
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
                o.Emission = tex2D(_EmissiveMap, IN.uv_MainTex) * _ColorEmissive;
            }
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
 