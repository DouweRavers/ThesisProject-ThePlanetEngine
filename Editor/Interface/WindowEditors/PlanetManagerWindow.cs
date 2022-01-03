using UnityEditor;
using UnityEngine;

/**********************************************************************
 * 
 *                      The planet engine tool
 *      This class is the body of all other subtools. It will toggle between
 *      different subtools while providing a uniform style and layout.
 * 
 **********************************************************************/
namespace PlanetEngine {

	public class PlanetManagerWindow : EditorWindow {

		void OnSelectionChange() {
			Repaint();
		}

		void OnGUI() {
			GUIheader();
			if (GUILayout.Button("Create planet")) {
				PlanetEngineEditor.CreatePlanet("Planet");
			}

			GUILayout.Label(" ", GUILayout.ExpandHeight(true));
			GUIfooter();
		}

		void GUIheader() {
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label("", GUILayout.ExpandWidth(true));
			GUILayout.Label(InterfaceManager.PlanetEnginelogo, GUILayout.Width(80), GUILayout.Height(100));
			GUILayout.Label("The Planet Engine", Stylesheet.titleStyle, GUILayout.Height(100));
			GUILayout.Label("", GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			GUILayout.Box(GUIContent.none, Stylesheet.horizontalLine);
			if (PlanetEngineEditor.isSelectedObjectPlanet()) GUILayout.Label(PlanetEngineEditor.getSelectedPlanet().name, Stylesheet.subtitleStyle);
			GUILayout.EndVertical();
		}

		void GUIfooter() {
			GUILayout.BeginHorizontal(Stylesheet.footingStyle, GUILayout.ExpandWidth(true));
			GUILayout.Label("", GUILayout.ExpandWidth(true));
			GUILayout.Label(InterfaceManager.Douwcologo, GUILayout.Width(50f), GUILayout.Height(50f));
			GUILayout.EndHorizontal();
		}
	}
}