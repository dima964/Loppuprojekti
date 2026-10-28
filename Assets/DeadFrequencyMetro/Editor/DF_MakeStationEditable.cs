// DEAD FREQUENCY - Editable station, corrected for Unity 6.
// Unlike the old tool, this version reads the meshes Unity actually imported
// and clones their full transform hierarchy. It NEVER parses raw OBJ coordinates.
// Original architecture and props remain in the scene, disabled only on success.
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class DF_MakeStationEditable
{
    private const string Base = "Assets/DeadFrequencyMetro";
    private const string StationName = "DEAD FREQUENCY - Metro Station";

    [MenuItem("Tools/Dead Frequency/Make Props Individually Editable")]
    public static void MakePropsEditable()
    {
        Convert("Props (furniture and lights)", "EDITABLE Props", "DF_Props_FixedMeshes.asset");
    }

    [MenuItem("Tools/Dead Frequency/Make Architecture Individually Editable")]
    public static void MakeArchitectureEditable()
    {
        Convert("Architecture (floors and walls)", "EDITABLE Architecture", "DF_Architecture_FixedMeshes.asset");
    }

    [MenuItem("Tools/Dead Frequency/Make Selected Piece Material Unique")]
    public static void MakeSelectedPieceMaterialUnique()
    {
        GameObject selected = Selection.activeGameObject;
        MeshRenderer renderer = selected == null ? null : selected.GetComponent<MeshRenderer>();
        if (renderer == null || renderer.sharedMaterial == null)
        {
            EditorUtility.DisplayDialog("Select an editable piece",
                "Select one individual piece with a MeshRenderer first.", "OK");
            return;
        }

        string folder = Base + "/CustomPieceMaterials";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(Base, "CustomPieceMaterials");

        string safeName = selected.name.Replace('/', '_').Replace('\\', '_');
        string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + safeName + ".mat");
        Material copy = new Material(renderer.sharedMaterial);
        copy.name = selected.name + " - Unique";
        AssetDatabase.CreateAsset(copy, path);
        Undo.RecordObject(renderer, "Give metro piece unique material");
        renderer.sharedMaterial = copy;
        EditorUtility.SetDirty(renderer);
        Selection.activeObject = copy;
    }

    private static void Convert(string sourceName, string outputName, string assetFile)
    {
        GameObject station = GameObject.Find(StationName);
        if (station == null)
        {
            EditorUtility.DisplayDialog("Metro station not found",
                "Open the scene with the original metro station first.", "OK");
            return;
        }

        Transform original = station.transform.Find(sourceName);
        if (original == null)
        {
            EditorUtility.DisplayDialog("Source not found",
                "Cannot find: " + sourceName + ". Restore the original combined group if needed.", "OK");
            return;
        }

        if (station.transform.Find(outputName) != null)
        {
            EditorUtility.DisplayDialog("Already split",
                "'" + outputName + "' already exists. Delete only the BROKEN editable group (not the original) before retrying. Undo is also an option.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Split metro station safely?",
            "This creates individual pieces using Unity's imported mesh coordinates and keeps your original '" + sourceName + "' as a disabled backup.\n\nSAVE A BACKUP OF YOUR SCENE FIRST.\n\nContinue?",
            "Split objects", "Cancel"))
            return;

        MeshFilter[] filters = original.GetComponentsInChildren<MeshFilter>(true);
        if (filters.Length == 0)
        {
            EditorUtility.DisplayDialog("No meshes", "The original group has no MeshFilters.", "OK");
            return;
        }

        // The Unity importer normally marks imported OBJ meshes non-readable.
        // Enabling Read/Write gives this editor-only conversion access to their triangles.
        var paths = new HashSet<string>();
        foreach (MeshFilter mf in filters)
        {
            if (mf.sharedMesh != null)
            {
                string path = AssetDatabase.GetAssetPath(mf.sharedMesh);
                if (!string.IsNullOrEmpty(path)) paths.Add(path);
            }
        }
        try
        {
            foreach (string path in paths)
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null && !importer.isReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Import settings issue",
                "Could not enable Read/Write on the source model: " + ex.Message, "OK");
            return;
        }

        filters = original.GetComponentsInChildren<MeshFilter>(true);
        string meshFolder = Base + "/EditableMeshes";
        if (!AssetDatabase.IsValidFolder(meshFolder))
            AssetDatabase.CreateFolder(Base, "EditableMeshes");
        string meshPath = AssetDatabase.GenerateUniqueAssetPath(meshFolder + "/" + assetFile);
        GameObject editableRoot = null;
        bool hasAsset = false;
        int piecesMade = 0;

        try
        {
            editableRoot = new GameObject(outputName);
            Undo.RegisterCreatedObjectUndo(editableRoot, "Make metro pieces editable");
            editableRoot.transform.SetParent(station.transform, false);

            // We keep an asset container so the mesh data is saved across Unity sessions.
            Mesh container = new Mesh { name = "Metro editable mesh collection" };
            AssetDatabase.CreateAsset(container, meshPath);
            hasAsset = true;

            Dictionary<Transform, Transform> clonedTransforms =
                new Dictionary<Transform, Transform>();

            foreach (MeshFilter mf in filters)
            {
                if (mf == null || mf.sharedMesh == null) continue;
                Mesh source = mf.sharedMesh;
                if (!source.isReadable)
                    throw new Exception("Mesh still non-readable: " + source.name);

                MeshRenderer sourceRenderer = mf.GetComponent<MeshRenderer>();
                if (sourceRenderer == null) continue;

                // Replicate EVERY local transform between the source mesh and the
                // station, so Unity uses exactly the same coordinates and orientation.
                Transform clonedFilter = CopyTransformChain(mf.transform, station.transform,
                    editableRoot.transform, clonedTransforms);
                if (!mf.gameObject.activeInHierarchy)
                    clonedFilter.gameObject.SetActive(false);

                Vector3[] positions = source.vertices;
                Vector3[] normals = source.normals;
                Vector2[] uvs = source.uv;
                Material[] materials = sourceRenderer.sharedMaterials;

                for (int submesh = 0; submesh < source.subMeshCount; submesh++)
                {
                    int[] triangles = source.GetTriangles(submesh);
                    if (triangles.Length == 0) continue;

                    string materialName = submesh < materials.Length && materials[submesh] != null
                        ? materials[submesh].name : "Material " + submesh;
                    GameObject category = new GameObject(materialName);
                    category.transform.SetParent(clonedFilter, false);
                    category.isStatic = sourceRenderer.gameObject.isStatic;
                    Material material = submesh < materials.Length ? materials[submesh] : null;

                    List<List<int>> components = SplitTrianglesByPosition(triangles, positions);
                    for (int i = 0; i < components.Count; i++)
                    {
                        Vector3 pivot;
                        Mesh pieceMesh = BuildPiece(components[i], triangles,
                            positions, normals, uvs, out pivot);
                        string label = materialName + " " + (i + 1).ToString("D3");
                        pieceMesh.name = label;
                        AssetDatabase.AddObjectToAsset(pieceMesh, meshPath);

                        GameObject piece = new GameObject(label);
                        piece.transform.SetParent(category.transform, false);
                        piece.transform.localPosition = pivot;
                        piece.transform.localRotation = Quaternion.identity;
                        piece.transform.localScale = Vector3.one;
                        piece.isStatic = sourceRenderer.gameObject.isStatic;

                        MeshFilter filter = piece.AddComponent<MeshFilter>();
                        filter.sharedMesh = pieceMesh;
                        MeshRenderer renderer = piece.AddComponent<MeshRenderer>();
                        renderer.sharedMaterial = material;
                        renderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                        renderer.receiveShadows = sourceRenderer.receiveShadows;
                        renderer.enabled = sourceRenderer.enabled;

                        // Colliders use exactly the same local mesh coordinates.
                        if (IsAxisAlignedBox(pieceMesh))
                        {
                            BoxCollider box = piece.AddComponent<BoxCollider>();
                            box.center = pieceMesh.bounds.center;
                            box.size = pieceMesh.bounds.size;
                        }
                        else
                        {
                            MeshCollider collider = piece.AddComponent<MeshCollider>();
                            collider.sharedMesh = pieceMesh;
                        }
                        piecesMade++;
                    }
                }
            }

            if (piecesMade == 0) throw new Exception("No pieces were generated.");
            AssetDatabase.SaveAssets();
            Undo.RecordObject(original.gameObject, "Disable combined metro group");
            original.gameObject.SetActive(false);
            Selection.activeGameObject = editableRoot;
            Debug.Log("DEAD FREQUENCY: Created " + piecesMade + " editable pieces from Unity's actual imported meshes. Original group disabled and kept as a backup.");
            EditorUtility.DisplayDialog("Split complete",
                "Created " + piecesMade + " independently selectable pieces.\n\nCheck the geometry BEFORE saving the scene. If anything looks wrong, use Ctrl+Z or disable '" + outputName + "' and re-enable '" + sourceName + "'.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            if (editableRoot != null) UnityEngine.Object.DestroyImmediate(editableRoot);
            if (hasAsset) AssetDatabase.DeleteAsset(meshPath);
            original.gameObject.SetActive(true);
            EditorUtility.DisplayDialog("Split failed",
                ex.Message + "\n\nYour original group has been kept.", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static Transform CopyTransformChain(Transform source, Transform station,
        Transform newRoot, Dictionary<Transform, Transform> clones)
    {
        Transform found;
        if (clones.TryGetValue(source, out found)) return found;
        if (source == station) return newRoot;
        if (source == null) throw new Exception("Unexpected parent transform outside station.");

        Transform parent = CopyTransformChain(source.parent, station, newRoot, clones);
        GameObject copy = new GameObject(source.name + " (transform)");
        copy.transform.SetParent(parent, false);
        copy.transform.localPosition = source.localPosition;
        copy.transform.localRotation = source.localRotation;
        copy.transform.localScale = source.localScale;
        copy.isStatic = source.gameObject.isStatic;
        clones[source] = copy.transform;
        return copy.transform;
    }

    // Union triangles that share a position in the *imported Unity mesh*.
    // Two faces of the same box may have different UV vertices, but their
    // spatially coincident coordinates are identical.
    private static List<List<int>> SplitTrianglesByPosition(int[] triangles, Vector3[] positions)
    {
        int count = triangles.Length / 3;
        int[] parent = new int[count];
        for (int i = 0; i < count; i++) parent[i] = i;
        Dictionary<Vector3, int> seen = new Dictionary<Vector3, int>();

        for (int i = 0; i < count; i++)
        {
            for (int k = 0; k < 3; k++)
            {
                Vector3 pos = positions[triangles[i * 3 + k]];
                int previous;
                if (seen.TryGetValue(pos, out previous)) Union(parent, i, previous);
                else seen.Add(pos, i);
            }
        }

        var groups = new Dictionary<int, List<int>>();
        var order = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int root = Find(parent, i);
            List<int> indices;
            if (!groups.TryGetValue(root, out indices))
            {
                indices = new List<int>();
                groups.Add(root, indices);
                order.Add(root);
            }
            indices.Add(i);
        }
        var result = new List<List<int>>();
        foreach (int root in order) result.Add(groups[root]);
        return result;
    }

    private static int Find(int[] parent, int x)
    {
        while (parent[x] != x)
        {
            parent[x] = parent[parent[x]];
            x = parent[x];
        }
        return x;
    }

    private static void Union(int[] parent, int a, int b)
    {
        a = Find(parent, a);
        b = Find(parent, b);
        if (a != b) parent[b] = a;
    }

    private static Mesh BuildPiece(List<int> component, int[] sourceTriangles,
        Vector3[] positions, Vector3[] normals, Vector2[] uvs, out Vector3 pivot)
    {
        Vector3 minimum = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 maximum = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        foreach (int tri in component)
        {
            for (int k = 0; k < 3; k++)
            {
                Vector3 p = positions[sourceTriangles[3 * tri + k]];
                minimum = Vector3.Min(minimum, p);
                maximum = Vector3.Max(maximum, p);
            }
        }
        pivot = (minimum + maximum) * 0.5f;

        var verts = new List<Vector3>(component.Count * 3);
        var norm = new List<Vector3>(component.Count * 3);
        var uv = new List<Vector2>(component.Count * 3);
        var indices = new List<int>(component.Count * 3);
        bool hasNormals = normals != null && normals.Length == positions.Length;
        bool hasUvs = uvs != null && uvs.Length == positions.Length;

        foreach (int tri in component)
        {
            for (int k = 0; k < 3; k++)
            {
                int src = sourceTriangles[tri * 3 + k];
                indices.Add(verts.Count);
                verts.Add(positions[src] - pivot);
                if (hasNormals) norm.Add(normals[src]);
                if (hasUvs) uv.Add(uvs[src]);
            }
        }

        Mesh mesh = new Mesh();
        if (verts.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);
        if (hasNormals) mesh.SetNormals(norm);
        else mesh.RecalculateNormals();
        if (hasUvs) mesh.SetUVs(0, uv);
        mesh.RecalculateBounds();
        return mesh;
    }

    private static bool IsAxisAlignedBox(Mesh mesh)
    {
        if (mesh.triangles.Length != 36) return false;
        Bounds b = mesh.bounds;
        HashSet<Vector3> points = new HashSet<Vector3>();
        foreach (Vector3 v in mesh.vertices)
        {
            if (!AtEdge(v.x, b.min.x, b.max.x) ||
                !AtEdge(v.y, b.min.y, b.max.y) ||
                !AtEdge(v.z, b.min.z, b.max.z)) return false;
            points.Add(v);
        }
        return points.Count == 8;
    }

    private static bool AtEdge(float val, float low, float high)
    {
        const float eps = 0.0001f;
        return Mathf.Abs(val - low) < eps || Mathf.Abs(val - high) < eps;
    }
}
