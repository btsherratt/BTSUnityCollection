using System.Collections.Generic;
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

            public SafeDisposable<ComputeBuffer> unculledDataBuffer;

            public SafeDisposable<ComputeBuffer> indirectArguments;
            public SafeDisposable<ComputeBuffer> matrixBuffer;
        }

        public ComputeShader m_computeShader;
        int m_kernel;


        [Layer]
        public int m_renderLayer;

        CommandBuffer m_commandBuffer;

        BoundingSphere[] m_boundingSpheres;
        CullingGroup m_cullingGroup;
        public SafeDisposable<ComputeBuffer> m_cullingBinBuffer;
        bool m_performFullCull;

        List<DrawData> m_drawData;
        HashSet<Camera> m_cameras;

        private void Awake() {
            m_kernel = m_computeShader.FindKernel("Culling");
        }

        private void OnApplicationQuit() {
            Cleanup();
        }

        void OnDestroy() {
            Cleanup();
        }

        /*void OnAnyInstanceProviderChanged(OldInstanceProvider instanceProvider) {
            Cleanup();
        }*/

        void OnAnyInstanceAreaChanged(InstanceArea instanceArea) {
            Cleanup();
        }

        void OnEnable() {
            InstanceArea.ms_changeEvent -= OnAnyInstanceAreaChanged;
            InstanceArea.ms_changeEvent += OnAnyInstanceAreaChanged;

            /*OldInstanceProvider.ms_changeEvent -= OnAnyInstanceProviderChanged;
            OldInstanceProvider.ms_changeEvent += OnAnyInstanceProviderChanged;*/

            Camera.onPreRender -= TryDraw;
            Camera.onPreRender += TryDraw;
        }

        void OnDisable() {
            InstanceArea.ms_changeEvent -= OnAnyInstanceAreaChanged;
            //OldInstanceProvider.ms_changeEvent -= OnAnyInstanceProviderChanged;
            Camera.onPreRender -= TryDraw;
            Cleanup();
        }

        void TryDraw(Camera camera) {
            if (camera != null && ((camera.cameraType == CameraType.Game && (camera.cullingMask & (1 << m_renderLayer)) > 0) || camera.cameraType == CameraType.SceneView)) {
                Camera detailsCamera = (Application.isPlaying && camera.cameraType == CameraType.SceneView) ? Camera.main : camera;
                if (detailsCamera != null) {
                    Vector4 cameraDetails1 = detailsCamera.transform.position;
                    Vector4 cameraDetails2 = new Vector4(detailsCamera.nearClipPlane, detailsCamera.farClipPlane, detailsCamera.fieldOfView * Mathf.Deg2Rad, QualitySettings.lodBias);
                    Vector4 cameraDetails3 = detailsCamera.transform.forward;
                    m_computeShader.SetVectorArray("_CameraDetails", new Vector4[] { cameraDetails1, cameraDetails2, cameraDetails3 });

                    if (m_performFullCull) {
                        m_performFullCull = false;
                        UploadCullingData();
                    }

                    if (m_commandBuffer == null) {
                        Setup(detailsCamera);
                    }

                    if (m_cameras == null) {
                        m_cameras = new HashSet<Camera>();
                    }

                    if (m_cameras.Contains(camera) == false) {
                        camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_commandBuffer);
                        m_cameras.Add(camera);
                    }

                    if (m_cullingGroup != null && m_cullingGroup.targetCamera != detailsCamera) {
                        m_cullingGroup.targetCamera = detailsCamera;
                        m_performFullCull = true; // Try performing cull next frame
                    }
                }
            }
        }

        void Setup(Camera camera) {
            GenerateDrawData(camera);
            GenerateCommandBuffer();
            UploadCullingData();
        }

        void Cleanup() {
            CleanupDrawData();
            CleanupCommandBuffer();
        }

        void GenerateDrawData(Camera camera) {
            CleanupDrawData();

            m_drawData = new List<DrawData>();

            Dictionary<LODGroup, List<InstanceProvider.InstanceDetails>> instanceDetailsByPrefabLOD = new Dictionary<LODGroup, List<InstanceProvider.InstanceDetails>>();

            InstanceProvider instanceProvider = GetComponent<InstanceProvider>();
//            foreach (OldInstanceProvider instanceProvider in OldInstanceProvider.All()) {
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
//           }


            //Bounds bounds = new Bounds(allInstanceDetails[0].position, Vector3.zero);
            //for (int i = 1; i < (int)startIdx; ++i) {
            //    bounds.Encapsulate(allInstanceDetails[i].position);
            //}

            const float CELL_SIZE = 100.0f;
            //int width = Mathf.CeilToInt(bounds.size.x / CELL_SIZE);
            //int depth = Mathf.CeilToInt(bounds.size.z / CELL_SIZE);

            Dictionary<(int, int), int> worldSpace = new Dictionary<(int, int), int>();
            List<List<Vector3>> worldSpacePoints = new List<List<Vector3>>();
            //Dictionary<(int, int), List<int>> worldSpace = new Dictionary<(int, int), List<int>>();

            //Bounds?[] boundingCollections = new Bounds?[width * depth];
            //int sphereCount = 0;


            long totalInstances = 0;
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
                    //drawData.unculledDataBuffer = new ComputeBuffer(roundedElements, TransformDetails.Size, ComputeBufferType.Structured, ComputeBufferMode.Immutable);

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

                    for (int i = 0; i < (int)startIdx; ++i) {
                        int cellX = Mathf.CeilToInt(allInstanceDetails[i].position.x / CELL_SIZE);
                        int cellZ = Mathf.CeilToInt(allInstanceDetails[i].position.z / CELL_SIZE);

                        if (worldSpace.TryGetValue((cellX, cellZ), out var listIdx)) {
                            allInstanceDetails[i].cullingBin = listIdx;
                            worldSpacePoints[listIdx].Add(allInstanceDetails[i].position);
                        } else {
                            listIdx = worldSpacePoints.Count;
                            worldSpace[(cellX, cellZ)] = listIdx;
                            worldSpacePoints.Add(new List<Vector3>());

                            allInstanceDetails[i].cullingBin = listIdx;
                            worldSpacePoints[listIdx].Add(allInstanceDetails[i].position);
                        }
                    }

                    if (startIdx > int.MaxValue) {
                        Debug.LogError("Too many values!!!");
                    }

                    if (startIdx > 0) {
                        drawData.unculledDataBuffer = new ComputeBuffer((int)startIdx, TransformDetails.Size, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
                        drawData.unculledDataBuffer.Value.SetData(allInstanceDetails, 0, 0, drawData.unculledDataBuffer.Value.count);

                        uint[] args = new uint[drawPairs.Count * 5];
                        for (int i = 0; i < drawPairs.Count; ++i) {
                            int baseIdx = i * 5;
                            args[baseIdx + 0] = (uint)drawPairs[i].mesh.GetIndexCount(drawPairs[i].submeshIndex);
                            args[baseIdx + 1] = (uint)0;
                            args[baseIdx + 2] = (uint)drawPairs[i].mesh.GetIndexStart(drawPairs[i].submeshIndex);
                            args[baseIdx + 3] = (uint)drawPairs[i].mesh.GetBaseVertex(drawPairs[i].submeshIndex);
                        }

                        drawData.indirectArguments = new ComputeBuffer(drawPairs.Count, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
                        drawData.indirectArguments.Value.SetData(args);

                        drawData.matrixBuffer = new ComputeBuffer((int)startIdx, 4 * 4 * sizeof(float), ComputeBufferType.Structured | ComputeBufferType.Append);

                        m_drawData.Add(drawData);
                    }

                    totalInstances += startIdx;
                }
            }

            Debug.Log($"Generated {totalInstances} instances");


            m_boundingSpheres = new BoundingSphere[worldSpace.Count];
            foreach (var worldSpacePair in worldSpace) {
                List<Vector3> points = worldSpacePoints[worldSpacePair.Value];

                Bounds bounds = new Bounds(points[0], Vector3.zero);
                foreach (Vector3 point in points) {
                    bounds.Encapsulate(point);
                }

                m_boundingSpheres[worldSpacePair.Value] = new BoundingSphere(bounds.center, bounds.extents.magnitude);
            }

            // Dictionary<(int, int), int> worldSpace = new Dictionary<(int, int), int>();
            // List<List<Vector3>> worldSpacePoints = new List<List<Vector3>>();

            Debug.Assert(m_boundingSpheres.Length == m_boundingSpheres.LongLength);

            m_cullingGroup = new CullingGroup();
            m_cullingGroup.onStateChanged += OnCullingStateChanged;
            m_cullingGroup.targetCamera = camera;
            //m_cullingGroup.SetDistanceReferencePoint(camera.transform); // This is dumb
            //m_cullingGroup.SetBoundingDistances(new float[] { float.PositiveInfinity });
            m_cullingGroup.SetBoundingSpheres(m_boundingSpheres);
            m_cullingGroup.SetBoundingSphereCount(m_boundingSpheres.Length);

            m_cullingBinBuffer = new ComputeBuffer(m_boundingSpheres.Length, sizeof(int), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);

        }

        void CleanupDrawData() {
            if (m_drawData != null) {
                foreach (DrawData drawData in m_drawData) {
                    drawData.indirectArguments.Dispose();
                    drawData.unculledDataBuffer.Dispose();
                    drawData.matrixBuffer.Dispose();
                }
                m_drawData = null;

                m_cullingGroup.Dispose();
                m_cullingGroup = null;

                m_cullingBinBuffer.Dispose();
                m_cullingBinBuffer = null;
            }
        }

        void GenerateCommandBuffer() {
            CleanupCommandBuffer();
            m_commandBuffer = new CommandBuffer();
            AddCommandBufferValues();
        }

        void AddCommandBufferValues() {
            m_commandBuffer.Clear();

            foreach (DrawData drawData in m_drawData) {
                m_commandBuffer.SetComputeBufferParam(m_computeShader, m_kernel, "_CullingBins", m_cullingBinBuffer);

                m_commandBuffer.SetComputeBufferParam(m_computeShader, m_kernel, "_Input", drawData.unculledDataBuffer);
                m_commandBuffer.SetComputeBufferParam(m_computeShader, m_kernel, "_Output", drawData.matrixBuffer);

                for (int i = 0; i < drawData.drawPairs.Length; ++i) {
                    DrawPair drawPair = drawData.drawPairs[i];

                    m_commandBuffer.SetComputeVectorParam(m_computeShader, "_Heights", drawPair.screenRelativeTransitionHeights);
                    m_commandBuffer.SetComputeFloatParam(m_computeShader, "_Size", drawPair.size);
                    m_commandBuffer.SetComputeVectorParam(m_computeShader, "_LocalReferencePoint", drawPair.localReferencePoint);

                    m_commandBuffer.SetComputeMatrixParam(m_computeShader, "_MeshOffsetMatrix", drawPair.matrix);

                    m_commandBuffer.SetBufferCounterValue(drawData.matrixBuffer, 0);
                    m_commandBuffer.DispatchCompute(m_computeShader, m_kernel, drawData.unculledDataBuffer.Value.count / KERNEL_SIZE, /*details.inputData.Length / 256*/1, 1);

                    m_commandBuffer.SetGlobalMatrix("_MeshOffsetMatrix", drawPair.matrix);

                    m_commandBuffer.CopyCounterValue(drawData.matrixBuffer, drawData.indirectArguments, (uint)(i * 5 * sizeof(uint) + sizeof(uint)));
                    drawPair.material.SetBuffer("_SKFXWorldBuilderInstanceData", drawData.matrixBuffer);

                    m_commandBuffer.DrawMeshInstancedIndirect(drawPair.mesh, drawPair.submeshIndex, drawPair.material, 0/*m_instanceData.m_bounds*/, drawData.indirectArguments, argsOffset: i * 5 * sizeof(uint));
                }
            }
        }

        void UploadCullingData() {
            if (m_cullingBinBuffer.Value != null) {
                var data = m_cullingBinBuffer.Value.BeginWrite<int>(0, m_boundingSpheres.Length);
                for (int i = 0; i < m_boundingSpheres.Length; ++i) {
                    data[i] = m_cullingGroup.IsVisible(i) ? 1 : 0;
                }
                m_cullingBinBuffer.Value.EndWrite<int>(m_boundingSpheres.Length);
            }
        }

        void OnCullingStateChanged(CullingGroupEvent sphere) {
            //UploadCullingData();

            var data = m_cullingBinBuffer.Value.BeginWrite<int>(sphere.index, 1);
            for (int i = 0; i < m_boundingSpheres.Length; ++i) {
                data[0] = sphere.isVisible ? 1 : 0;
            }
            m_cullingBinBuffer.Value.EndWrite<int>(1);
            
            //AddCommandBufferValues();//???
        }

        void CleanupCommandBuffer() {
            if (m_commandBuffer != null) {
                foreach (Camera camera in m_cameras) {
                    if (camera != null)
                        camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_commandBuffer);
                }

                m_commandBuffer.Release();
                m_commandBuffer = null;
            }

            if (m_cameras != null) {
                m_cameras.Clear();
            }
        }

        private void OnDrawGizmosSelected() {
            if (m_drawData != null) {
                foreach (DrawData drawData in m_drawData) {
                    foreach (BoundingSphere boundingSphere in m_boundingSpheres) {
                        Gizmos.DrawWireSphere(boundingSphere.position, boundingSphere.radius);
                    }
                }
            }
            
        }
    }
}
