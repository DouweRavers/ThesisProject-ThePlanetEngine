using UnityEditor;
using UnityEngine;

/**********************************************************************
 * 
 *                      The planet engine tool
 *      This class is the body of all other subtools. It will toggle between
 *      different subtools while providing a uniform style and layout.
 * 
 **********************************************************************/

public class PlanetEngineWindow : EditorWindow {

    Texture2D PElogo, Dlogo;
    View activeView;
    Vector2 scroll;

    [MenuItem("Window/Douwco/Planet Engine")]
    public static void ShowWindow() {
        PlanetEngineWindow wnd = GetWindow<PlanetEngineWindow>();
        wnd.titleContent = new GUIContent("The Planet Engine");
    }

    [MenuItem("GameObject/3D Object/Planet", false, 40)]
    public static void createPlanet() {
        NewPlanetPopup.Popup();
    }

    private void Awake() {
        // configure view
        activeView = View.GetViewOfType(Views.NOPLANET);

        // load in images from package
        PElogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.douwco.theplanetengine/Icons/PElogo.png", typeof(Texture2D));
        Dlogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.douwco.theplanetengine/Icons/DouwcoLogo.png", typeof(Texture2D));
    }

    public void OnFocus() {
        // update styles if new ones are implemented (can be changed to awake once development is done)
        Stylesheet.InitStyles();
    }

    public void OnGUI() {
        GUIheader();
        GUItoolbar();

        // body
        GUILayout.BeginScrollView(scroll, Stylesheet.headingStyle, GUILayout.ExpandHeight(true));
        activeView.ShowGUI();
        GUILayout.EndScrollView();

        GUIfooter();
    }

    void GUIheader() {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.ExpandWidth(true));
        GUILayout.Label(PElogo, GUILayout.Width(250), GUILayout.Height(140));
        GUILayout.Label("", GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        GUILayout.Box(GUIContent.none, Stylesheet.horizontalLine);
        if (PlanetEngineEditor.isSelectedObjectPlanet()) GUILayout.Label(PlanetEngineEditor.getSelectedPlanet().name, Stylesheet.subtitleStyle);
        GUILayout.EndVertical();
    }

    void GUIfooter() {
        GUILayout.BeginHorizontal(Stylesheet.footingStyle, GUILayout.ExpandWidth(true));
        GUILayout.Label("", GUILayout.ExpandWidth(true));
        GUILayout.Label(Dlogo, GUILayout.Width(50f), GUILayout.Height(50f));
        GUILayout.EndHorizontal();
    }

    void GUItoolbar() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.ExpandWidth(true)); // spacer
        if (!PlanetEngineEditor.isSelectedObjectPlanet()) {
            activeView = View.GetViewOfType(Views.NOPLANET);
            GUI.enabled = false;
        } else if (activeView.viewType == Views.NOPLANET) {
            activeView = View.GetViewOfType(Views.GENERATOR);
        }
        if (GUILayout.Button("G")) {
            activeView = View.GetViewOfType(Views.GENERATOR);
        }
        if (true) GUI.enabled = false;
        if (GUILayout.Button("E")) {
            activeView = View.GetViewOfType(Views.EDITOR);
        }
        GUI.enabled = true;
        GUILayout.Label("", GUILayout.ExpandWidth(true)); // spacer
        GUILayout.EndHorizontal();
    }
}
