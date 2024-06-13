Shader "SyncTextureShaderDXT/IdentityCRTShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Update"

            CGPROGRAM
            #pragma fragment frag
            
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 frag (v2f_customrendertexture i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.localTexcoord.xy);
                return col;
            }
            ENDCG
        }
    }
}
