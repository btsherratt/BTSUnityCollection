Shader "Hidden/QADNERPortal" {
    Properties {
        _PortalID("Portal Identifier", Int) = 1
    }

    SubShader {
        Tags {
            "Queue" = "Geometry+100"
            "RenderType" = "Opaque"
        }

        LOD 100

        Pass {
            //Cull Off
            ColorMask 0
            //ZWrite Off

            Stencil {
                Ref [_PortalID]
                Comp Always
                Pass Replace
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target {
                return half4(1,0,0,1);
            }

            ENDCG
        }
    }
}
