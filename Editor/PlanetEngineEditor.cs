using System;
using UnityEditor;
using UnityEngine;


/**********************************************************************
 * 
 *                      The planet engine editor
 *      
 * 
 **********************************************************************/
namespace PlanetEngine {
	public static class PlanetEngineEditor {
        
		#region Private static methods
        [MenuItem("GameObject/3D Object/Planet", false, 40)]
		static void AddPlanetCreatorToScene()
		{
			CreateTag("PlanetEngine");
			GameObject planet = new GameObject("Planet");
			planet.AddComponent<PreviewPlanet>();
			Selection.activeTransform = planet.transform;
		}

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
        #endregion
    }
}