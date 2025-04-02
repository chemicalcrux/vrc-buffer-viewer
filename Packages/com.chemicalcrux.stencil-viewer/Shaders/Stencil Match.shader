Shader "Hidden/chemicalcrux/Stencil Viewer/Stencil Match"
{
    Properties
    {
        _StencilRef ("Stencil Ref", Integer) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Overlay" "Queue"="Overlay"
        }
        LOD 100
        ZWrite Off
        
        CGINCLUDE
        #pragma vertex vert

        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            return o;
        }
        ENDCG

        Pass
        {
            Blend One Zero
            
            CGPROGRAM
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
        Pass
        {
            Blend One One
            Stencil
            {
                Ref [_StencilRef]
                ReadMask 255
                Comp Equal
            }
            
            CGPROGRAM
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                return 1;
            }
            ENDCG
        }
    }
}