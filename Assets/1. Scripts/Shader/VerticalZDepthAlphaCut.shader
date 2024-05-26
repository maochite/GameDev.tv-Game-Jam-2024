
Shader "Universal Render Pipeline/Custom/VerticalZDepthAlphaCut" 
{
	Properties 
	{
		[MainTexture] _MainTex("Main Texture (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
		[MainColor]   _BaseColor("Base Color", Color) = (1, 1, 1, 1)

		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		[Toggle(_EMISSION)] _Emission ("Emission", Float) = 0
		[NoScaleOffset]_EmissionMap("Emission Map", 2D) = "white" {}

		_Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
	}

	SubShader 
	{

		Tags 
		{
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Opaque"
			"Queue"="Geometry"
		}
		
		Cull Off
		//ZTest Always

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		CBUFFER_START(UnityPerMaterial)
		float4 _MainTex_ST;
		float4 _BaseColor;
		float4 _EmissionColor;
		float _Cutoff;
		CBUFFER_END
		ENDHLSL


		Pass 
		{
			Name "ForwardLit"
			Tags { "LightMode"="UniversalForward" }

			
			HLSLPROGRAM
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment

			// Material Keywords
			#pragma shader_feature_local_fragment _EMISSION
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			//#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

			// URP Keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			// Note, v11 changes this to :
			// #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING // v10+ only, renamed from "_MIXED_LIGHTING_SUBTRACTIVE"
			#pragma multi_compile _ SHADOWS_SHADOWMASK // v10+ only

			// Unity Keywords
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile_fog

			// GPU Instancing (not supported)
			//#pragma multi_compile_instancing

			// Includes
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			// Structs
			struct Attributes 
			{
				float4 positionOS	: POSITION;
				float4 normalOS		: NORMAL;
				float2 uv		    : TEXCOORD0;
				float2 lightmapUV	: TEXCOORD1;
				float4 color		: COLOR;
				float4 tangentOS     : TANGENT;
				//UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings 
			{
				float4 positionCS 					: SV_POSITION;
				float2 uv		    				: TEXCOORD0;
				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
				float3 positionWS					: TEXCOORD2;
				half3 normalWS					: TEXCOORD3;
				
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					half4 fogFactorAndVertexLight	: TEXCOORD6; // x: fogFactor, yzw: vertex light
				#else
					half  fogFactor					: TEXCOORD6;
				#endif
				//#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord 				: TEXCOORD7;
				//#endif
				float4 color						: COLOR;
				//UNITY_VERTEX_INPUT_INSTANCE_ID
				//UNITY_VERTEX_OUTPUT_STEREO
			};

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

			// Textures, Samplers & Global Properties
			// (note, BaseMap, BumpMap and EmissionMap is being defined by the SurfaceInput.hlsl include)
			//TEXTURE2D(_SpecGlossMap); 	SAMPLER(sampler_SpecGlossMap);


			//  SurfaceData & InputData
			void InitalizeSurfaceData(Varyings IN, out SurfaceData surfaceData)
			{
				surfaceData = (SurfaceData)0; // avoids "not completely initalized" errors

				half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);


				clip(baseMap.a - _Cutoff);


				half4 diffuse = baseMap * _BaseColor * IN.color;
				surfaceData.albedo = diffuse.rgb;
				//surfaceData.normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
				surfaceData.emission = SampleEmission(IN.uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
				surfaceData.occlusion = 1.0; // unused
				//
			}

			void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData) 
			{
				inputData = (InputData)0; // avoids "not completely initalized" errors

				inputData.positionWS = input.positionWS;


				half3 viewDirWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
				inputData.normalWS = input.normalWS;

				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);

				viewDirWS = SafeNormalize(viewDirWS);
				inputData.viewDirectionWS = viewDirWS;

				//#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				inputData.shadowCoord = input.shadowCoord;
				//#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
				//	inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				//#else
				//	inputData.shadowCoord = float4(0, 0, 0, 0);
				//#endif

				// Fog
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.fogCoord = input.fogFactorAndVertexLight.x;
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#else
					inputData.fogCoord = input.fogFactor;
					inputData.vertexLighting = half3(0, 0, 0);
				#endif

				/* in v11/v12?, could use :
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#else
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactor);
					inputData.vertexLighting = half3(0, 0, 0);
				#endif
				*/

				inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
				inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
			}

			float rayPlaneIntersection( float3 rayDir, float3 rayPos, float3 planeNormal, float3 planePos)
            {
                float denom = dot(planeNormal, rayDir);
                denom = max(denom, 0.000001); // avoid divide by zero
                float3 diff = planePos - rayPos;
                return dot(diff, planeNormal) / denom;
            }

			float BillboardVerticalZDepthVert(Attributes IN, inout Varyings OUT)
			{
				// billboard mesh towards camera
				float3 vpos = mul((float3x3)unity_ObjectToWorld, IN.positionOS.xyz);
				float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
    
				// view to clip space
				OUT.positionCS = mul(UNITY_MATRIX_P, viewPos);
    
				// calculate distance to vertical billboard plane seen at this vertex's screen position
				float3 planeNormal = normalize(float3(UNITY_MATRIX_V._m20, 0.0, UNITY_MATRIX_V._m22));
				float3 planePoint = unity_ObjectToWorld._m03_m13_m23;
				float3 rayStart = _WorldSpaceCameraPos.xyz;
				float3 rayDir = -normalize(mul(UNITY_MATRIX_I_V, float4(viewPos.xyz, 1.0)).xyz - rayStart); // convert view to world, minus camera pos
				float dist = rayPlaneIntersection(rayDir, rayStart, planeNormal, planePoint);

				// calculate the clip space z for vertical plane
				float4 planeOutPos = mul(UNITY_MATRIX_VP, float4(rayStart + rayDir * dist, 1.0));
				float newPosZ = planeOutPos.z / planeOutPos.w * OUT.positionCS.w;
    
				// use the closest clip space z
				#if defined(UNITY_REVERSED_Z)
				newPosZ = max(OUT.positionCS.z, newPosZ);
				#else
				newPosZ = min(OUT.positionCS.z, newPosZ);
				#endif

				return newPosZ;
			}

			float4 RotateAroundYInRads(float4 vertex, float angleRad)
            {
                float sina, cosa;
                sincos(angleRad, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }

			float CameraRotation(float3 tangentWS)
			{
				half3 worldTangent = mul(unity_ObjectToWorld, float4(tangentWS.x, tangentWS.y, tangentWS.z, 0)).xyz;
				//half3 worldTangent = UnityObjectToWorldDir(tangentOS);
				half3 viewTangent = mul((float3x3) UNITY_MATRIX_V, worldTangent);
				return atan2(viewTangent.z, viewTangent.x);
			}

			// Vertex Shader
			Varyings LitPassVertex(Attributes IN) 
			{
				Varyings OUT = (Varyings)0;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				// billboard mesh towards camera
				float3 vpos = mul((float3x3)unity_ObjectToWorld, IN.positionOS.xyz);
				float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
    
				// view to clip space
				OUT.positionCS = mul(UNITY_MATRIX_P, viewPos);
    
				// calculate distance to vertical billboard plane seen at this vertex's screen position
				float3 planeNormal = normalize(float3(UNITY_MATRIX_V._m20, 0.0, UNITY_MATRIX_V._m22));
				float3 planePoint = unity_ObjectToWorld._m03_m13_m23;
				float3 rayStart = _WorldSpaceCameraPos.xyz;
				float3 rayDir = -normalize(mul(UNITY_MATRIX_I_V, float4(viewPos.xyz, 1.0)).xyz - rayStart); // convert view to world, minus camera pos
				float dist = rayPlaneIntersection(rayDir, rayStart, planeNormal, planePoint);

				// calculate the clip space z for vertical plane
				float4 planeOutPos = mul(UNITY_MATRIX_VP, float4(rayStart + rayDir * dist, 1.0));
				float newPosZ = planeOutPos.z / planeOutPos.w * OUT.positionCS.w;
    
				// use the closest clip space z
				#if defined(UNITY_REVERSED_Z)
				newPosZ = max(OUT.positionCS.z, newPosZ);
				#else
				newPosZ = min(OUT.positionCS.z, newPosZ);
				#endif

				OUT.positionCS.z = newPosZ;

				float3 forward = UNITY_MATRIX_V[2].y;

				VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);
				
				float cameraAngleRad = CameraRotation(normalInputs.tangentWS);
				float4 rotatedOS = RotateAroundYInRads(IN.positionOS, cameraAngleRad);

				VertexPositionInputs positionInputs = GetVertexPositionInputs(rotatedOS);


				half3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
				half3 vertexLight = VertexLighting(positionInputs.positionWS, normalInputs.normalWS);
				half fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

				OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);

				OUT.positionWS = positionInputs.positionWS;

				OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, OUT.lightmapUV);
				OUTPUT_SH(OUT.normalWS.xyz, OUT.vertexSH);

				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					OUT.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				#else
					OUT.fogFactor = fogFactor;
				#endif

				OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);

				
				OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
				OUT.color = IN.color;

				return OUT;
			}

			// Fragment Shader
			half4 LitPassFragment(Varyings IN) : SV_Target 
			{
				//UNITY_SETUP_INSTANCE_ID(IN);
				//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				// Setup SurfaceData
				SurfaceData surfaceData;
				InitalizeSurfaceData(IN, surfaceData);

				// Setup InputData
				InputData inputData;
				InitializeInputData(IN, surfaceData.normalTS, inputData);

				// Simple Lighting (Lambert & BlinnPhong)
				half4 color = UniversalFragmentPBR(inputData, surfaceData);(inputData, surfaceData); // v12 only
				//half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData.albedo, half4(surfaceData.specular, 1), 
				//surfaceData.smoothness, surfaceData.emission, surfaceData.alpha);
				// See Lighting.hlsl to see how this is implemented.
				// https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl

				color.rgb = MixFog(color.rgb, inputData.fogCoord);
				//color.a = OutputAlpha(color.a, _Surface);

				return color;
			}
			ENDHLSL
		}
	}
}
