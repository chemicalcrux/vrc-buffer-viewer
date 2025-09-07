Shader "Hidden/chemicalcrux/Buffer Viewer/Display Result"
{
    Properties
    {
        _Opacity ("Opacity", Range(0, 1)) = 1
        [Enum(Normal, 0, Multiply, 1)] _BlendMode ("Blend Mode", Float) = 0
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

        GrabPass
        {
            "_SceneColor"
        }
        Pass
        {
            Blend One Zero

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            SamplerState LinearRepeat;

            int _BlendMode;

            Texture2D _BufferViewGrab;
            Texture2D _SceneColor;

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

                float4 sceneColor = _SceneColor.Sample(LinearRepeat, grabPos.xy);

                float4 col = _BufferViewGrab.Sample(LinearRepeat, grabPos.xy);

                float4 resultColor;

                switch (_BlendMode)
                {
                case 0:
                    resultColor = col;
                    break;
                case 1:
                    resultColor = sceneColor * col;
                    break;
                default:
                    resultColor = 0;
                    break;
                }
                
                return lerp(sceneColor, resultColor, _Opacity);
            }
            ENDCG
        }
    }
}