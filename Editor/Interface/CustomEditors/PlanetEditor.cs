using UnityEngine;
using UnityEditor;

namespace PlanetEngine {

	// [CustomEditor(typeof(Planet))]
	//	[CanEditMultipleObjects]
	public class PlanetEditor : Editor {
		static bool enableHide = true;
		Planet planet;
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
			planet = target as Planet;
		}

		void SetVisibilty() {
			// Hide Components used by the Planet editor
			if (enableHide) {
				planet.GetComponent<LODGroup>().hideFlags = HideFlags.HideInInspector;
				for (int i = 0; i < planet.transform.childCount; i++) {
					Transform child = planet.transform.GetChild(i);
					if (child.gameObject.tag.Equals("PlanetEngine")) {
						SceneVisibilityManager.instance.DisablePicking(child.gameObject, true);
						child.gameObject.hideFlags = HideFlags.HideInHierarchy;
					}
				}
			} else {
				planet.GetComponent<LODGroup>().hideFlags = HideFlags.None;
				for (int i = 0; i < planet.transform.childCount; i++) {
					Transform child = planet.transform.GetChild(i);
					if (child.gameObject.tag.Equals("PlanetEngine")) {
						SceneVisibilityManager.instance.EnablePicking(child.gameObject, true);
						child.gameObject.hideFlags = HideFlags.None;
					}
				}
			}
		}
	}
}