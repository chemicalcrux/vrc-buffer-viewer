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
            float4 clipPos : TEXCOORD0;
        };

        v2f vert(appdata v)
        {
            v2f o;
            
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.clipPos = o.vertex;
            
            return o;
        }
        ENDCG   

        GrabPass
        {
            "_StencilPreserve"
        }
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
        GrabPass
        {
            "_StencilViewGrab"
        }
        Pass
        {
            Blend One Zero
            CGPROGRAM
            #pragma fragment frag
            
            SamplerState LinearRepeat;
            Texture2D _StencilPreserve;

            fixed4 frag(v2f i) : SV_Target
            {
                i.clipPos /= i.clipPos.w;
                float4 grabPos = ComputeGrabScreenPos(i.clipPos);
                grabPos /= grabPos.w;

                float4 col = _StencilPreserve.Sample(LinearRepeat, grabPos.xy);
                col.w = 1;
                return col;
            }
            ENDCG
        }
    }
}