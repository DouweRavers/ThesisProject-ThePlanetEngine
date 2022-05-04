using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanetEngine
{
    internal enum Preset { NONE, EARTH, MARS, MOON }
    /// <summary>
    /// This class represents the UI for creating the planet. It in essence changes the 
    /// properties of the planetData object which is displayed by a previewPlanet component.
    /// Once the creation is done the configured planetData is used by the actual planet system
    /// with LODs, quadtree and terrain.
    /// </summary>

    [CustomEditor(typeof(PreviewPlanet))]
    internal class PlanetCreatorTool : Editor
    {
        // Used for limited updates
        static float lastUpdate = 0;

        // reference to component
        PreviewPlanet planet;
        // Collection of styles used by the UI
        Dictionary<string, GUIStyle> styles;
        // Textures that are displayed in the UI
        Texture2D oceanTexture = null;
        Texture2D biomeTexture = null;
        // Indicator if values are changed
        bool changed = false;
        // Properties of currently selected point
        Color oceanGradientColor;
        int selectedPoint = -1;

        // temp
        Mesh mesh;
        Color color;

        public override void OnInspectorGUI()
        {
            ShowGenerationMenu();

            if (lastUpdate + 0.3f < Time.realtimeSinceStartup && changed)
            {
                changed = false;
                if (!AssetDatabase.IsValidFolder("Assets/PlanetEngineData")) AssetDatabase.CreateFolder("Assets", "PlanetEngineData");
                planet.Data.SaveData(planet.name);
                planet.Regenerate();
                SceneView.lastActiveSceneView.Frame(planet.GetComponent<MeshRenderer>().bounds, false);
                lastUpdate = Time.realtimeSinceStartup;
            }
        }

        void Awake()
        {
            planet = target as PreviewPlanet;
            CreateStyles();
            planet.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
            planet.GetComponent<MeshRenderer>().hideFlags = HideFlags.HideInInspector;
            SceneView.lastActiveSceneView.Frame(planet.GetComponent<MeshRenderer>().bounds, false);
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
            if (planet.Data != null && planet.Data.OceanGradient != null && oceanTexture == null)
            {
                GenerateOceanGradientTexture();
                changed = false;
            }

            style = new GUIStyle();
            style.margin = new RectOffset(20, 20, 20, 20);
            styles["BiomeTexture"] = style;
            if (planet.Data != null && planet.Data.biomeGradient != null && biomeTexture == null)
            {
                GenerateBiomeGradientTexture();
                changed = false;
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
                    RandomizeProperties(PreviewPhase.NONE);
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
                    if (planet.Phase != PreviewPhase.VEGETATION) planet.Phase++;
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

            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius:", GUILayout.Width(100f));
            planet.Data.Radius = EditorGUILayout.Slider(planet.Data.Radius/1000f, 1f, 100f) * 1000f;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Preset:", GUILayout.Width(100f));
            EditorGUILayout.EnumFlagsField(Preset.NONE);
            GUILayout.EndHorizontal();

            if (oldRadius != planet.Data.Radius) changed = true;

            ShowPanelFooter(PreviewPhase.BASICS);
        }

        void ShowHeightMapMenu()
        {
            ShowPanelHeader("Height map - " + planet.name);

            int seed = planet.Data.Seed;
            float scale = planet.Data.ContinentScale;
            float difference = planet.Data.heightDifference;
            bool ocean = planet.Data.HasOcean;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed:", GUILayout.Width(100f));
            planet.Data.Seed = EditorGUILayout.IntField(planet.Data.Seed, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Continent size");
            planet.Data.ContinentScale = EditorGUILayout.Slider(planet.Data.ContinentScale, 0.1f, 5f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Height difference");
            planet.Data.heightDifference = EditorGUILayout.Slider(planet.Data.heightDifference, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Oceans");
            planet.Data.HasOcean = EditorGUILayout.Toggle(planet.Data.HasOcean);
            GUILayout.EndHorizontal();
            if (planet.Data.HasOcean) ShowOceanGradient();

            if (seed != planet.Data.Seed ||
                scale != planet.Data.ContinentScale ||
                difference != planet.Data.heightDifference ||
                ocean != planet.Data.HasOcean) changed = true;

            ShowPanelFooter(PreviewPhase.HEIGHTMAP);
        }

        void ShowClimateMenu()
        {
            ShowPanelHeader("Climate - " + planet.name);

            float solarHeat = planet.Data.SolarHeat;
            float heightCooling = planet.Data.HeightCooling;
            float humidity = planet.Data.HumidityTransfer;
            bool atmosphere = planet.Data.HasAtmosphere;
            Color atmosphereColor = planet.Data.AtmosphereColor;
            bool clouds = planet.Data.HasClouds;
            float cloudsdensity = planet.Data.CloudDensity;
            int cloudgradient = planet.Data.CloudGradient.GetHashCode();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Solar heat");
            planet.Data.SolarHeat = EditorGUILayout.Slider(planet.Data.SolarHeat, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Height cooling");
            planet.Data.HeightCooling = EditorGUILayout.Slider(planet.Data.HeightCooling, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Humidity transfer");
            planet.Data.HumidityTransfer = EditorGUILayout.Slider(planet.Data.HumidityTransfer, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Atmosphere");
            planet.Data.HasAtmosphere = EditorGUILayout.Toggle(planet.Data.HasAtmosphere);
            GUILayout.EndHorizontal();

            if (planet.Data.HasAtmosphere)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Atmosphere color");
                planet.Data.AtmosphereColor = EditorGUILayout.ColorField(planet.Data.AtmosphereColor);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Clouds");
                planet.Data.HasClouds = EditorGUILayout.Toggle(planet.Data.HasClouds);
                GUILayout.EndHorizontal();
                if (planet.Data.HasClouds)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Clouds density");
                    planet.Data.CloudDensity = EditorGUILayout.Slider(planet.Data.CloudDensity, 0f, 1f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Clouds Gradient");
                    planet.Data.CloudGradient = EditorGUILayout.GradientField(planet.Data.CloudGradient);
                    GUILayout.EndHorizontal();
                }
            }

            if (solarHeat != planet.Data.SolarHeat ||
                heightCooling != planet.Data.HeightCooling ||
                humidity != planet.Data.HumidityTransfer ||
                atmosphere != planet.Data.HasAtmosphere ||
                atmosphereColor != planet.Data.AtmosphereColor ||
                clouds != planet.Data.HasClouds ||
                cloudsdensity != planet.Data.CloudDensity ||
                cloudgradient != planet.Data.CloudGradient.GetHashCode())
            {
                if (solarHeat != planet.Data.SolarHeat || heightCooling != planet.Data.HeightCooling) planet.Data.previewHeat = true;
                if (humidity != planet.Data.HumidityTransfer) planet.Data.previewHeat = false;
                changed = true;
            }

            ShowPanelFooter(PreviewPhase.CLIMATE);
        }

        void ShowBiomesMenu()
        {
            ShowPanelHeader("Biomes - " + planet.name);

            ShowBiomeGradient();

            ShowPanelFooter(PreviewPhase.BIOMES);
        }

        void ShowVegetationMenu()
        {
            ShowPanelHeader("Vegetation - " + planet.name);
            GUILayout.Label("Tree:");
            //EditorGUILayout.ObjectField(null);
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

        void ShowPanelFooter(PreviewPhase phase = PreviewPhase.NONE)
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
                RandomizeProperties(phase);
                changed = true;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void ShowOceanGradient()
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
                float refleciveness = planet.Data.OceanReflectiveness;

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
                GUILayout.Label("Reflection");
                planet.Data.OceanReflectiveness = GUILayout.HorizontalSlider(planet.Data.OceanReflectiveness, 0.5f, 1f, GUILayout.Height(20));

                if (planet.Data.OceanGradient.Smooth != smooth || planet.Data.OceanReflectiveness != refleciveness) GenerateOceanGradientTexture();

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
                        GenerateOceanGradientTexture();
                    }
                    else
                    {
                        // If not removed check for changes and apply new properties
                        planet.Data.OceanGradient.Points[selectedPoint] = point;
                        if (prevPoint.Color != point.Color ||
                            prevPoint.Position != point.Position ||
                            prevPoint.Weight != point.Weight)
                        {
                            GenerateOceanGradientTexture();
                            changed = false;
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
            changed = true;
        }

        void ShowBiomeGradient()
        {
            if (oceanTexture == null) GenerateOceanGradientTexture();
            GUILayout.BeginHorizontal();
            Rect textureRect = ShowGradientField();
            Rect pointMenuRect = ShowPointProperties();
            HandleMouseInput(textureRect, pointMenuRect);

            Rect ShowGradientField()
            {
                // Check for change values
                float smooth = planet.Data.biomeGradient.Smooth;

                GUILayout.BeginVertical();
                // Title
                GUILayout.Label("Biome gradient");
                GUILayout.Space(10);

                // Graph
                GUILayout.Label("Humidity");
                GUILayout.Label("", styles["BiomeTexture"], GUILayout.MinWidth(150), GUILayout.MinHeight(100));
                Rect textureRect = GUILayoutUtility.GetLastRect();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Temperature");
                GUILayout.EndHorizontal();

                // Smoothing Menu
                GUILayout.Label("Gradient smoothing");
                planet.Data.biomeGradient.Smooth = GUILayout.HorizontalSlider(planet.Data.biomeGradient.Smooth, 0.1f, 10f, GUILayout.Height(20));
                if (planet.Data.biomeGradient.Smooth != smooth ) GenerateBiomeGradientTexture();

                // Color points on graph
                for (int i = 0; i < planet.Data.biomeGradient.Points.Count; i++)
                {
                    GradientPoint point = planet.Data.biomeGradient.Points[i];
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
                    GradientPoint point = planet.Data.biomeGradient.Points[selectedPoint];
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
                        planet.Data.biomeGradient.Points.RemoveAt(selectedPoint);
                        selectedPoint = -1;
                        GenerateBiomeGradientTexture();
                    }
                    else
                    {
                        // If not removed check for changes and apply new properties
                        planet.Data.biomeGradient.Points[selectedPoint] = point;
                        if (prevPoint.Color != point.Color ||
                            prevPoint.Position != point.Position ||
                            prevPoint.Weight != point.Weight)
                        {
                            GenerateBiomeGradientTexture();
                            changed = false;
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
                            GradientPoint point = planet.Data.biomeGradient.Points[selectedPoint];
                            oceanGradientColor = point.Color;
                            point.Position = positionNormalized;
                            planet.Data.biomeGradient.Points[selectedPoint] = point;
                        }
                        // No point selected means adding new point
                        else
                        {
                            planet.Data.biomeGradient.Points.Add(new GradientPoint(oceanGradientColor, positionNormalized, 1f));
                            selectedPoint = planet.Data.biomeGradient.Points.Count - 1;
                        }
                        GenerateBiomeGradientTexture();
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

        void GenerateBiomeGradientTexture()
        {
            biomeTexture = planet.Data.biomeGradient.GetTexture(256, 256);
            styles["BiomeTexture"].normal.background = biomeTexture;
            changed = true;
        }

        void RandomizeProperties(PreviewPhase phase)
        {
            UnityEngine.Random.InitState(Time.frameCount);
            if (phase == PreviewPhase.BASICS) planet.Data.RandomizeCelestialProperties();
            else if (phase == PreviewPhase.HEIGHTMAP) planet.Data.RandomizeHeightMapProperties();
            else if (phase == PreviewPhase.CLIMATE) planet.Data.RandomizeClimateProperties();
            else if (phase == PreviewPhase.BIOMES) planet.Data.RandomizeBiomeProperties();
            else planet.Data.Randomize();
        }
    }
}

