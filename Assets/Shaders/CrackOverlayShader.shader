Shader "Custom/CrackOverlaySquare"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _CrackTex ("Crack Overlay", 2D) = "black" {}
        _CrackAmount ("Crack Amount", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _MainTex;
        sampler2D _CrackTex;
        float _CrackAmount;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 baseCol = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 crackCol = tex2D(_CrackTex, IN.uv_MainTex);

            float2 centeredUV = abs(IN.uv_MainTex - 0.5);
            float maxUV = max(centeredUV.x, centeredUV.y);
            float mask = smoothstep(_CrackAmount, _CrackAmount - 0.02, maxUV * 2);

            o.Albedo = lerp(baseCol.rgb, crackCol.rgb, mask * crackCol.a);
            o.Alpha = 1;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
