Shader "Roystan/ToonMeh"
{//no da sombra a otros objetos con el shader, la sombra no cambia cuando cambias el color de la luz
	Properties
	{
		_Color("Color", Color) = (0.5, 0.65, 1, 1)
		_AmbientColor("Ambient Color", Color) = (0.4, 0.4, 0.4, 1)
		_MainTex("Main Texture", 2D) = "white" {}	
		_RampTex("Gradient Texture", 2D) = "white" {}	
		_SpecularColor("Specular Color", Color) = (0.9, 0.9, 0.9, 1)
		_Glossiness("Glossiness", Float) = 32
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0,1)) = 0.716
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
	}
	SubShader
	{

		Pass
		{
			Tags
			{
				"RenderPipeline" = "UniversalPipeline"
				"LightMode" = "UniversalForward"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float3 normal : NORMAL;
				float4 vertex : POSITION;				
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float3 viewDir : TEXCOORD1;
				float3 worldNormal : NORMAL;
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(2)
			};

			float4 _MainTex_ST;
			float4 _AmbientColor;
			sampler2D _MainTex;
			sampler2D _RampTex;  //echar un ojo
			float _Glossiness;
			float4 _SpecularColor;
			float4 _RimColor;
			float _RimAmount;
			float _RimThreshold;
			
			
			v2f vert (appdata v)
			{
				v2f o;
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal (v.normal);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				TRANSFER_SHADOW(o)
				return o;
			}
			
			float4 _Color;

			float4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float NdotL = dot(_WorldSpaceLightPos0, normal);
				Float shadow = SHADOW_ATTENUATION(i);

				float lightIntensity = smoothstep (0, 0.01,NdotL * shadow);
				float4 light = lightIntensity * _LightColor0;
				float2 uv = float2(1- (NdotL * 0.5 + 0.5), 0.5);
				float3 viewDir = normalize(i.viewDir);
				
				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVector);
				float specularIntensity = pow (NdotH * lightIntensity, _Glossiness * _Glossiness); 
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float4 specular = specularIntensitySmooth * _SpecularColor;
				float4 rimDot = 1 - dot(viewDir, normal);
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01,_RimAmount + 0.01, rimIntensity);
				float4 rim = rimIntensity * _RimColor;
				float4 sample = tex2D(_MainTex, i.uv);

				return _Color * sample * (_AmbientColor + light + specular + rim);


			}
			ENDCG
		}
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}