Shader "BTS/Null" {
    Properties{
    }

    SubShader{
        Tags {
            "RenderType" = "Opaque"
        }

        // This is really bad.
        // I want it for testing only.
        // Please don't actually use this.
        Pass {
            ColorMask 0
            ZWrite Off
        }
    }
}
