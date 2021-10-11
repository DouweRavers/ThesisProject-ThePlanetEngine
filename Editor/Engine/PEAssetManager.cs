using UnityEditor;
using UnityEngine;

internal class PEAssetManager : ScriptableObject {
    private string PEassetsFolderPath = "Assets/PEassets/";

    void Awake() {
        UpdateAssetPath();
    }

    private void UpdateAssetPath() {
        // Create or find the planet engine asset folder
        string[] PEmanagerGUID = AssetDatabase.FindAssets("PlanetEngineRoot l:PlanetEngine", new[] { "Assets" });
        if (PEmanagerGUID.Length == 0) {
            AssetDatabase.CreateFolder("Assets", "PEassets");
            PEassetsFolderPath = "Assets/PEassets/";
            TextAsset PEroot = new TextAsset();
            AssetDatabase.CreateAsset(PEroot, PEassetsFolderPath + "PlanetEngineRoot.json");
            AssetDatabase.SetLabels(PEroot, new[] { "PlanetEngine" });
        } else {
            PEassetsFolderPath = AssetDatabase.GUIDToAssetPath(PEmanagerGUID[0]).Replace("PlanetEngineRoot.json", "");
        }
    }

    public GameObject SavePlanetObjectToFile(GameObject planetObject) {
        UpdateAssetPath();
        if (AssetDatabase.FindAssets(planetObject.name + ".prefab l:PlanetEngine", new[] { PEassetsFolderPath }).Length > 0) {
            planetObject = PrefabUtility.SavePrefabAsset(planetObject);
        } else {
            planetObject = PrefabUtility.SaveAsPrefabAsset(planetObject, PEassetsFolderPath + planetObject.name + ".prefab");
            if (planetObject != null) AssetDatabase.SetLabels(planetObject, new[] { "PlanetEngine" });
        }
        return planetObject;
    }

    public string[] ScanSavedPlanets() {
        UpdateAssetPath();
        string[] planetFiles = AssetDatabase.FindAssets("*.prefab l:PlanetEngine", new string[] { PEassetsFolderPath });
        for (int i = 0; i < planetFiles.Length; i++) {
            planetFiles[i] = AssetDatabase.GUIDToAssetPath(planetFiles[i]).Replace(PEassetsFolderPath, "");
        }
        return planetFiles;
    }

    internal void DeletePlanet(string planetName) {
        AssetDatabase.DeleteAsset(PEassetsFolderPath + planetName + ".prefab");
    }

    internal GameObject LoadPlanet(string planetName, bool open = true) {
        GameObject planetObject = null;
        if (open) {
            planetObject = AssetDatabase.LoadAssetAtPath<GameObject>(PEassetsFolderPath + planetName + ".prefab");
            AssetDatabase.OpenAsset(planetObject);
        }
        return planetObject;
    }
}
