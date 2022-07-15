// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//AUTHOR: CherryShroom
Shader "Custom/RockShader"
{
	Properties
	{
		_Color( "Color", Color ) = ( 1,1,1,1 )
		_Emission( "Emission", Color ) = (0,0,0,0)
		_MainTex( "Albedo (RGB)", 2D ) = "white" {}
		_AttenRamp( "Attenuation Ramp", 2D ) = "white" {}
		_DiffuseRamp( "Diffuse Ramp" , 2D ) = "white" {}
		_Noise( "Noise", 2D ) = "black" {}
		_TexScale( "Texture Scale", Range(1,100)) = 1
		_OffScale( "Offset Scale", Range(0,1)) = 0.0012
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
		#pragma surface surf Stepped vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _AttenRamp;
		sampler2D _DiffuseRamp;
		sampler2D _Noise;
		fixed4 _Color;
		fixed4 _Emission;
		fixed _SpecularIntensity;
		fixed _SpecularCutoff;
		fixed _TexScale;
		fixed _OffScale;

		struct Input
		{
			float2 uv_MainTex;
		};

		float4 LightingStepped( SurfaceOutput s, float3 lightDir, half3 viewDir, float shadowAttenuation )
		{
			half dotProduct = dot( s.Normal, lightDir );
			half dotProductInRange = ( dotProduct + 1.0 ) / 2.0;
			half attenFixed = min( shadowAttenuation, 0.9999999 );
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
			return float4( ( s.Albedo * _LightColor0.rgb * finalRamp ) + ( _LightColor0.rgb * spec ), s.Alpha ) + _Emission;;
		}

        void vert (inout appdata_full v, out Input o)
        {
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float offScale = 0.011;
			float offset = tex2Dlod( _Noise, float4(worldPos.x / _TexScale % 1, worldPos.z / _TexScale % 1, 0, 0)).x;
			float offset2 = tex2Dlod( _Noise, float4(worldPos.z / _TexScale % 1, worldPos.x / _TexScale % 1, 0, 0)).x;
			v.vertex.z += (offset + offset2)/2 * v.vertex.z / 5;
			v.vertex.x += (offset* 2 - 1) * _OffScale;
			v.vertex.y += (offset2 * 2 - 1) * _OffScale;
			
			UNITY_INITIALIZE_OUTPUT(Input, o);
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