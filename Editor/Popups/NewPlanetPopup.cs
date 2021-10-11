using UnityEditor;
using UnityEngine;

/**********************************************************************
 * 
 *                      The planet engine tool
 *      This class is the body of all other subtools. It will toggle between
 *      different subtools while providing a uniform style and layout.
 * 
 **********************************************************************/

internal class NewPlanetPopup : EditorWindow {
    private string planetName = "";

    public static void Popup() {
        Stylesheet.InitStyles();
        NewPlanetPopup wnd = CreateInstance<NewPlanetPopup>();
        Rect sceneViewPos = GetWindow<SceneView>().position;
        wnd.position = new Rect(sceneViewPos.x + sceneViewPos.width / 2 - 150, sceneViewPos.y + sceneViewPos.height / 2 - 50, 300, 150);
        wnd.ShowPopup();
    }

    void OnGUI() {
        GUILayout.Label("Create new planet", Stylesheet.titleStyle, GUILayout.ExpandHeight(true));
        planetName = EditorGUILayout.TextField("Planet name:", planetName);
        if (planetName.Length == 0) { // check if name already exists or check for no name

            GUILayout.Label("Invalid name or name already taken", Stylesheet.redTextStyle);
            GUI.enabled = false;
        }
        GUILayout.BeginHorizontal(Stylesheet.headingStyle);
        if (GUILayout.Button("Submit")) {
            PlanetEngineEditor.CreatePlanet(planetName);
            Close();
        }
        GUI.enabled = true;
        if (GUILayout.Button("Cancel")) Close();
        GUILayout.EndHorizontal();
    }
}