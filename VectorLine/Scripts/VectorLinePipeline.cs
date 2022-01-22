using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class VectorLinePipeline : RenderPipeline {
    const int MAX_DRAWABLE_VERTEX_COUNT = 1024;

    static Vertex[] ms_vertices;

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

    VectorLinePipelineAsset m_asset;
    Material m_renderMaterial;

    Dictionary<int, CachedData> m_cachedShapeData;

    public VectorLinePipeline(VectorLinePipelineAsset asset) {
        if (ms_vertices == null) {
            ms_vertices = new Vertex[MAX_DRAWABLE_VERTEX_COUNT];
        }

        m_asset = asset;
        LoadMaterials();
        m_cachedShapeData = new Dictionary<int, CachedData>();
    }

    protected override void Dispose(bool disposing) {
        foreach (CachedData data in m_cachedShapeData.Values) {
            data.m_vertexBuffer.Release();
        }

        base.Dispose(disposing);
    }

    void LoadMaterials() {
        m_renderMaterial = new Material(m_asset.m_basicShader);
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
#if UNITY_EDITOR
        // Safety for a weird editor bug that happens when player is built...
        if (m_renderMaterial == null) {
            LoadMaterials();
        }
#endif

        // Rebuild data here probably...

        foreach (Camera camera in cameras) {
            context.SetupCameraProperties(camera);

            CommandBuffer cameraCommandBuffer = new CommandBuffer();
            cameraCommandBuffer.ClearRenderTarget(camera.clearFlags == CameraClearFlags.SolidColor, true, camera.backgroundColor);

            foreach (VectorLineDrawable drawable in VectorLineDrawable.All(true)) {
                int drawableID = drawable.DrawableID;

                if (drawableID != VectorLineDrawable.INVALID_ID) {
                    CachedData cachedData;
                    bool dataAvailable = m_cachedShapeData.TryGetValue(drawableID, out cachedData);

                    if (dataAvailable == false || drawable.IsDirty) {
                        int vertexCount = drawable.VertexCount;

                        if (vertexCount > 0) {
                            if (dataAvailable == false) {
                                cachedData = new CachedData();
                                cachedData.m_materialProperties = new MaterialPropertyBlock();
                            }

                            int idx = 0;
                            foreach (VectorLineVertex vertex in drawable.GetVertices()) {
                                if (idx >= ms_vertices.Length) {
                                    Debug.LogError($"Drawable gave too many vertices!", drawable);
                                    break;
                                }

                                ms_vertices[idx] = new Vertex(vertex);
                                idx += 1;
                            }

                            Debug.Assert(idx == vertexCount, $"Drawable '{drawable}' returned incorrect number of vertices, expected {vertexCount}, got {idx}!", drawable);

                            if (cachedData.m_vertexBuffer == null || cachedData.m_vertexBuffer.count != vertexCount) {
                                cachedData.m_vertexBuffer?.Release();
                                cachedData.m_vertexBuffer = new ComputeBuffer(vertexCount, Marshal.SizeOf(typeof(Vertex)), ComputeBufferType.Default); //new GraphicsBuffer(GraphicsBuffer.Target.Vertex, vertices.Length, Marshal.SizeOf(typeof(Vertex)));
                                cachedData.m_materialProperties.SetBuffer("vertices", cachedData.m_vertexBuffer);
                            }

                            cachedData.m_vertexBuffer.SetData(ms_vertices, 0, 0, vertexCount);

                            m_cachedShapeData[drawableID] = cachedData;
                            dataAvailable = true;
                        } else {
                            // FIXME, better way to mark this?
                            dataAvailable = false;
                        }
                    }

                    if (dataAvailable) {
                        cameraCommandBuffer.DrawProcedural(drawable.transform.localToWorldMatrix, m_renderMaterial, 0, MeshTopology.Lines, cachedData.m_vertexBuffer.count, 1, cachedData.m_materialProperties);
                    }
                }
            }

            context.ExecuteCommandBuffer(cameraCommandBuffer);
            cameraCommandBuffer.Release();

#if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView) {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
#endif

            context.Submit();
        }
    }
}