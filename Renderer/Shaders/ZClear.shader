Shader "BTS/ZClear"
{
    Properties {
    }

    SubShader {
        Tags {
            "Queue" = "Background"
            "RenderType"="Opaque"
        }

        LOD 100

        Pass {
            Cull Off
            ColorMask 0
            ZTest Always
        }
    }
}
