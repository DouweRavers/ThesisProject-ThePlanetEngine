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
            Texture2D grass = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.douwco.theplanetengine/Editor/Resources/RuntimeAssets/Grass.jpg", typeof(Texture2D));
            if(!AssetDatabase.CopyAsset("Packages/com.douwco.theplanetengine/Editor/Resources/RuntimeAssets/Grass.jpg", "Assets/PlanetEngineData/Resources/Grass.jpg")) Debug.LogError("Copy failed");
        }
    }
}