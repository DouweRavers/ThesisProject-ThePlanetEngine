using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanetEngine
{

    [CustomEditor(typeof(PreviewPlanet))]
    public class PlanetCreatorTool : Editor
    {
        #region Private attributes
        PreviewPlanet planet;
        Dictionary<string, GUIStyle> styles;


        // temp
        bool showGradient = false;
        Mesh mesh;
        Texture2D texture2D;
        Color color;
        #endregion


        public override void OnInspectorGUI()
        {
            PlanetData testData = planet.Data.Copy();
            bool prevSet = planet.PreviewSettings;
            PreviewPhase phase = planet.Phase;

            ShowGenerationMenu();

            if (planet.Data != testData || prevSet != planet.PreviewSettings || phase != planet.Phase)
            {
                planet.Regenerate();
            }

        }

        void Awake()
        {
            planet = target as PreviewPlanet;
            texture2D = new Texture2D(64, 32);
            texture2D.filterMode = FilterMode.Point;
            CreateStyles();
            planet.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
            planet.GetComponent<MeshRenderer>().hideFlags = HideFlags.HideInInspector;
        }


        void ShowGenerationMenu()
        {
            ShowIndicatorMenu();
            ShowPanel();
            ShowNavigationMenu();

            void ShowIndicatorMenu()
            {
                GUILayout.BeginHorizontal(GUILayout.Height(50f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.BASICS), GUILayout.Height(40f), GUILayout.Width(40f))) planet.Phase = PreviewPhase.BASICS;
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.HEIGHTMAP), GUILayout.Height(40f), GUILayout.Width(40f))) planet.Phase = PreviewPhase.HEIGHTMAP;
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.CLIMATE), GUILayout.Height(40f), GUILayout.Width(40f))) planet.Phase = PreviewPhase.CLIMATE;
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.BIOMES), GUILayout.Height(40f), GUILayout.Width(40f))) planet.Phase = PreviewPhase.BIOMES;
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.VEGETATION), GUILayout.Height(40f), GUILayout.Width(40f))) planet.Phase = PreviewPhase.VEGETATION;

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUIStyle IndicatorStyle(PreviewPhase phase)
                {
                    return planet.Phase == phase ? styles["IndicatorSelected"] : styles["IndicatorLocked"];
                }
            }

            void ShowPanel()
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.MaxWidth(25f));
                GUILayout.BeginVertical(styles["BackgroundPanel"], GUILayout.Height(250f));
                switch (planet.Phase)
                {
                    case PreviewPhase.BASICS:
                        ShowCelestialMenu();
                        break;
                    case PreviewPhase.HEIGHTMAP:
                        ShowHeightMapMenu();
                        break;
                    case PreviewPhase.CLIMATE:
                        ShowClimateMenu();
                        break;
                    case PreviewPhase.BIOMES:
                        ShowBiomesMenu();
                        break;
                    case PreviewPhase.VEGETATION:
                        ShowVegetationMenu();
                        break;
                }
                GUILayout.EndVertical();
                GUILayout.Label("", GUILayout.MaxWidth(25f));
                GUILayout.EndHorizontal();
            }

            void ShowNavigationMenu()
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(50));
                GUILayout.Label("Randomize planet");
                if (GUILayout.Button(Resources.Load<Texture2D>("UI/RandomIcon"), GUILayout.Height(25f), GUILayout.Width(25f)))
                {

                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Resources.Load<Texture2D>("UI/LeftArrow"), GUILayout.Height(25f), GUILayout.Width(50f)))
                {
                    if (planet.Phase == PreviewPhase.BASICS) return;
                    planet.Phase--;
                }
                if (GUILayout.Button(Resources.Load<Texture2D>(planet.Phase == PreviewPhase.VEGETATION ? "UI/Add" : "UI/RightArrow"), GUILayout.Height(25f), GUILayout.Width(50f)))
                {
                    if (planet.Phase == PreviewPhase.VEGETATION) return;
                    planet.Phase++;
                }
                GUILayout.Label("", GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }
        }

        void ShowCelestialMenu()
        {
            ShowPanelHeader("Basics - " + planet.name);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed:", GUILayout.Width(100f));
            planet.Data.Seed = EditorGUILayout.IntField(planet.Data.Seed, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius:", GUILayout.Width(100f));
            planet.Data.Radius = EditorGUILayout.Slider(planet.Data.Radius, 1f, 100f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Detail levels:", GUILayout.Width(100f));
            planet.Data.LODSphereCount = EditorGUILayout.IntSlider(planet.Data.LODSphereCount, 2, 4);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Subdivisions:", GUILayout.Width(100f));
            planet.Data.MaxDepth = EditorGUILayout.IntSlider(planet.Data.MaxDepth, 2, 12);
            GUILayout.EndHorizontal();

            ShowPanelFooter(RandomizeCelestialProperties);

            int RandomizeCelestialProperties()
            {
                UnityEngine.Random.InitState(Time.frameCount);
                planet.Data.Seed = UnityEngine.Random.Range(0, 1000);
                planet.Data.Radius = UnityEngine.Random.Range(1f, 100f);
                planet.Data.LODSphereCount = UnityEngine.Random.Range(2, 5);
                planet.Data.MaxDepth = UnityEngine.Random.Range(2, 13);
                return 1;
            }
        }

        void ShowHeightMapMenu()
        {
            ShowPanelHeader("Height map - " + planet.name);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Continent size");
            EditorGUILayout.Slider(0.5f, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Oceans");
            EditorGUILayout.Toggle(false);
            GUILayout.EndHorizontal();

            showGradient = EditorGUILayout.Foldout(showGradient, "Ocean gradient");
            if (showGradient)
            {
                color = EditorGUILayout.ColorField(color);
                GUILayout.Label("", styles["texture"], GUILayout.Width(texture2D.width * 3), GUILayout.Height(texture2D.height * 3));
                Rect rect = GUILayoutUtility.GetLastRect();
                Event e = Event.current;
                if (e.type == EventType.MouseDown && rect.x < e.mousePosition.x && e.mousePosition.x < rect.x + rect.width &&
                    rect.y < e.mousePosition.y && e.mousePosition.y < rect.y + rect.height)
                {
                    int x = (int)(e.mousePosition.x - rect.x) / 3;
                    int y = (int)(rect.y + rect.width - e.mousePosition.y) / 3;
                    texture2D.SetPixel(x, y, color);
                    texture2D.SetPixel(x + 1, y, color);
                    texture2D.SetPixel(x - 1, y, color);
                    texture2D.SetPixel(x, y + 1, color);
                    texture2D.SetPixel(x, y - 1, color);
                    texture2D.Apply();
                    GUI.changed = true;
                }
            }

            ShowPanelFooter();
        }

        void ShowClimateMenu()
        {
            ShowPanelHeader("Climate - " + planet.name);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Solar heat");
            EditorGUILayout.Slider(0.5f, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Atmosphere");
            EditorGUILayout.Toggle(false);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Green house effect");
            EditorGUILayout.Slider(0.5f, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Humidity transfer");
            EditorGUILayout.Slider(0.5f, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Cloud color");
            EditorGUILayout.GradientField(new Gradient());
            GUILayout.EndHorizontal();

            ShowPanelFooter();
        }

        void ShowBiomesMenu()
        {
            ShowPanelHeader("Biomes - " + planet.name);

            if (GUILayout.Button("Biome"))
            {
                PopupWindow.Show(new Rect(50, 50, 50, 50), new PopupExample());
            }
            color = EditorGUILayout.ColorField(color);
            texture2D = (Texture2D)EditorGUILayout.ObjectField("terrain texture", texture2D, typeof(Texture2D), false);
            texture2D = (Texture2D)EditorGUILayout.ObjectField("heightmap texture", texture2D, typeof(Texture2D), false);
            float a = 0.1f, b = 0.5f;
            EditorGUILayout.MinMaxSlider("Height:", ref a, ref b, 0f, 1f);

            showGradient = EditorGUILayout.Foldout(showGradient, "graph");
            if (showGradient)
            {
                color = EditorGUILayout.ColorField(color);
                GUILayout.Label("", styles["texture"], GUILayout.Width(texture2D.width * 3), GUILayout.Height(texture2D.height * 3));
                Rect rect = GUILayoutUtility.GetLastRect();
                Event e = Event.current;
                if (e.type == EventType.MouseDown && rect.x < e.mousePosition.x && e.mousePosition.x < rect.x + rect.width &&
                    rect.y < e.mousePosition.y && e.mousePosition.y < rect.y + rect.height)
                {
                    int x = (int)(e.mousePosition.x - rect.x) / 3;
                    int y = (int)(rect.y + rect.width - e.mousePosition.y) / 3;
                    texture2D.SetPixel(x, y, color);
                    texture2D.SetPixel(x + 1, y, color);
                    texture2D.SetPixel(x - 1, y, color);
                    texture2D.SetPixel(x, y + 1, color);
                    texture2D.SetPixel(x, y - 1, color);
                    texture2D.Apply();
                    GUI.changed = true;
                }
            }

            ShowPanelFooter();
        }

        void ShowVegetationMenu()
        {
            ShowPanelHeader("Vegetation - " + planet.name);


            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("vegetation"))
            {
                PopupWindow.Show(new Rect(50, 50, 50, 50), new PopupExample());
            }
            if (GUILayout.Button("type"))
            {
                PopupWindow.Show(new Rect(50, 50, 50, 50), new PopupExample());
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Solar heat");
            EditorGUILayout.Slider(0.5f, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Resources.Load<Texture2D>("UI/Add")))
            {
            }
            if (GUILayout.Button(Resources.Load<Texture2D>("UI/RandomIcon")))
            {
            }
            if (GUILayout.Button("Biome X"))
            {
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            texture2D = (Texture2D)EditorGUILayout.ObjectField(texture2D, typeof(Texture2D), false, GUILayout.Width(250));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(2));
            GUILayout.Space(100);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("vegetation"))
            {
                PopupWindow.Show(new Rect(50, 50, 50, 50), new PopupExample());
            }
            if (GUILayout.Button("type"))
            {
                PopupWindow.Show(new Rect(50, 50, 50, 50), new PopupExample());
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Resources.Load<Texture2D>("UI/Add")))
            {
            }
            if (GUILayout.Button(Resources.Load<Texture2D>("UI/RandomIcon")))
            {
            }
            color = EditorGUILayout.ColorField(color);
            GUILayout.Label("", styles["texture"], GUILayout.Width(texture2D.width * 3), GUILayout.Height(texture2D.height * 3));
            Rect rect = GUILayoutUtility.GetLastRect();
            Event e = Event.current;
            if (e.type == EventType.MouseDown && rect.x < e.mousePosition.x && e.mousePosition.x < rect.x + rect.width &&
                rect.y < e.mousePosition.y && e.mousePosition.y < rect.y + rect.height)
            {
                int x = (int)(e.mousePosition.x - rect.x) / 3;
                int y = (int)(rect.y + rect.width - e.mousePosition.y) / 3;
                texture2D.SetPixel(x, y, color);
                texture2D.SetPixel(x + 1, y, color);
                texture2D.SetPixel(x - 1, y, color);
                texture2D.SetPixel(x, y + 1, color);
                texture2D.SetPixel(x, y - 1, color);
                texture2D.Apply();
                GUI.changed = true;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            mesh = (Mesh)EditorGUILayout.ObjectField(mesh, typeof(Mesh), true, GUILayout.Width(250));
            GUILayout.EndHorizontal();

            ShowPanelFooter();
        }

        void ShowPanelHeader(string title)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(title);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(3));
        }

        void ShowPanelFooter()
        {
            ShowPanelFooter(() => { return 0; });
        }

        void ShowPanelFooter(Func<int> randomize)
        {
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Preview settings");
            if (GUILayout.Button("", planet.PreviewSettings ? styles["SelectOn"] : styles["SelectOff"], GUILayout.Height(25f), GUILayout.Width(50f)))
            {
                planet.PreviewSettings = !planet.PreviewSettings;
            }
            GUILayout.EndHorizontal();

            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Randomize settings");
            if (GUILayout.Button(Resources.Load<Texture2D>("UI/RandomIcon"), GUILayout.Height(25f), GUILayout.Width(25f)))
            {
                randomize.Invoke();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void CreateStyles()
        {
            styles = new Dictionary<string, GUIStyle>();

            GUIStyle style = new GUIStyle();
            style.padding = new RectOffset(25, 25, 25, 25);
            style.normal.background = Resources.Load<Texture2D>("UI/BackgroundPanel");
            styles["BackgroundPanel"] = style;

            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/IndicatorLocked");
            styles["IndicatorLocked"] = style;
            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/IndicatorFinished");
            styles["IndicatorFinished"] = style;
            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/IndicatorSelected");
            styles["IndicatorSelected"] = style;

            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/SelectOn");
            styles["SelectOn"] = style;
            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/SelectOff");
            styles["SelectOff"] = style;

            style = new GUIStyle();
            style.normal.background = texture2D;
            styles["texture"] = style;



        }
    }

    public class PopupExample : PopupWindowContent
    {
        bool toggle1 = true;
        bool toggle2 = true;
        bool toggle3 = true;

        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 150);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label("Popup Options Example", EditorStyles.boldLabel);
            toggle1 = EditorGUILayout.Toggle("Toggle 1", toggle1);
            toggle2 = EditorGUILayout.Toggle("Toggle 2", toggle2);
            toggle3 = EditorGUILayout.Toggle("Toggle 3", toggle3);
        }

        public override void OnOpen()
        {
            Debug.Log("Popup opened: " + this);
        }

        public override void OnClose()
        {
            Debug.Log("Popup closed: " + this);
        }
    }
}

