Shader "Hidden/chemicalcrux/Stencil Viewer/Display Result"
{
    Properties
    {
        _Opacity ("Opacity", Range(0, 1)) = 1
        _StencilRef ("Stencil Ref", Integer) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Overlay" "Queue"="Overlay+1001"
        }
        LOD 100
        ZWrite Off
        ZTest Always

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            SamplerState LinearRepeat;
            Texture2D _StencilViewGrab;

            float _Opacity;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 clipPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.clipPos = o.vertex;
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                i.clipPos /= i.clipPos.w;
                float4 grabPos = ComputeGrabScreenPos(i.clipPos);
                grabPos /= grabPos.w;

                float4 col = _StencilViewGrab.Sample(LinearRepeat, grabPos.xy);
                col.w = _Opacity;
                
                return col;
            }
            ENDCG
        }
    }
}