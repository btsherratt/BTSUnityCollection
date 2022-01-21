using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class VectorLinePipeline : RenderPipeline {
    struct Vertex {
        public Vector3 position;
        public Vector4 color;

        public Vertex(VectorLineVertex vlvtx) {
            position = vlvtx.position;
            color = vlvtx.color;
        }
    }

    public interface IVertexProvider {
        bool IsDirty { get; }
        int VertexCount { get; }
        IEnumerator<VectorLineVertex> Vertices();
    }

    struct CachedData {
        public ComputeBuffer m_vertexBuffer;
        public MaterialPropertyBlock m_materialProperties;
    }

    /*public abstract class VertexProvider : MonoBehaviour, IVertexProviding {
        public abstract IEnumerator<Vertex> Vertices();

        public abstract int VertexHash();
    }*/

    public static VectorLinePipeline Current { get; private set; }

    VectorLinePipelineAsset m_asset;
//    ComputeBuffer m_primitiveBuffer;
    Material m_renderMaterial;

    Dictionary<int, CachedData> m_cachedShapeData;

    /*struct ComputeBufferContext : IVectorLineDrawContext {
        ComputeBuffer m_computeBuffer;

        uint m_currentIdx;
        Color m_currentColor;

        public ComputeBufferContext(ComputeBuffer computeBuffer, uint startIdx) {
            m_computeBuffer = computeBuffer;
            m_currentIdx = startIdx;
            m_currentColor = Color.white;
        }

        public void Vertex(Vector3 position) {
            m_computeBuffer.




        //        Unity.Collections.NativeArray<Vertex> vertices = m_primitiveBuffer.BeginWrite<Vertex>(0, 1024);
        Vertex[] vertices = new Vertex[1024];
            vertices[0] = new Vertex(Vector3.zero, Color.white);
            vertices[1] = new Vertex((Vector3.left + Vector3.up)/* * 100.0, Color.white);
            vertices[2] = new Vertex((Vector3.right + Vector3.up) * 100.0f, Color.white);
            m_primitiveBuffer.SetData(vertices);
            //m_primitiveBuffer.EndWrite<Vertex>(2);
        }
    }*/

    public VectorLinePipeline(VectorLinePipelineAsset asset) {
        Current = this;
        m_asset = asset;

        //new GraphicsBuffer(GraphicsBuffer.Target.Index)

        //m_primitiveBuffer = new ComputeBuffer(1024, Marshal.SizeOf(typeof(Vertex)), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
        
        m_renderMaterial = new Material(Shader.Find("Hidden/BTS/VectorLine/Basic"));
        //m_renderMaterial.SetBuffer("vertices", m_primitiveBuffer);

        m_cachedShapeData = new Dictionary<int, CachedData>();
    }

    ~VectorLinePipeline() {
        foreach (CachedData data in m_cachedShapeData.Values) {
            data.m_vertexBuffer.Release();
        }
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        // Rebuild data here probably...

        //CommandBuffer commandBuffer = new CommandBuffer();

        foreach (Camera camera in cameras) {
            context.SetupCameraProperties(camera);

            CommandBuffer cameraCommandBuffer = new CommandBuffer();
            cameraCommandBuffer.ClearRenderTarget(camera.clearFlags == CameraClearFlags.SolidColor, true, camera.backgroundColor);

            foreach (VectorLineDrawable drawable in VectorLineDrawable.All(true)) {
                int drawableID = drawable.DrawableID;

                CachedData cachedData;
                bool dataAvailable = m_cachedShapeData.TryGetValue(drawableID, out cachedData);

                if (drawable.IsDirty) {
                    if (dataAvailable) {
                        cachedData.m_vertexBuffer.Release();
                    }
                    dataAvailable = false;
                }

                if (dataAvailable == false) {
                    cachedData = new CachedData();

                    Vertex[] vertices = new Vertex[drawable.VertexCount];
                    int idx = 0;
                    foreach (VectorLineVertex vertex in drawable.GetVertices()) {
                        if (idx >= vertices.Length) {
                            break;
                        }

                        vertices[idx] = new Vertex(vertex);
                        idx += 1;
                    }

                    cachedData.m_vertexBuffer = new ComputeBuffer(vertices.Length, Marshal.SizeOf(typeof(Vertex)), ComputeBufferType.Default); //new GraphicsBuffer(GraphicsBuffer.Target.Vertex, vertices.Length, Marshal.SizeOf(typeof(Vertex)));
                    cachedData.m_vertexBuffer.SetData(vertices);

                    cachedData.m_materialProperties = new MaterialPropertyBlock();
                    cachedData.m_materialProperties.SetBuffer("vertices", cachedData.m_vertexBuffer);

                    m_cachedShapeData[drawableID] = cachedData;
                }

                cameraCommandBuffer.DrawProcedural(drawable.transform.localToWorldMatrix, m_renderMaterial, 0, MeshTopology.Lines, cachedData.m_vertexBuffer.count, 1, cachedData.m_materialProperties);
            }

            context.ExecuteCommandBuffer(cameraCommandBuffer);
            cameraCommandBuffer.Release();

            if (camera.cameraType == CameraType.SceneView) {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }

            context.Submit();
        }

        //commandBuffer.Release();
        //context.Submit();


#if false
        CommandBuffer commandBuffer = new CommandBuffer();

        commandBuffer.ClearRenderTarget(true, true, cameras[0].backgroundColor);

        context.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Release();

        foreach (Camera camera in cameras) {
            ScriptableCullingParameters cullingParameters;
            camera.TryGetCullingParameters(out cullingParameters);

            CullingResults cullingResults = context.Cull(ref cullingParameters);

            context.SetupCameraProperties(camera);


            CommandBuffer cameraCommandBuffer = new CommandBuffer();

            VisibleLight primaryLight = new VisibleLight();
            //Debug.Log("--");
            foreach (VisibleLight light in cullingResults.visibleLights) {
                //Debug.Log(light.light.name, light.light);
                if (light.lightType == LightType.Directional) {
                    primaryLight = light;
                    //break;
                }
            }

            //foreach (VisibleLight visibleLight in cullingResults.visibleLights) {
            //     cameraCommandBuffer.SetGlobalVector("_LightColor0", visibleLight.finalColor);
            //     cameraCommandBuffer.SetGlobalVector("_WorldSpaceLightPos0", visibleLight.light.transform.forward);


            //    break;
            //}

            cameraCommandBuffer.SetGlobalVector("_LightColor0", /*primaryLight.finalColor*/Vector3.one);
            cameraCommandBuffer.SetGlobalVector("_WorldSpaceLightPos0", Vector3.up /*- primaryLight.light.transform.forward*/);
            cameraCommandBuffer.SetGlobalMatrix("unity_WorldToLight", primaryLight.light.transform.worldToLocalMatrix);

            cameraCommandBuffer.SetGlobalVector("unity_4LightPosX0", Vector4.zero);
            cameraCommandBuffer.SetGlobalVector("unity_4LightPosY0", Vector4.zero);
            cameraCommandBuffer.SetGlobalVector("unity_4LightPosZ0", Vector4.zero);
            cameraCommandBuffer.SetGlobalVector("unity_4LightAtten0", Vector4.zero);
            cameraCommandBuffer.SetGlobalMatrix("unity_LightColor", Matrix4x4.zero);

            cameraCommandBuffer.SetGlobalVector("unity_AmbientSky", RenderSettings.ambientSkyColor);
            cameraCommandBuffer.SetGlobalVector("unity_AmbientEquator", RenderSettings.ambientEquatorColor);
            cameraCommandBuffer.SetGlobalVector("unity_AmbientGround", RenderSettings.ambientGroundColor);
            cameraCommandBuffer.SetGlobalVector("UNITY_LIGHTMODEL_AMBIENT", RenderSettings.ambientSkyColor);

            context.ExecuteCommandBuffer(cameraCommandBuffer);
            cameraCommandBuffer.Release();

            {
                // Tell Unity which geometry to draw, based on its LightMode Pass tag value
                ShaderTagId shaderTagId = new ShaderTagId("ForwardBase");

                // Tell Unity how to sort the geometry, based on the current Camera
                var sortingSettings = new SortingSettings(camera);

                // Create a DrawingSettings struct that describes which geometry to draw and how to draw it
                DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);

                // Tell Unity how to filter the culling results, to further specify which geometry to draw
                // Use FilteringSettings.defaultValue to specify no filtering
                FilteringSettings filteringSettings = FilteringSettings.defaultValue;

                // Schedule a command to draw the geometry, based on the settings you have defined
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            }

            /*{
                // Tell Unity which geometry to draw, based on its LightMode Pass tag value
                ShaderTagId shaderTagId = new ShaderTagId("FORWARDADD");

                // Tell Unity how to sort the geometry, based on the current Camera
                var sortingSettings = new SortingSettings(camera);

                // Create a DrawingSettings struct that describes which geometry to draw and how to draw it
                DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);

                // Tell Unity how to filter the culling results, to further specify which geometry to draw
                // Use FilteringSettings.defaultValue to specify no filtering
                FilteringSettings filteringSettings = FilteringSettings.defaultValue;

                // Schedule a command to draw the geometry, based on the settings you have defined
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            }*/





            // Schedule a command to draw the Skybox if required
            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null) {
                context.DrawSkybox(camera);
            }

            if (camera.cameraType == CameraType.SceneView) {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }

            context.Submit();
        }
#endif
    }
}