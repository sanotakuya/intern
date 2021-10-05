Shader "Custom/culloff" {
	Properties {
		_MainTex ("RGB", 2D) = "white" {}
	}
	SubShader {
		Tags { "LightMode" = "Always" "RenderType"="Opaque" }
		LOD 100
		Cull off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma surface surf Lambert ambient

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
