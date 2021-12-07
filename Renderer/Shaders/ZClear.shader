Shader "BTS/ZClear"
{
    Properties {
    }

    SubShader {
        Tags { "RenderType"="Opaque-100" }
        LOD 100

        Pass {
            Cull Off
            ColorMask 0
            ZTest Always
        }
    }
}
