using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class InstanceRenderer : MonoBehaviour {
        const int KERNEL_SIZE = 256;

        struct DrawPair {
            public Material material;
            public Mesh mesh;
            public int submeshIndex;
            public Matrix4x4 matrix;
            public Vector2 screenRelativeTransitionHeights;

            public float size;
            public Vector3 localReferencePoint;
        }

        struct DrawData {
            public DrawPair[] drawPairs;

            public ComputeBuffer unculledDataBuffer;

            public ComputeBuffer indirectArguments;
            public ComputeBuffer matrixBuffer;
        }

        /*[StructLayout(LayoutKind.Explicit, Pack = 1, Size = InstanceDetails.Size)]
        public struct InstanceDetails{
            public const int Size = 3 * 4 * sizeof(float);

            [FieldOffset(0 * sizeof(float))]
            public Vector3 position;

            [FieldOffset(3 * sizeof(float))]
            public float uniformScale;

            [FieldOffset(4 * sizeof(float))]
            public Quaternion rotation;

            [FieldOffset(8 * sizeof(float))]
            public Vector4 instanceData;
        }*/

        public ComputeShader m_computeShader;

        [Layer]
        public int m_renderLayer;

        CommandBuffer m_commandBuffer;

        List<DrawData> m_drawData;
        HashSet<Camera> m_cameras;

        void OnDestroy() {
            Cleanup();
        }

        void OnAnyInstanceProviderChanged(InstanceProvider instanceProvider) {
            Cleanup();
        }

        void OnAnyInstanceAreaChanged(InstanceArea instanceArea) {
            Cleanup();
        }

        void OnEnable() {
            InstanceArea.ms_changeEvent -= OnAnyInstanceAreaChanged;
            InstanceArea.ms_changeEvent += OnAnyInstanceAreaChanged;

            InstanceProvider.ms_changeEvent -= OnAnyInstanceProviderChanged;
            InstanceProvider.ms_changeEvent += OnAnyInstanceProviderChanged;

            Camera.onPreCull -= TryDraw;
            Camera.onPreCull += TryDraw;
        }

        void OnDisable() {
            InstanceArea.ms_changeEvent -= OnAnyInstanceAreaChanged;
            InstanceProvider.ms_changeEvent -= OnAnyInstanceProviderChanged;
            Camera.onPreCull -= TryDraw;
        }

        void TryDraw(Camera camera) {
            if (camera != null && ((camera.cameraType == CameraType.Game && (camera.cullingMask & (1 << m_renderLayer)) > 0) || camera.cameraType == CameraType.SceneView)) {
                Camera detailsCamera = (Application.isPlaying && camera.cameraType == CameraType.SceneView) ? Camera.main : camera;

                Vector4 cameraDetails1 = detailsCamera.transform.position;
                Vector4 cameraDetails2 = new Vector4(detailsCamera.nearClipPlane, detailsCamera.farClipPlane, detailsCamera.fieldOfView * Mathf.Deg2Rad, QualitySettings.lodBias);
                Vector4 cameraDetails3 = detailsCamera.transform.forward;
                m_computeShader.SetVectorArray("_CameraDetails", new Vector4[] { cameraDetails1, cameraDetails2, cameraDetails3 });

                if (m_commandBuffer == null) {
                    Setup();
                }

                if (m_cameras == null) {
                    m_cameras = new HashSet<Camera>();
                }

                if (m_cameras.Contains(camera) == false) {
                    camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_commandBuffer);
                    m_cameras.Add(camera);
                }
            }
        }

        void Setup() {
            GenerateDrawData();
            GenerateCommandBuffer();
        }

        void Cleanup() {
            CleanupDrawData();
            CleanupCommandBuffer();
        }

        void GenerateDrawData() {
            CleanupDrawData();

            m_drawData = new List<DrawData>();

            Dictionary<LODGroup, List<InstanceProvider.InstanceDetails>> instanceDetailsByPrefabLOD = new Dictionary<LODGroup, List<InstanceProvider.InstanceDetails>>();

            foreach (InstanceProvider instanceProvider in InstanceProvider.All()) {
                foreach (InstanceProvider.InstanceDetails details in instanceProvider.GenerateInstanceDetails()) {
                    LODGroup prefabLODGroup = details.prefabConfiguration.m_prefab.GetComponent<LODGroup>();
                    if (prefabLODGroup != null) {
                        List<InstanceProvider.InstanceDetails> instanceDetails;
                        if (instanceDetailsByPrefabLOD.TryGetValue(prefabLODGroup, out instanceDetails) == false) {
                            instanceDetails = new List<InstanceProvider.InstanceDetails>();
                            instanceDetailsByPrefabLOD[prefabLODGroup] = instanceDetails;
                        }
                        instanceDetails.Add(details);
                    }
                }
            }

            foreach (var pair in instanceDetailsByPrefabLOD) {
                LODGroup prefabLODGroup = pair.Key;
                List<InstanceProvider.InstanceDetails> instanceDetails = pair.Value;

                List<DrawPair> drawPairs = new List<DrawPair>();

                LOD[] prefabLODs = prefabLODGroup.GetLODs();
                float lastTransitionHeight = float.PositiveInfinity;
                for (int i = 0; i < prefabLODs.Length; ++i) {
                    LOD prefabLOD = prefabLODs[i];

                    float nextTransitionHeight = i < prefabLODs.Length - 2 ? prefabLODs[i + 1].screenRelativeTransitionHeight : 0;

                    foreach (Renderer renderer in prefabLOD.renderers) {
                        MeshFilter mf = renderer.GetComponent<MeshFilter>();
                        if (mf != null) {
                            for (int submeshIdx = 0; submeshIdx < renderer.sharedMaterials.Length; ++submeshIdx) {
                                DrawPair drawPair = new DrawPair();
                                drawPair.material = new Material(renderer.sharedMaterials[submeshIdx]);
                                drawPair.material.EnableKeyword("SKFX_WB_INSTANCING_ENABLED");
                                drawPair.mesh = mf.sharedMesh;
                                drawPair.submeshIndex = submeshIdx;
                                //drawPair.matrix = details.prefabConfiguration.m_prefab.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                                drawPair.matrix = prefabLODGroup.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix; // FIXME!!!!
                                // drawPair.material.SetMatrix("_MeshOffsetMatrix", drawPair.matrix);
                                //drawPair.screenRelativeTransitionHeights = new Vector2(prefabLOD.screenRelativeTransitionHeight, nextTransitionHeight);
                                drawPair.screenRelativeTransitionHeights = new Vector2(lastTransitionHeight, prefabLOD.screenRelativeTransitionHeight);
                                drawPair.size = prefabLODGroup.size;
                                drawPair.localReferencePoint = prefabLODGroup.localReferencePoint;

                                drawPairs.Add(drawPair);
                            }
                        }
                    }

                    lastTransitionHeight = prefabLOD.screenRelativeTransitionHeight;
                }

                DrawData drawData = new DrawData();
                drawData.drawPairs = drawPairs.ToArray();

                long detailsCount = 0;
                foreach (InstanceProvider.InstanceDetails details in instanceDetails) {
                    detailsCount += details.DetailsCount;
                }

                if (detailsCount > 0) {
                    int roundedElements = Mathf.CeilToInt((float)detailsCount / (float)KERNEL_SIZE) * KERNEL_SIZE;
                    drawData.unculledDataBuffer = new ComputeBuffer(roundedElements, TransformDetails.Size, ComputeBufferType.Structured, ComputeBufferMode.Immutable);

                    TransformDetails[] allInstanceDetails = new TransformDetails[roundedElements];
                    int count = 0;

                    long startIdx = 0;
                    foreach (InstanceProvider.InstanceDetails details in instanceDetails) {
                        startIdx = details.GenerateDetails(allInstanceDetails, startIdx);

                        /*foreach (TransformDetails transformDetails in details.GenerateSnappedDetails()) {
                            InstanceDetails d = new InstanceDetails();
                            d.position = transformDetails.position;
                            d.rotation = transformDetails.rotation;
                            d.uniformScale = transformDetails.uniformScale;
                            d.instanceData = new Vector4();
                            allInstanceDetails[count++] = d;
                        }*/
                    }
                    drawData.unculledDataBuffer.SetData(allInstanceDetails);

                    uint[] args = new uint[drawPairs.Count * 5];
                    for (int i = 0; i < drawPairs.Count; ++i) {
                        int baseIdx = i * 5;
                        args[baseIdx + 0] = (uint)drawPairs[i].mesh.GetIndexCount(drawPairs[i].submeshIndex);
                        args[baseIdx + 1] = (uint)0;
                        args[baseIdx + 2] = (uint)drawPairs[i].mesh.GetIndexStart(drawPairs[i].submeshIndex);
                        args[baseIdx + 3] = (uint)drawPairs[i].mesh.GetBaseVertex(drawPairs[i].submeshIndex);
                    }

                    drawData.indirectArguments = new ComputeBuffer(drawPairs.Count, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
                    drawData.indirectArguments.SetData(args);

                    drawData.matrixBuffer = new ComputeBuffer(roundedElements, 4 * 4 * sizeof(float), ComputeBufferType.Structured | ComputeBufferType.Append);

                    m_drawData.Add(drawData);
                }
            }
        }

        void CleanupDrawData() {
            if (m_drawData != null) {
                foreach (DrawData drawData in m_drawData) {
                    drawData.indirectArguments.Release();
                    drawData.unculledDataBuffer.Release();
                    drawData.matrixBuffer.Release();
                }
                m_drawData = null;
            }
        }

        void GenerateCommandBuffer() {
            CleanupCommandBuffer();

            int kernel = m_computeShader.FindKernel("Culling");

            m_commandBuffer = new CommandBuffer();

            foreach (DrawData drawData in m_drawData) {
                m_commandBuffer.SetComputeBufferParam(m_computeShader, kernel, "_Input", drawData.unculledDataBuffer);
                m_commandBuffer.SetComputeBufferParam(m_computeShader, kernel, "_Output", drawData.matrixBuffer);

                for (int i = 0; i < drawData.drawPairs.Length; ++i) {
                    DrawPair drawPair = drawData.drawPairs[i];

                    m_commandBuffer.SetComputeVectorParam(m_computeShader, "_Heights", drawPair.screenRelativeTransitionHeights);
                    m_commandBuffer.SetComputeFloatParam(m_computeShader, "_Size", drawPair.size);
                    m_commandBuffer.SetComputeVectorParam(m_computeShader, "_LocalReferencePoint", drawPair.localReferencePoint);

                    m_commandBuffer.SetComputeMatrixParam(m_computeShader, "_MeshOffsetMatrix", drawPair.matrix);

                    m_commandBuffer.SetBufferCounterValue(drawData.matrixBuffer, 0);
                    m_commandBuffer.DispatchCompute(m_computeShader, kernel, drawData.unculledDataBuffer.count / KERNEL_SIZE, /*details.inputData.Length / 256*/1, 1);

                    m_commandBuffer.SetGlobalMatrix("_MeshOffsetMatrix", drawPair.matrix);

                    m_commandBuffer.CopyCounterValue(drawData.matrixBuffer, drawData.indirectArguments, (uint)(i * 5 * sizeof(uint) + sizeof(uint)));
                    drawPair.material.SetBuffer("_SKFXWorldBuilderInstanceData", drawData.matrixBuffer);

                    m_commandBuffer.DrawMeshInstancedIndirect(drawPair.mesh, drawPair.submeshIndex, drawPair.material, 0/*m_instanceData.m_bounds*/, drawData.indirectArguments, argsOffset: i * 5 * sizeof(uint));
                }
            }
        }

        void CleanupCommandBuffer() {
            if (m_commandBuffer != null) {
                foreach (Camera camera in m_cameras) {
                    if (camera != null)
                        camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_commandBuffer);
                }
                m_cameras.Clear();

                m_commandBuffer.Release();
                m_commandBuffer = null;
            }
        }
    }
}
