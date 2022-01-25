using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class VectorLinePipeline : RenderPipeline {
    const int MAX_DRAWABLE_COUNT = 1024;
    const int MAX_DRAWABLE_VERTEX_COUNT = 1024;
    const int MAX_CAMERA_BUFFERS = 3;

    static VectorLineDrawable[] ms_drawableArray = new VectorLineDrawable[MAX_DRAWABLE_COUNT];
    static VertexArray ms_vertexArray;

    struct Vertex {
        public Vector3 position;
        public Vector4 color;

        public Vertex(VectorLineVertex vlvtx) {
            position = vlvtx.position;
            color = vlvtx.color;
        }
    }

    class VertexArray : IVectorLineVertexArray {
        public Vertex[] vertices { get; private set; }
        public int currentVertex { get; private set; }

        public VertexArray(int vertexCount) {
            vertices = new Vertex[vertexCount];
            currentVertex = 0;
        }

        public void Reset() {
            currentVertex = 0;
        }

        void IVectorLineVertexArray.AddVertex(VectorLineVertex vertex) {
            vertices[currentVertex] = new Vertex(vertex);
            currentVertex += 1;
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

    CommandBuffer[] m_cameraCommandBuffers = new CommandBuffer[MAX_CAMERA_BUFFERS];
    int m_currentCameraCommandBuffer;

    public VectorLinePipeline(VectorLinePipelineAsset asset) {
        if (ms_vertexArray == null) {
            ms_vertexArray = new VertexArray(MAX_DRAWABLE_VERTEX_COUNT);
        }

        m_asset = asset;
        LoadMaterials();
        m_cachedShapeData = new Dictionary<int, CachedData>();

        for (int i = 0; i < m_cameraCommandBuffers.Length; ++i) {
            m_cameraCommandBuffers[i] = new CommandBuffer();
        }
        m_currentCameraCommandBuffer = 0;
    }

    protected override void Dispose(bool disposing) {
        foreach (CachedData data in m_cachedShapeData.Values) {
            data.m_vertexBuffer.Release();
        }

        for (int i = 0; i < m_cameraCommandBuffers.Length; ++i) {
            m_cameraCommandBuffers[i].Release();
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

        int drawableCount = VectorLineDrawable.Populate(ms_drawableArray, true);

        for (int cameraIdx = 0; cameraIdx < cameras.Length; ++cameraIdx) {
            Camera camera = cameras[cameraIdx];

            context.SetupCameraProperties(camera);

            CommandBuffer cameraCommandBuffer = m_cameraCommandBuffers[m_currentCameraCommandBuffer];
            m_currentCameraCommandBuffer = (m_currentCameraCommandBuffer + 1) % m_cameraCommandBuffers.Length;

            cameraCommandBuffer.Clear();
            cameraCommandBuffer.ClearRenderTarget(camera.clearFlags == CameraClearFlags.SolidColor, true, camera.backgroundColor);

            for (int drawableIdx = 0; drawableIdx < drawableCount; ++drawableIdx) {
                VectorLineDrawable drawable = ms_drawableArray[drawableIdx];
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

                            ms_vertexArray.Reset();
                            drawable.GetVertices(ms_vertexArray);
                            Debug.Assert(ms_vertexArray.currentVertex == vertexCount, $"Drawable '{drawable}' returned incorrect number of vertices, expected {vertexCount}, got {ms_vertexArray.currentVertex}!", drawable);

                            if (cachedData.m_vertexBuffer == null || cachedData.m_vertexBuffer.count != vertexCount) {
                                cachedData.m_vertexBuffer?.Release();
                                cachedData.m_vertexBuffer = new ComputeBuffer(vertexCount, Marshal.SizeOf(typeof(Vertex)), ComputeBufferType.Default); //new GraphicsBuffer(GraphicsBuffer.Target.Vertex, vertices.Length, Marshal.SizeOf(typeof(Vertex)));
                                cachedData.m_materialProperties.SetBuffer("vertices", cachedData.m_vertexBuffer);
                            }

                            cachedData.m_vertexBuffer.SetData(ms_vertexArray.vertices, 0, 0, vertexCount);

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