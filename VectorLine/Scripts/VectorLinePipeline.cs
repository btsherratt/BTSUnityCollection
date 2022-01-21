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

    VectorLinePipelineAsset m_asset;
    Material m_renderMaterial;

    Dictionary<int, CachedData> m_cachedShapeData;

    public VectorLinePipeline(VectorLinePipelineAsset asset) {
        m_asset = asset;

        m_renderMaterial = new Material(asset.m_basicShader);
        m_cachedShapeData = new Dictionary<int, CachedData>();
    }

    protected override void Dispose(bool disposing) {
        foreach (CachedData data in m_cachedShapeData.Values) {
            data.m_vertexBuffer.Release();
        }

        base.Dispose(disposing);
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
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

                    if (drawable.IsDirty) {
                        if (dataAvailable) {
                            cachedData.m_vertexBuffer.Release();
                        }
                        dataAvailable = false;
                    }

                    if (dataAvailable == false) {
                        int vertexCount = drawable.VertexCount;

                        if (vertexCount > 0) {
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
                            dataAvailable = true;
                        }
                    }

                    if (dataAvailable) {
                        cameraCommandBuffer.DrawProcedural(drawable.transform.localToWorldMatrix, m_renderMaterial, 0, MeshTopology.Lines, cachedData.m_vertexBuffer.count, 1, cachedData.m_materialProperties);
                    }
                }
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
    }
}