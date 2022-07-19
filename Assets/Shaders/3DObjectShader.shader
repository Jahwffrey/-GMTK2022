//AUTHOR: CherryShroom
Shader "Custom/3DObject"
{
	Properties
	{
		_Color( "Color", Color ) = ( 1,1,1,1 )
		_Emission( "Emission", Color ) = (0,0,0,0)
		_MainTex( "Albedo (RGB)", 2D ) = "white" {}
		_AttenRamp( "Attenuation Ramp", 2D ) = "white" {}
		_DiffuseRamp( "Diffuse Ramp" , 2D ) = "white" {}
		_Cutoff( "Alpha cutoff", Range( 0,1 ) ) = 0.5
		_SpecularIntensity( "Specular Intensity", Range( 0, 1 ) ) = 0
		_SpecularCutoff( "Specular Cutoff", Range( 0, 1 ) ) = 0.9
	}
	SubShader
	{
		Tags {
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Stepped fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _AttenRamp;
		sampler2D _DiffuseRamp;
		fixed4 _Color;
		fixed4 _Emission;
		fixed _SpecularIntensity;
		fixed _SpecularCutoff;

		struct Input
		{
			float2 uv_MainTex;
		};

		float4 LightingStepped( SurfaceOutput s, float3 lightDir, half3 viewDir, float shadowAttenuation )
		{
			half dotProduct = dot( s.Normal, lightDir );
			half dotProductInRange = ( dotProduct + 1.0 ) / 2.0;
			half attenFixed = min( shadowAttenuation, 0.9 );
			half3 aR = tex2D( _AttenRamp, float2( attenFixed, 0 ) ).rgb;
			half3 dR = tex2D( _DiffuseRamp, float2( dotProductInRange, 0 ) ).rgb;
			half3 finalRamp = float3( min( aR.r, dR.r ), min( aR.g, dR.g ), min( aR.b, dR.b ) );
			
			half specDot = dot( viewDir, reflect( -lightDir, s.Normal ) ) * 0.5 + 0.5;
			fixed spec = 0;
			if( specDot > _SpecularCutoff )
			{
				spec = _SpecularIntensity;
			}

			#ifdef USING_DIRECTIONAL_LIGHT
			finalRamp = ( finalRamp + _LightColor0.rgb ) / 2;
			#endif
			return float4( ( s.Albedo * _LightColor0.rgb * finalRamp ) + ( _LightColor0.rgb * spec ), s.Alpha ) + _Emission;
		}

		void surf( Input IN, inout SurfaceOutput o )
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D( _MainTex, IN.uv_MainTex ) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			clip( c.a - 1 );
		}
		ENDCG
	}
	FallBack "Diffuse"
}