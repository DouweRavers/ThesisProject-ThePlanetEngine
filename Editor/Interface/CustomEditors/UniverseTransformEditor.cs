using UnityEngine;
using UnityEditor;

namespace PlanetEngine {

	[CustomEditor(typeof(UniverseTransform))]
	//	[CanEditMultipleObjects]
	public class UniverseTransformEditor : Editor {
		UniverseTransform universeTransform;
		static bool enableHide = true;
		public override void OnInspectorGUI() {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Enable hide mode", GUILayout.Width(120f));
			enableHide = EditorGUILayout.Toggle(enableHide, GUILayout.Width(20f));
			EditorGUILayout.LabelField(" ", GUILayout.ExpandWidth(true));
			EditorGUILayout.EndHorizontal();


			// Call update functions
			SetVisibilty();
		}

		void Awake() {
			universeTransform = target as UniverseTransform;
		}

		void SetVisibilty() {
			if (enableHide) {
				universeTransform.transform.hideFlags = HideFlags.HideInInspector;
			} else {
				universeTransform.transform.hideFlags = HideFlags.None;
			}
		}
	}
}