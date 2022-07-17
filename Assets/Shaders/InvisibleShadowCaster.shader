Shader "Custom/InvisibleShadowCaster"
{
	Subshader
	{
		Tags
		{
			"Queue"="Transparent"
			"RenderType"="Transparent"
			"IgnoreProjector"="True"
		}

		UsePass "VertexLit/SHADOWCASTER"
	}
    FallBack off
}
