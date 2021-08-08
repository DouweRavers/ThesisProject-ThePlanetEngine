using System;
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

    public void SavePlanetObjectToFile(PlanetRoot planetRoot) {
        UpdateAssetPath();
        string jsonPlanet = JsonUtility.ToJson(planetRoot);
        TextAsset planetFile = new TextAsset(jsonPlanet);
        AssetDatabase.CreateAsset(planetFile, PEassetsFolderPath + planetRoot.planetName + ".json");
        AssetDatabase.SetLabels(planetFile, new[] { "PlanetEngine" });
    }

    public string[] ScanSavedPlanets() {
        UpdateAssetPath();
        string[] planetFiles = AssetDatabase.FindAssets("* l:PlanetEngine", new string[] { PEassetsFolderPath });
        string[] reducedArray = new string[planetFiles.Length-1];
        for (int i = 0, j = 0; i < planetFiles.Length; i++, j++) {
            string file = AssetDatabase.GUIDToAssetPath(planetFiles[i]);
            file = file.Replace(PEassetsFolderPath, "");
            file = file.Replace(".json", "");
            if (file.Contains("PlanetEngineRoot")) j--;
            else reducedArray[j] = file;
        }
        return reducedArray;
    }

    internal void DeletePlanet(string planetName) {
        AssetDatabase.DeleteAsset(PEassetsFolderPath + planetName + ".json");
    }

    internal string LoadPlanet(string planetName) {
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(PEassetsFolderPath + planetName + ".json");
        return textAsset.text;
    }
}
