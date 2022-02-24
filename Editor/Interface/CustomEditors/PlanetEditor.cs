using UnityEngine;
using UnityEditor;

namespace PlanetEngine {

	[CustomEditor(typeof(Planet))]
	public class PlanetEditor : Editor {
		static bool enableHide = true;
		Planet planet;
		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(" - Properties");

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Seed: ", GUILayout.Width(120f));
			EditorGUILayout.IntField(100);
			EditorGUILayout.LabelField(" ", GUILayout.ExpandWidth(true));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Radius: ", GUILayout.Width(120f));
			EditorGUILayout.Slider(50f, 1f, 100f);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("LOD spheres: ", GUILayout.Width(120f));
			EditorGUILayout.IntSlider(3, 1, 4);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Landmass Colors: ", GUILayout.Width(120f));
			EditorGUILayout.ColorField(Color.black);
			EditorGUILayout.ColorField(Color.black);
			EditorGUILayout.LabelField(" ", GUILayout.ExpandWidth(true));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Ocean Colors: ", GUILayout.Width(120f));
			EditorGUILayout.ColorField(Color.black);
			EditorGUILayout.ColorField(Color.black);
			EditorGUILayout.LabelField(" ", GUILayout.ExpandWidth(true));
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(10);

			if (GUILayout.Button("Generate")) planet.CreateNewPlanet();
			EditorGUILayout.Space(10);

			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(" - Debug");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Enable hide mode", GUILayout.Width(120f));
			enableHide = EditorGUILayout.Toggle(enableHide, GUILayout.Width(20f));
			EditorGUILayout.LabelField(" ", GUILayout.ExpandWidth(true));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
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