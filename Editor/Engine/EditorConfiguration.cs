using System;
using UnityEditor;
using UnityEngine;

namespace PlanetEngine
{
	public static class EditorConfigurator
	{
		public static void CreateTag(string tag)
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
	}
}