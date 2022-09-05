using UnityEditor;
using UnityEngine;

namespace PlanetEngine
{
    internal static class PlanetEngineEditor
    {

        /// <summary>
        /// A menu option which enables creating a planet from the 3D object menu.
        /// </summary>
        [MenuItem("GameObject/3D Object/Planet", false, 40)]
        static void AddPlanetCreatorToScene()
        {
            CreateTag("PlanetEngine");
            CreateAssetFolder();
            GameObject planet = new GameObject("Planet");
            planet.tag = "PlanetEngine";
            planet.AddComponent<PreviewPlanet>();
            Selection.activeTransform = planet.transform;
        }

        // Creates a tag in the editor.
        static void CreateTag(string tag)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
            if (asset != null)
            {
                var so = new SerializedObject(asset);
                var tags = so.FindProperty("tags");

                var numTags = tags.arraySize;
                for (int i = 0; i < numTags; i++)
                {
                    var existingTag = tags.GetArrayElementAtIndex(i);
                    if (existingTag.stringValue == tag) return;
                }

                tags.InsertArrayElementAtIndex(numTags);
                tags.GetArrayElementAtIndex(numTags).stringValue = tag;
                so.ApplyModifiedProperties();
                so.Update();
            }
        }

        static void CreateAssetFolder()
        {
            if (AssetDatabase.IsValidFolder("Assets/PlanetEngineData")) return;
            AssetDatabase.CreateFolder("Assets", "PlanetEngineData");
            AssetDatabase.CreateFolder("Assets/PlanetEngineData", "Resources");
            AssetDatabase.CreateFolder("Assets/PlanetEngineData/Resources", "Presets");

            string[] presetAssets = new string[] {
                "black.png","blue.png","brown.png","darkblue.png","darkgreen.png","darkgrey.png","darkred.png","darkyellow.png",
                "green.png","grey.png","orange.png","red.png","white.png","yellow.png", "earth.asset", "mars.asset", "moon.asset"
            };

            foreach (string presetAsset in presetAssets)
            {
                AssetDatabase.LoadAssetAtPath<Texture2D>(ProceduralData.PackageAssetPath + presetAsset);
                if (!AssetDatabase.CopyAsset(ProceduralData.PackageAssetPath + presetAsset, ProceduralData.AssetPath + "Presets/" + presetAsset))
                    Debug.LogError("Copy failed: " + presetAsset);
            }
        }
    }
}