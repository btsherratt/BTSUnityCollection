Shader "BTS/ZClear" {
    Properties {
    }

    SubShader {
        Tags {
            "Queue" = "Background"
            "RenderType"="Opaque"
        }

        LOD 100

        Pass {
            Cull Front
            ColorMask 0
            //ZTest Always
        }
    }
}
