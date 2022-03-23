using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanetEngine
{

    [CustomEditor(typeof(PreviewPlanet))]
    internal class PlanetCreatorTool : Editor
    {
        #region Private attributes
        PreviewPlanet planet;
        Dictionary<string, GUIStyle> styles;
        Texture2D oceanTexture = null;
        Color oceanGradientColor = Color.blue;
        bool changed = false;
        int selectedPoint = -1;
        // temp
        Mesh mesh;
        Color color;
        static float lastUpdate = 0;
        #endregion


        public override void OnInspectorGUI()
        {
            ShowGenerationMenu();
            if (lastUpdate + 0.3f < Time.realtimeSinceStartup && changed)
            {
                changed = false;
                planet.Regenerate();
                lastUpdate = Time.realtimeSinceStartup;
            }
        }

        void Awake()
        {
            planet = target as PreviewPlanet;
            CreateStyles();
            planet.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
            planet.GetComponent<MeshRenderer>().hideFlags = HideFlags.HideInInspector;
        }

        void CreateStyles()
        {
            styles = new Dictionary<string, GUIStyle>();

            GUIStyle style = new GUIStyle();
            style.padding = new RectOffset(25, 25, 25, 25);
            style.normal.background = Resources.Load<Texture2D>("UI/BackgroundPanel");
            styles["BackgroundPanel"] = style;

            style = new GUIStyle();
            //style.padding = new RectOffset(10,10,10,10);
            style.normal.background = Texture2D.whiteTexture;
            styles["line"] = style;

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
            style.margin = new RectOffset(20, 20, 20, 20);
            styles["OceanTexture"] = style;
            if (planet.Data != null && oceanTexture == null)
            {
                oceanTexture = planet.Data.OceanGradient.GetTexture(256, 256);
                styles["OceanTexture"].normal.background = oceanTexture;
            }
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
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.BASICS), GUILayout.Height(40f), GUILayout.Width(40f))) { planet.Phase = PreviewPhase.BASICS; changed = true; }
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.HEIGHTMAP), GUILayout.Height(40f), GUILayout.Width(40f))) { planet.Phase = PreviewPhase.HEIGHTMAP; changed = true; }
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.CLIMATE), GUILayout.Height(40f), GUILayout.Width(40f))) { planet.Phase = PreviewPhase.CLIMATE; changed = true; }
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.BIOMES), GUILayout.Height(40f), GUILayout.Width(40f))) { planet.Phase = PreviewPhase.BIOMES; changed = true; }
                if (GUILayout.Button("", IndicatorStyle(PreviewPhase.VEGETATION), GUILayout.Height(40f), GUILayout.Width(40f))) { planet.Phase = PreviewPhase.VEGETATION; changed = true; }

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
                    changed = true;
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Resources.Load<Texture2D>("UI/LeftArrow"), GUILayout.Height(25f), GUILayout.Width(50f)))
                {
                    if (planet.Phase == PreviewPhase.BASICS) return;
                    planet.Phase--;
                    changed = true;
                }
                if (GUILayout.Button(Resources.Load<Texture2D>(planet.Phase == PreviewPhase.VEGETATION ? "UI/Add" : "UI/RightArrow"), GUILayout.Height(25f), GUILayout.Width(50f)))
                {
                    if (planet.Phase == PreviewPhase.VEGETATION) return;
                    planet.Phase++;
                    changed = true;
                }
                GUILayout.Label("", GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }
        }

        void ShowCelestialMenu()
        {
            ShowPanelHeader("Basics - " + planet.name);

            float oldRadius = planet.Data.Radius;
            int oldLOD = planet.Data.LODSphereCount;
            int oldDepth = planet.Data.MaxDepth;

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

            if (oldRadius != planet.Data.Radius ||
                oldLOD != planet.Data.LODSphereCount ||
                oldDepth != planet.Data.MaxDepth) changed = true;


            ShowPanelFooter(RandomizeCelestialProperties);

            int RandomizeCelestialProperties()
            {
                UnityEngine.Random.InitState(Time.frameCount);
                planet.Data.Radius = UnityEngine.Random.Range(1f, 100f);
                planet.Data.LODSphereCount = UnityEngine.Random.Range(2, 5);
                planet.Data.MaxDepth = UnityEngine.Random.Range(2, 13);
                return 1;
            }
        }

        void ShowHeightMapMenu()
        {
            ShowPanelHeader("Height map - " + planet.name);

            int seed = planet.Data.Seed;
            float scale = planet.Data.ContinentScale;
            bool ocean = planet.Data.HasOcean;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed:", GUILayout.Width(100f));
            planet.Data.Seed = EditorGUILayout.IntField(planet.Data.Seed, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Continent size");
            planet.Data.ContinentScale = EditorGUILayout.Slider(planet.Data.ContinentScale, 0.5f, 10f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Oceans");
            planet.Data.HasOcean = EditorGUILayout.Toggle(planet.Data.HasOcean);
            GUILayout.EndHorizontal();
            if (planet.Data.HasOcean) ShowGradient();

            if (seed != planet.Data.Seed ||
                scale != planet.Data.ContinentScale ||
                ocean != planet.Data.HasOcean) changed = true;

            ShowPanelFooter(RandomizeHeightMapProperties);

            int RandomizeHeightMapProperties()
            {
                UnityEngine.Random.InitState(Time.frameCount);
                planet.Data.Seed = UnityEngine.Random.Range(1, 1000);
                planet.Data.ContinentScale = UnityEngine.Random.Range(0.5f, 10f);
                planet.Data.HasOcean = UnityEngine.Random.Range(0, 2) == 0;
                planet.Data.OceanGradient = ScriptableObject.CreateInstance<Gradient2D>();
                List<GradientPoint> points = new List<GradientPoint>();
                for (int i = 0; i < UnityEngine.Random.Range(3, 10); i++)
                {
                    Color randColor = UnityEngine.Random.ColorHSV(0, 1, 0, 1, 0, 1, 1, 1);
                    Vector2 randVec = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    float randWeight = UnityEngine.Random.Range(0, 5f);
                    points.Add(new GradientPoint(randColor, randVec, randWeight));
                }
                planet.Data.OceanGradient.Points = points;
                return 1;
            }


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
            /*
             * texture2D = (Texture2D)EditorGUILayout.ObjectField("terrain texture", texture2D, typeof(Texture2D), false);
            texture2D = (Texture2D)EditorGUILayout.ObjectField("heightmap texture", texture2D, typeof(Texture2D), false);
            float a = 0.1f, b = 0.5f;
            EditorGUILayout.MinMaxSlider("Height:", ref a, ref b, 0f, 1f);
            
            showGradient = EditorGUILayout.Foldout(showGradient, "graph");
            if (showGradient)
            {
                color = EditorGUILayout.ColorField(color);
                GUILayout.Label("", styles["texture"], GUILayout.Width(texture2D.width * 3), GUILayout.Height(texture2D.height * 3));
                Rect textureRect = GUILayoutUtility.GetLastRect();
                Event e = Event.current;
                if (e.type == EventType.MouseDown && textureRect.x < e.mousePosition.x && e.mousePosition.x < textureRect.x + textureRect.width &&
                    textureRect.y < e.mousePosition.y && e.mousePosition.y < textureRect.y + textureRect.height)
                {
                    int x = (int)(e.mousePosition.x - textureRect.x) / 3;
                    int y = (int)(textureRect.y + textureRect.width - e.mousePosition.y) / 3;
                    texture2D.SetPixel(x, y, color);
                    texture2D.SetPixel(x + 1, y, color);
                    texture2D.SetPixel(x - 1, y, color);
                    texture2D.SetPixel(x, y + 1, color);
                    texture2D.SetPixel(x, y - 1, color);
                    texture2D.Apply();
                    GUI.changed = true;
                }
            }*/

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
            //texture2D = (Texture2D)EditorGUILayout.ObjectField(texture2D, typeof(Texture2D), false, GUILayout.Width(250));
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
            /*GUILayout.Label("", styles["texture"], GUILayout.Width(texture2D.width * 3), GUILayout.Height(texture2D.height * 3));
            Rect textureRect = GUILayoutUtility.GetLastRect();
            Event e = Event.current;
            if (e.type == EventType.MouseDown && textureRect.x < e.mousePosition.x && e.mousePosition.x < textureRect.x + textureRect.width &&
                textureRect.y < e.mousePosition.y && e.mousePosition.y < textureRect.y + textureRect.height)
            {
                int x = (int)(e.mousePosition.x - textureRect.x) / 3;
                int y = (int)(textureRect.y + textureRect.width - e.mousePosition.y) / 3;
                texture2D.SetPixel(x, y, color);
                texture2D.SetPixel(x + 1, y, color);
                texture2D.SetPixel(x - 1, y, color);
                texture2D.SetPixel(x, y + 1, color);
                texture2D.SetPixel(x, y - 1, color);
                texture2D.Apply();
                GUI.changed = true;
            }
            */
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
                changed = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Randomize settings");
            if (GUILayout.Button(Resources.Load<Texture2D>("UI/RandomIcon"), GUILayout.Height(25f), GUILayout.Width(25f)))
            {
                randomize.Invoke();
                changed = true;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void ShowGradient()
        {
            if (oceanTexture == null) GenerateOceanGradientTexture();
            GUILayout.BeginHorizontal();
            Rect textureRect = ShowGradientField();
            Rect pointMenuRect = ShowPointProperties();
            HandleMouseInput(textureRect, pointMenuRect);

            Rect ShowGradientField()
            {
                // Check for change values
                float smooth = planet.Data.OceanGradient.Smooth;

                GUILayout.BeginVertical();
                // Title
                GUILayout.Label("Ocean gradient");
                GUILayout.Space(10);

                // Graph
                GUILayout.Label("Depth");
                GUILayout.Label("", styles["OceanTexture"], GUILayout.MinWidth(150), GUILayout.MinHeight(100));
                Rect textureRect = GUILayoutUtility.GetLastRect();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Temperature");
                GUILayout.EndHorizontal();

                // Smoothing Menu
                GUILayout.Label("Gradient smoothing");
                planet.Data.OceanGradient.Smooth = GUILayout.HorizontalSlider(planet.Data.OceanGradient.Smooth, 0.1f, 10f, GUILayout.Height(20));
                if (planet.Data.OceanGradient.Smooth != smooth) GenerateOceanGradientTexture();

                // Color points on graph
                for (int i = 0; i < planet.Data.OceanGradient.Points.Count; i++)
                {
                    GradientPoint point = planet.Data.OceanGradient.Points[i];
                    int size = i == selectedPoint ? 30 : 20;
                    GUI.backgroundColor = Color.Lerp(point.Color, Color.white, 0.2f);
                    if (GUI.Button(new Rect(
                        textureRect.x + textureRect.width * point.Position.x - size / 2,
                        textureRect.y + textureRect.height * (1 - point.Position.y) - size / 2,
                        size, size), "", styles["IndicatorFinished"]))
                    {
                        selectedPoint = i;
                        oceanGradientColor = point.Color;
                    }
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndVertical();

                return textureRect;
            }

            Rect ShowPointProperties()
            {
                GUILayout.BeginVertical(GUILayout.MaxWidth(100), GUILayout.ExpandHeight(true));

                // Title
                GUILayout.Label("Point menu");
                GUILayout.Space(10);

                // Menu
                if (0 <= selectedPoint)
                {
                    // Check for change values
                    GradientPoint point = planet.Data.OceanGradient.Points[selectedPoint];
                    GradientPoint prevPoint = new GradientPoint(point.Color, point.Position, point.Weight);

                    // Gradient point properties
                    GUILayout.Label("Color:");
                    oceanGradientColor = EditorGUILayout.ColorField(oceanGradientColor);
                    point.Color = oceanGradientColor;
                    GUILayout.Label("Weight:");
                    point.Weight = GUILayout.HorizontalSlider(point.Weight, 0, 5, GUILayout.Height(20));

                    // Remove selected point
                    if (GUILayout.Button("-", GUILayout.Width(50)))
                    {
                        planet.Data.OceanGradient.Points.RemoveAt(selectedPoint);
                        selectedPoint = -1;
                        oceanTexture = planet.Data.OceanGradient.GetTexture(256, 256);
                        styles["OceanTexture"].normal.background = oceanTexture;
                    }
                    else
                    {
                        // If not removed check for changes and apply new properties
                        planet.Data.OceanGradient.Points[selectedPoint] = point;
                        if (prevPoint.Color != point.Color ||
                            prevPoint.Position != point.Position ||
                            prevPoint.Weight != point.Weight)
                        {
                            oceanTexture = planet.Data.OceanGradient.GetTexture(256, 256);
                            styles["OceanTexture"].normal.background = oceanTexture;
                        }
                    }
                }
                else
                {
                    GUILayout.Label("Click to add a point.");
                }
                GUILayout.EndVertical();
                Rect pointMenuRect = GUILayoutUtility.GetLastRect();
                GUILayout.EndHorizontal();

                return pointMenuRect;
            }

            void HandleMouseInput(Rect textureRect, Rect pointMenuRect)
            {
                // Catch events
                Event e = Event.current;
                if (e.type == EventType.MouseDown)
                {
                    // Check if mouse press is over the gradient graph area
                    if (textureRect.x < e.mousePosition.x && e.mousePosition.x < textureRect.x + textureRect.width &&
                    textureRect.y < e.mousePosition.y && e.mousePosition.y < textureRect.y + textureRect.height)
                    {
                        Vector2 positionNormalized = new Vector2((e.mousePosition.x - textureRect.x) / textureRect.width, (textureRect.y + textureRect.height - e.mousePosition.y) / textureRect.height);
                        // If point was previously selected apply new position
                        if (0 <= selectedPoint)
                        {
                            GradientPoint point = planet.Data.OceanGradient.Points[selectedPoint];
                            oceanGradientColor = point.Color;
                            point.Position = positionNormalized;
                            planet.Data.OceanGradient.Points[selectedPoint] = point;
                        }
                        // No point selected means adding new point
                        else
                        {
                            planet.Data.OceanGradient.Points.Add(new GradientPoint(oceanGradientColor, positionNormalized, 1f));
                            selectedPoint = planet.Data.OceanGradient.Points.Count - 1;
                        }
                        GenerateOceanGradientTexture();
                        GUI.changed = true;
                    }
                    // Check if mouse is over point menu
                    else if (pointMenuRect.x < e.mousePosition.x && e.mousePosition.x < pointMenuRect.x + pointMenuRect.width &&
                  pointMenuRect.y < e.mousePosition.y && e.mousePosition.y < pointMenuRect.y + pointMenuRect.height)
                    {
                        // Do not unselect points when adjusting point properties
                    }
                    // When mousepress is not on focus area
                    else
                    {
                        selectedPoint = -1;
                        GUI.changed = true;
                    }
                }
            }
        }

        void GenerateOceanGradientTexture()
        {
            oceanTexture = planet.Data.OceanGradient.GetTexture(256, 256);
            styles["OceanTexture"].normal.background = oceanTexture;
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

        public override void OnGUI(Rect textureRect)
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

