using UnityEditor;
using UnityEngine;


public class PlanetEngineTool : EditorWindow
{
    private GUIStyle titleStyle, headingStyle, defaultStyle, horizontalLine;
    private Texture2D PElogo;
    private string PEassetsFolderPath;

    [MenuItem("Window/The Planet Engine")]
    public static void ShowWindow()
    {
        PlanetEngineTool wnd = GetWindow<PlanetEngineTool>();
        wnd.titleContent = new GUIContent("The Planet Engine");
    }

    public void OnFocus()//Awake() 
    {
        // create styles
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 30;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        headingStyle = new GUIStyle();
        headingStyle.margin = new RectOffset(20, 20, 20, 20);
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        horizontalLine.margin = new RectOffset(10, 10, 4, 4);
        horizontalLine.fixedHeight = 2;
        defaultStyle = new GUIStyle();
        defaultStyle.fontSize = 12;
        titleStyle.normal.textColor = Color.white;

        // loading package assets
        PElogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.douwco.theplanetengine/Editor/Icons/PElogo.png", typeof(Texture2D));

        // Create or find planet asset folder
        string[] PEmanagerGUID = AssetDatabase.FindAssets("DoNotRemove l:PlanetEngine", new [] { "Assets" });
        if (PEmanagerGUID.Length == 0) {
            AssetDatabase.CreateFolder("Assets", "PEassets");
            Texture2D dummyTexture = new Texture2D(1, 1);
            AssetDatabase.CreateAsset(dummyTexture, "Assets/PEassets/DoNotRemove");
            AssetDatabase.SetLabels(dummyTexture, new [] { "PlanetEngine" });
        } else {
            PEassetsFolderPath = AssetDatabase.GUIDToAssetPath(PEmanagerGUID[0]).Replace("DoNotRemove", "");
            Debug.Log(PEassetsFolderPath);
        }
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        GUILayout.BeginHorizontal(headingStyle);
        GUILayout.Label(PElogo, GUILayout.Width(50f), GUILayout.Height(50f));
        GUILayout.Label("The Planet Engine", titleStyle, GUILayout.Height(50f));
        GUILayout.EndHorizontal();
        GUILayout.Box(GUIContent.none, horizontalLine);
        GUILayout.EndVertical();
    }
}