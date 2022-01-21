Shader "Hidden/BTS/VectorLine/Basic" {
    Properties {
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Vertex {
                float3 position;
                float4 color;
            };

            #include "UnityCG.cginc"

            StructuredBuffer<Vertex> vertices;

            struct v2f {
                float4 position : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID) {
                UNITY_SETUP_INSTANCE_ID(inst);

                v2f o;

                o.position = UnityObjectToClipPos(float4(vertices[id].position, 1.0f));
                o.color = vertices[id].color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target{
                fixed4 col = i.color;
                return col;
            }

            ENDCG
        }
    }
}
