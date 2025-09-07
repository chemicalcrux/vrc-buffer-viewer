Shader "Hidden/chemicalcrux/Buffer Viewer/Depth View"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Overlay" "Queue"="Transparent"
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
            "_BufferPreserve"
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
            CGPROGRAM
            #pragma fragment frag

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            bool _ShowFarPlane;

            float inverse_lerp(float from, float to, float value)
            {
                return (value - from) / (to - from);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                i.clipPos /= i.clipPos.w;
                float4 grabPos = ComputeGrabScreenPos(i.clipPos);
                grabPos /= grabPos.w;

                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, grabPos.xy);

                if (depth == 0 && _ShowFarPlane)
                {
                    float2 uv = grabPos;
                    uv.x *= _ScreenParams.x / _ScreenParams.y;
                    return (uv.x + uv.y) % 0.02 < 0.01 ? float4(0.4, 0, 0, 1) : float4(0.5, 0, 0, 1);
                }

                depth = LinearEyeDepth(depth);
                depth = inverse_lerp(0, 25, depth);

                return float4(depth.xxx, 1);
            }
            ENDCG
        }
        GrabPass
        {
            "_BufferViewGrab"
        }
        Pass
        {
            Blend One Zero
            CGPROGRAM
            #pragma fragment frag

            SamplerState LinearRepeat;
            Texture2D _BufferPreserve;

            fixed4 frag(v2f i) : SV_Target
            {
                i.clipPos /= i.clipPos.w;
                float4 grabPos = ComputeGrabScreenPos(i.clipPos);
                grabPos /= grabPos.w;

                float4 col = _BufferPreserve.Sample(LinearRepeat, grabPos.xy);
                col.w = 1;
                return col;
            }
            ENDCG
        }
    }
}