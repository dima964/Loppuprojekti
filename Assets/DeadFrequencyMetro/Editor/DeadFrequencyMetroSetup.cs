// DEAD FREQUENCY station import helper - Unity 6.x Editor only.
// Put this file in an "Editor" folder (as packaged).
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class DeadFrequencyMetroSetup
{
    private const string Base = "Assets/DeadFrequencyMetro";

    // Model order and files match the supplied OBJ files.
    private static readonly string[] ModelFiles =
    {
        "DF_Metro_Architecture.obj",
        "DF_Metro_Props.obj",
        "DF_Metro_Ceilings_OPTIONAL.obj"
    };

    private static readonly string[] MaterialNames =
    {
        "concrete", "floor", "platform", "darkfloor", "wall", "walltile",
        "trim", "iron", "rust", "wood", "crate", "safety", "red",
        "emergency", "green", "blue", "light", "screen", "dark",
        "gravel", "rail", "paper", "mattress", "pillow", "black"
    };

    [MenuItem("Tools/Dead Frequency/Build Metro Station")]
    public static void BuildMetroStation()
    {
        foreach (string file in ModelFiles)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Base + "/Models/" + file) == null)
            {
                EditorUtility.DisplayDialog("Station files missing",
                    "Couldn't find " + file + ".\n\nCopy the complete DeadFrequencyMetro folder into your project's Assets directory, then wait for import.", "OK");
                return;
            }
        }

        Dictionary<string, Material> materials = CreateMaterials();
        GameObject root = new GameObject("DEAD FREQUENCY - Metro Station");
        Undo.RegisterCreatedObjectUndo(root, "Create metro station");

        for (int index = 0; index < ModelFiles.Length; index++)
        {
            string assetPath = Base + "/Models/" + ModelFiles[index];
            GameObject original = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            GameObject model = PrefabUtility.InstantiatePrefab(original) as GameObject;
            if (model == null)
            {
                Debug.LogError("Could not instantiate: " + assetPath);
                continue;
            }

            Undo.RegisterCreatedObjectUndo(model, "Create station geometry");
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            // Use prefab-friendly names and allow easy ceiling visibility toggle.
            model.name = index == 0 ? "Architecture (floors and walls)" :
                         index == 1 ? "Props (furniture and lights)" :
                                      "Ceilings (optional - disable to edit rooms)";

            AssignMaterials(model, assetPath, materials);
            AddStaticMeshColliders(model);
        }

        GameObject lights = new GameObject("Lighting");
        Undo.RegisterCreatedObjectUndo(lights, "Create metro lighting");
        lights.transform.SetParent(root.transform, false);
        CreateLights(lights.transform);

        GameObject marker = new GameObject("Player Spawn - place existing Player here");
        Undo.RegisterCreatedObjectUndo(marker, "Create station spawn marker");
        marker.transform.SetParent(root.transform, false);
        marker.transform.localPosition = new Vector3(0f, 1.05f, -14f);

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
        Debug.Log("DEAD FREQUENCY: Metro station created. Player spawn is (0, 1.05, -14). Move your existing Player there; add no Rigidbody to the map. You can disable the Ceilings object while editing.");
    }

    private static Dictionary<string, Material> CreateMaterials()
    {
        string dir = Base + "/UnityMaterials";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder(Base, "UnityMaterials");

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Texture");

        var result = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
        foreach (string name in MaterialNames)
        {
            string matPath = dir + "/DF_" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.name = name;
                AssetDatabase.CreateAsset(mat, matPath);
            }
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Base + "/Textures/" + name + ".png");
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                mat.mainTexture = tex;
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            EditorUtility.SetDirty(mat);
            result[name] = mat;
        }
        AssetDatabase.SaveAssets();
        return result;
    }

    private static void AssignMaterials(GameObject model, string objPath, Dictionary<string, Material> mats)
    {
        var namesInFile = new List<string>();
        string content = File.ReadAllText(objPath);
        using (StringReader reader = new StringReader(content))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("usemtl "))
                {
                    string name = line.Substring(7).Trim();
                    if (!namesInFile.Contains(name)) namesInFile.Add(name);
                }
            }
        }

        MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer r in renderers)
        {
            Material[] previous = r.sharedMaterials;
            Material[] replacement = new Material[previous.Length];
            bool wholeModel = renderers.Length == 1 && previous.Length == namesInFile.Count;
            for (int i = 0; i < previous.Length; i++)
            {
                string source = previous[i] == null ? "" : previous[i].name;
                source = source.Replace(" (Instance)", "").Replace("_Mat", "");

                Material m;
                if (!mats.TryGetValue(source, out m))
                {
                    string childName = r.gameObject.name;
                    if (!mats.TryGetValue(childName, out m))
                    {
                        // The importer sometimes replaces every source material with
                        // Default-Material. For a single renderer the submesh order is
                        // the same as the OBJ's "usemtl" order.
                        string key = wholeModel ? namesInFile[i] : "concrete";
                        m = mats[key];
                    }
                }
                replacement[i] = m;
            }
            r.sharedMaterials = replacement;
        }
    }

    private static void AddStaticMeshColliders(GameObject model)
    {
        foreach (MeshFilter mf in model.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            MeshCollider collider = mf.GetComponent<MeshCollider>();
            if (collider == null) collider = mf.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mf.sharedMesh;
            collider.convex = false;
            mf.gameObject.isStatic = true;
        }
    }

    private static void CreateLights(Transform parent)
    {
        // X, Z, brightness in arbitrary intensity units; change in Inspector.
        AddLight(parent, "Control / Bedroom", 0f, -15f, 2.0f, 8f, new Color(1f, .72f, .43f));
        AddLight(parent, "Radio Room", -14f, -14f, 1.7f, 7f, new Color(1f, .72f, .45f));
        AddLight(parent, "Maintenance", -15f, -1.5f, 1.5f, 8f, new Color(.75f, .85f, 1f));
        AddLight(parent, "Main Corridor West", -5f, 0f, 1.35f, 8f, new Color(.75f, .86f, 1f));
        AddLight(parent, "Main Corridor East", 10f, 0f, 1.35f, 9f, new Color(.75f, .86f, 1f));
        AddLight(parent, "Storage", 13.7f, 8.5f, 1.4f, 8f, new Color(1f, .84f, .62f));
        AddLight(parent, "Signal Repair", 27.5f, -1.1f, 1.6f, 8f, new Color(.64f, .83f, 1f));
        AddLight(parent, "Corridor to Platform", 0f, 8f, 1.1f, 9f, new Color(.70f, .82f, 1f));
        AddLight(parent, "Metro Platform Left", -8f, 18.75f, 1.5f, 10f, new Color(.72f, .82f, 1f));
        AddLight(parent, "Metro Platform Right", 8f, 18.75f, 1.5f, 10f, new Color(.72f, .82f, 1f));
        AddLight(parent, "Metro Platform Edge", 0f, 23.1f, 1.5f, 10f, new Color(.68f, .79f, 1f));
        AddLight(parent, "Trap Enemy Zone", -30f, -3.8f, 2.1f, 11f, new Color(1f, .13f, .10f));
    }

    private static void AddLight(Transform parent, string name, float x, float z,
                                 float intensity, float range, Color color)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent, false);
        item.transform.localPosition = new Vector3(x, 3.05f, z);
        Light l = item.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = range;
        l.intensity = intensity;
        l.color = color;
        l.shadows = LightShadows.None; // Keep it cheap for an indie student game.
    }
}
