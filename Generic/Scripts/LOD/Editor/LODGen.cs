using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class LODGen {
    const TextureFormat k_TextureFormat = TextureFormat.RGBA32;

    [MenuItem("Assets/BTS/Generate Billboards", false, 100)]
    static void GenerateBillboards() {
        bool linearColorSpace = QualitySettings.activeColorSpace == ColorSpace.Linear;

        List<Quaternion> directions = new List<Quaternion>();
        directions.Add(Quaternion.LookRotation(Vector3.down, Vector3.back));
        directions.Add(Quaternion.LookRotation(Vector3.forward));
        directions.Add(Quaternion.LookRotation(Vector3.left));
        directions.Add(Quaternion.LookRotation(Vector3.back));
        directions.Add(Quaternion.LookRotation(Vector3.right));

        foreach (GameObject gameObject in Selection.gameObjects) {
            BillboardDetails billboardDetails = GenerateBillboard(gameObject, directions.ToArray(), 1024, linearColorSpace);

            Scene previewScene = EditorSceneManager.NewPreviewScene();

            string path = AssetDatabase.GetAssetPath(gameObject);
            string directory = System.IO.Path.GetDirectoryName(path);
            string baseFilename = System.IO.Path.GetFileNameWithoutExtension(path);
            string billboardFilename = $"{directory}/{baseFilename}_Billboard.prefab";

            GameObject billboardContainer = new GameObject($"{gameObject.name}_Billboard");
            SceneManager.MoveGameObjectToScene(billboardContainer, previewScene);
            billboardContainer = PrefabUtility.SaveAsPrefabAsset(billboardContainer, billboardFilename);

            AssetDatabase.AddObjectToAsset(billboardDetails.albedo, billboardContainer);
            AssetDatabase.AddObjectToAsset(billboardDetails.mesh, billboardContainer);

            Material material = new Material(Shader.Find("Unlit/Transparent Cutout"));
            material.name = "Billboard Material";
            material.mainTexture = billboardDetails.albedo;
            //material.SetFloat("_Cutoff", 0.6f);

            AssetDatabase.AddObjectToAsset(material, billboardContainer);

            MeshFilter mf = billboardContainer.AddComponent<MeshFilter>();
            mf.sharedMesh = billboardDetails.mesh;

            MeshRenderer mr = billboardContainer.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;

            PrefabUtility.SavePrefabAsset(billboardContainer);

            EditorSceneManager.ClosePreviewScene(previewScene);
        }
    }

    [MenuItem("Assets/BTS/Generate Billboards", true)]
    static bool GenerateBillboardValidate() {
        foreach (GameObject gameObject in Selection.gameObjects) {
            if (gameObject.GetComponentInChildren<Renderer>() != null) {
                return true;
            }
        }
        return false;
    }

    struct BillboardDetails {
        public Texture2D albedo;
        public Mesh mesh;
    }

    static BillboardDetails GenerateBillboard(GameObject gameObject, Quaternion[] rotations, int individualTextureSize, bool linearColorSpace) {
        List<Texture2D> albedoTextures = new List<Texture2D>();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> indices = new List<int>();

        foreach (Quaternion rotation in rotations) {
            Bounds bounds;
            Texture2D texture = GenerateBillboard(out bounds, gameObject, rotation, individualTextureSize, linearColorSpace);
            albedoTextures.Add(texture);

            int startIdx = vertices.Count;

            Quaternion iRotation = Quaternion.LookRotation(rotation * Vector3.back, rotation * Vector3.up);

            float mag = bounds.extents.magnitude;
            vertices.Add(iRotation * new Vector3(-mag, -mag, 0) + bounds.center);
            vertices.Add(iRotation * new Vector3( mag, -mag, 0) + bounds.center);
            vertices.Add(iRotation * new Vector3( mag,  mag, 0) + bounds.center);
            vertices.Add(iRotation * new Vector3(-mag,  mag, 0) + bounds.center);

            Vector3 normal = iRotation * Vector3.forward;
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            indices.Add(startIdx + 0);
            indices.Add(startIdx + 1);
            indices.Add(startIdx + 2);
            indices.Add(startIdx + 0);
            indices.Add(startIdx + 2);
            indices.Add(startIdx + 3);
        }

        int atlasRows = Mathf.CeilToInt(Mathf.Sqrt(rotations.Length));
        int atlasCols = Mathf.CeilToInt(rotations.Length / (float)atlasRows);

        int atlasWidth = Mathf.NextPowerOfTwo(individualTextureSize * atlasCols);
        int atlasHeight = Mathf.NextPowerOfTwo(individualTextureSize * atlasRows);
        Texture2D albedoAtlas = new Texture2D(atlasWidth, atlasHeight, k_TextureFormat, false, linearColorSpace);
        albedoAtlas.name = "Billboard Albedo Atlas";

        Rect[] rects = albedoAtlas.PackTextures(albedoTextures.ToArray(), 0, Mathf.Max(atlasWidth, atlasHeight), true); // FIXME, need to smear this to the edges...

        List<Vector2> uvs = new List<Vector2>();
        foreach (Rect rect in rects) {
            uvs.Add(new Vector2(rect.xMax, rect.yMin));
            uvs.Add(rect.min);
            uvs.Add(new Vector2(rect.xMin, rect.yMax));
            uvs.Add(rect.max);
        }

        Mesh mesh = new Mesh();
        mesh.name = "Billboard";
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        BillboardDetails billboardDetails = new BillboardDetails();
        billboardDetails.albedo = albedoAtlas;
        billboardDetails.mesh = mesh;

        return billboardDetails;
    }

    static Texture2D GenerateBillboard(out Bounds renderedBounds, GameObject gameObject, Quaternion rotation, int textureSize, bool linearColorSpace) {
        Scene previewScene = EditorSceneManager.NewPreviewScene();

        GameObject subjectGameObject = (GameObject)PrefabUtility.InstantiatePrefab(gameObject, previewScene);

        Bounds bounds = new Bounds();
        foreach (Renderer renderer in subjectGameObject.GetComponentsInChildren<Renderer>()) {
            if (bounds.extents.magnitude <= float.Epsilon) {
                bounds = renderer.bounds;
            } else {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        GameObject cameraGameObject = EditorUtility.CreateGameObjectWithHideFlags("Billboard Scene Camera", HideFlags.HideAndDontSave, typeof(Camera));
        SceneManager.MoveGameObjectToScene(cameraGameObject, previewScene);

        Camera camera = cameraGameObject.GetComponent<Camera>();
        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;
        camera.orthographic = true;
        camera.orthographicSize = bounds.extents.magnitude;
        camera.backgroundColor = new Color(0, 0, 0, 0);
        camera.farClipPlane = bounds.extents.magnitude + 1.0f;
        camera.nearClipPlane = -camera.farClipPlane;
        camera.scene = previewScene;

        camera.transform.position = bounds.center;
        camera.transform.rotation = rotation;

        Texture2D texture = camera.CaptureTexture(textureSize, linearColorSpace);
        
        EditorSceneManager.ClosePreviewScene(previewScene);

        renderedBounds = bounds;
        return texture;
    }

    static Texture2D CaptureTexture(this Camera camera, int textureSize, bool linearColorSpace) {
        RenderTexture previousCameraRenderTexture = camera.targetTexture;
        RenderTexture previousRenderTexture = RenderTexture.active;

        RenderTexture cameraRenderTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
        camera.targetTexture = cameraRenderTexture;
        RenderTexture.active = cameraRenderTexture;

        camera.Render();

        // TODO: Smear the colours for nicer edges...

        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false, linearColorSpace);
        texture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        texture.Apply();

        RenderTexture.active = previousRenderTexture;
        camera.targetTexture = previousCameraRenderTexture;
        cameraRenderTexture.Release();

        return texture;
    }
}
