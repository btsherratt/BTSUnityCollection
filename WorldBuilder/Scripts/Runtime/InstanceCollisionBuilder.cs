using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace SKFX.WorldBuilder {
    public class InstanceCollisionBuilder : MonoBehaviour {
#if false
        void Start() {
            Setup();
        }

        void Setup() {
            StartCoroutine(GenerateCollisionMesh());
        }

        IEnumerator GenerateCollisionMesh() {
            Dictionary<GameObject, List<OldInstanceProvider.InstanceDetails>> instanceDetailsByPrefab = new Dictionary<GameObject, List<OldInstanceProvider.InstanceDetails>>();

            foreach (OldInstanceProvider instanceProvider in OldInstanceProvider.All()) {
                foreach (OldInstanceProvider.InstanceDetails details in instanceProvider.GenerateInstanceDetails()) {
                    Collider prefabCollider = details.prefabConfiguration.m_prefab.GetComponentInChildren<Collider>();
                    if (prefabCollider != null) {
                        List<OldInstanceProvider.InstanceDetails> instanceDetails;
                        if (instanceDetailsByPrefab.TryGetValue(details.prefabConfiguration.m_prefab, out instanceDetails) == false) {
                            instanceDetails = new List<OldInstanceProvider.InstanceDetails>();
                            instanceDetailsByPrefab[details.prefabConfiguration.m_prefab] = instanceDetails;
                        }
                        instanceDetails.Add(details);
                    }
                }
            }

            List<CombineInstance> combineInstances = new List<CombineInstance>();

            GameObject host = new GameObject("__COLLISION_HOST");

            long maxTransformDetails = 0;
            foreach (var pair in instanceDetailsByPrefab) {
                long transformDetailsCount = 0;
                List<OldInstanceProvider.InstanceDetails> instanceDetails = pair.Value;
                foreach (OldInstanceProvider.InstanceDetails details in instanceDetails) {
                    transformDetailsCount += details.DetailsCount;
                }
                if (transformDetailsCount > maxTransformDetails) {
                    maxTransformDetails = transformDetailsCount;
                }
            }

            TransformDetails[] allTransformDetails = new TransformDetails[maxTransformDetails];
            foreach (var pair in instanceDetailsByPrefab) {
                GameObject prefab = pair.Key;
                List<OldInstanceProvider.InstanceDetails> instanceDetails = pair.Value;

                Collider[] prefabColliders = prefab.GetComponentsInChildren<Collider>();

                long endIdx = 0;
                foreach (OldInstanceProvider.InstanceDetails details in instanceDetails) {
                    endIdx = details.GenerateDetails(allTransformDetails, endIdx);
                }

                for (long i = 0; i < endIdx; ++i) {
                    TransformDetails transformDetails = allTransformDetails[i];

                    if (transformDetails.uniformScale > 0.0f) {
                        foreach (Collider collider in prefabColliders) {
                            System.Type colliderType = collider.GetType();

                            if (colliderType == typeof(MeshCollider)) {
                                Matrix4x4 matrix = Matrix4x4.TRS(transformDetails.position, transformDetails.rotation, Vector3.one * transformDetails.uniformScale);

                                MeshCollider cloneCollider = collider as MeshCollider;

                                CombineInstance combineInstance = new CombineInstance();
                                combineInstance.mesh = cloneCollider.sharedMesh;
                                combineInstance.transform = matrix;
                                combineInstances.Add(combineInstance);
                            } else {
                                GameObject newColliderHost = new GameObject("Host");
                                newColliderHost.transform.SetParent(host.transform);
                                newColliderHost.transform.position = transformDetails.position;
                                newColliderHost.transform.rotation = transformDetails.rotation;
                                newColliderHost.transform.localScale = Vector3.one * transformDetails.uniformScale; // FIXME, we need to add the matrix for the child....

                                if (colliderType == typeof(BoxCollider)) {
                                    BoxCollider cloneCollider = collider as BoxCollider;
                                    BoxCollider newCollider = newColliderHost.AddComponent<BoxCollider>();
                                    newCollider.center = cloneCollider.center;
                                    newCollider.size = cloneCollider.size;
                                } else if (colliderType == typeof(SphereCollider)) {
                                    SphereCollider cloneCollider = collider as SphereCollider;
                                    SphereCollider newCollider = newColliderHost.AddComponent<SphereCollider>();
                                    newCollider.center = cloneCollider.center;
                                    newCollider.radius = cloneCollider.radius;
                                } else if (colliderType == typeof(CapsuleCollider)) {
                                    CapsuleCollider cloneCollider = collider as CapsuleCollider;
                                    CapsuleCollider newCollider = newColliderHost.AddComponent<CapsuleCollider>();
                                    newCollider.center = cloneCollider.center;
                                    newCollider.direction = cloneCollider.direction;
                                    newCollider.height = cloneCollider.height;
                                    newCollider.radius = cloneCollider.radius;
                                }
                            }
                        }
                    }

                    // Pause a little for a break and some coffee.
                    if (endIdx % 100 == 0) {
                        yield return null;
                    }

                    /*Matrix4x4 matrix = Matrix4x4.TRS(transformDetails.position, transformDetails.rotation, Vector3.one * transformDetails.uniformScale);

                    foreach (Renderer renderer in primaryLOD.renderers) {
                        MeshFilter mf = renderer.GetComponent<MeshFilter>();
                        if (mf != null) {
                            for (int i = 0; i < mf.sharedMesh.subMeshCount; ++i) {
                                CombineInstance combineInstance = new CombineInstance();
                                combineInstance.mesh = mf.sharedMesh;
                                combineInstance.subMeshIndex = i;
                                combineInstance.transform = matrix;
                                combineInstances.Add(combineInstance);
                                Debug.Log("HAI");
                            }
                        }
                    }*/
                }

                // Another little break...
                yield return null;
            }

            if (combineInstances.Count > 0) {
                Mesh collisionMesh = new Mesh();
                collisionMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                collisionMesh.CombineMeshes(combineInstances.ToArray(), true, true);

                MeshCollider collider = host.AddComponent<MeshCollider>();
                collider.sharedMesh = collisionMesh;
            }

        }
#endif
    }
}
