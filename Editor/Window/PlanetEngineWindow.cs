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

    Vector2 scrollManager = new Vector2();
    Texture2D PElogo, Dlogo;
    Views currentView = Views.MANAGER;

    [MenuItem("Window/Planet Engine")]
    public static void ShowWindow() {
        PlanetEngineWindow wnd = GetWindow<PlanetEngineWindow>();
        wnd.titleContent = new GUIContent("The Planet Engine");
    }

    private void Awake() {
        // load in images from package
        PElogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.douwco.theplanetengine/Editor/Icons/PElogo.png", typeof(Texture2D));
        Dlogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.douwco.theplanetengine/Editor/Icons/DouwcoLogo.png", typeof(Texture2D));
    }

    public void OnFocus() {
        // update styles if new ones are implemented
        Stylesheet.InitStyles();
    }

    public void OnGUI() {
        GUIheader();
        GUItoolbar();
        GUILayout.BeginVertical(Stylesheet.headingStyle, GUILayout.ExpandHeight(true));
        GUIshowview();

        GUILayout.EndVertical();
        GUIfooter();        
    }

    void GUIheader() {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal(Stylesheet.headingStyle);
        GUILayout.Label(PElogo, GUILayout.Width(50), GUILayout.Height(50));
        GUILayout.BeginVertical(GUILayout.Height(50));
        GUILayout.Label("The Planet Engine", Stylesheet.titleStyle);
        if (PlanetEngine.GetActivePlanet() != null) GUILayout.Label(PlanetEngine.GetActivePlanet().name, Stylesheet.subtitleStyle);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Box(GUIContent.none, Stylesheet.horizontalLine);
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
        if (GUILayout.Button("P")) {
            currentView = Views.MANAGER;
        }
        if (PlanetEngine.GetActivePlanet() == null) GUI.enabled = false;
        if (GUILayout.Button("G")) {
            currentView = Views.GENERATOR;
        }
        if (true) GUI.enabled = false;
        if (GUILayout.Button("E")) {
            currentView = Views.EDITOR;
        }
        GUI.enabled = true;
        GUILayout.Label("", GUILayout.ExpandWidth(true)); // spacer
        GUILayout.EndHorizontal();
    }

    void GUIshowview() {
        switch (currentView) {
            case Views.MANAGER: {
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.ExpandWidth(true)); // spacer
                if (GUILayout.Button("Create new planet", GUILayout.Width(150))) { NewPlanetPopup.Popup(); }
                GUILayout.Label("", GUILayout.ExpandWidth(true)); // spacer
                GUILayout.EndHorizontal();
                scrollManager = GUILayout.BeginScrollView(scrollManager, Stylesheet.headingStyle);
                foreach(string planetName in PlanetEngine.GetAllPlanetNames()) {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Load", GUILayout.Width(50))) { 
                        
                    };
                    if (GUILayout.Button("Delete", GUILayout.Width(55))) {

                    };
                    GUILayout.Label(planetName, Stylesheet.selectingStyle);
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndScrollView();
                break;
            }
        }
    }
}
