using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanetEngine
{
    internal enum PlanetPresetConfigurations { NONE, EARTH, MARS, MOON }

    /// <summary>
    /// This class represents the UI for creating the planet. It replaces the default UI of the previewPlanet and 
    /// changes the properties of the planetData object. These changes are displayed by the previewPlanet.
    /// </summary>

    [CustomEditor(typeof(PreviewPlanet))]
    internal class PlanetCreatorTool : Editor
    {
        // Used for limited updates
        static float s_lastUpdate = 0;

        // reference to component
        PreviewPlanet _planet;
        // Collection of styles used by the UI
        Dictionary<string, GUIStyle> _styles;
        // Textures that are displayed in the UI
        Texture2D _oceanTexture = null;
        Texture2D _biomeTexture = null;
        // Indicator if values are changed
        bool _changed = false;
        // Properties of currently selected point
        Color _oceanGradientColor;
        int _selectedPoint = -1;

        public override void OnInspectorGUI()
        {
            ShowGenerationMenu();

            if (s_lastUpdate + 0.3f < Time.realtimeSinceStartup && _changed)
            {
                _changed = false;
                if (!AssetDatabase.IsValidFolder("Assets/PlanetEngineData")) AssetDatabase.CreateFolder("Assets", "PlanetEngineData");
                _planet.Data.SaveData(_planet.name);
                _planet.Regenerate();
                SceneView.lastActiveSceneView.Frame(_planet.GetComponent<MeshRenderer>().bounds, false);
                s_lastUpdate = Time.realtimeSinceStartup;
            }
        }

        void Awake()
        {
            _planet = target as PreviewPlanet;
            CreateStyles();
            _planet.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
            _planet.GetComponent<MeshRenderer>().hideFlags = HideFlags.HideInInspector;
            SceneView.lastActiveSceneView.Frame(_planet.GetComponent<MeshRenderer>().bounds, false);
        }

        /// <summary>
        /// Generates a dictionary with named styles which are used by different components in the UI.
        /// </summary>
        void CreateStyles()
        {
            _styles = new Dictionary<string, GUIStyle>();

            GUIStyle style = new GUIStyle();
            style.padding = new RectOffset(25, 25, 25, 25);
            style.normal.background = Resources.Load<Texture2D>("UI/BackgroundPanel");
            _styles["BackgroundPanel"] = style;

            style = new GUIStyle();
            //style.padding = new RectOffset(10,10,10,10);
            style.normal.background = Texture2D.whiteTexture;
            _styles["line"] = style;

            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/IndicatorLocked");
            _styles["IndicatorLocked"] = style;
            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/IndicatorFinished");
            _styles["IndicatorFinished"] = style;
            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/IndicatorSelected");
            _styles["IndicatorSelected"] = style;

            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/SelectOn");
            _styles["SelectOn"] = style;
            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/SelectOff");
            _styles["SelectOff"] = style;

            style = new GUIStyle();
            style.margin = new RectOffset(20, 20, 20, 20);
            _styles["OceanTexture"] = style;
            if (_planet.Data != null && _planet.Data.OceanGradient != null && _oceanTexture == null)
            {
                GenerateOceanGradientTexture();
                _changed = false;
            }

            style = new GUIStyle();
            style.margin = new RectOffset(20, 20, 20, 20);
            _styles["BiomeTexture"] = style;
            if (_planet.Data != null && _planet.Data.biomeGradient != null && _biomeTexture == null)
            {
                GenerateBiomeGradientTexture();
                _changed = false;
            }
        }

        /// <summary>
        /// This method should only be called using a OnGUI method and draws the entire UI.
        /// </summary>
        void ShowGenerationMenu()
        {
            ShowIndicatorMenu();
            ShowPanel();
            ShowNavigationMenu();

        }

        /// <summary>
        /// Shows the top drawer used for pivoting between different panels.
        /// </summary>
        void ShowIndicatorMenu()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(50f));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", IndicatorStyle(PreviewDesignPhase.BASICS), GUILayout.Height(40f), GUILayout.Width(40f))) { _planet.Phase = PreviewDesignPhase.BASICS; _changed = true; }
            if (GUILayout.Button("", IndicatorStyle(PreviewDesignPhase.HEIGHTMAP), GUILayout.Height(40f), GUILayout.Width(40f))) { _planet.Phase = PreviewDesignPhase.HEIGHTMAP; _changed = true; }
            if (GUILayout.Button("", IndicatorStyle(PreviewDesignPhase.CLIMATE), GUILayout.Height(40f), GUILayout.Width(40f))) { _planet.Phase = PreviewDesignPhase.CLIMATE; _changed = true; }
            if (GUILayout.Button("", IndicatorStyle(PreviewDesignPhase.BIOMES), GUILayout.Height(40f), GUILayout.Width(40f))) { _planet.Phase = PreviewDesignPhase.BIOMES; _changed = true; }
            if (GUILayout.Button("", IndicatorStyle(PreviewDesignPhase.VEGETATION), GUILayout.Height(40f), GUILayout.Width(40f))) { _planet.Phase = PreviewDesignPhase.VEGETATION; _changed = true; }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUIStyle IndicatorStyle(PreviewDesignPhase phase)
            {
                return _planet.Phase == phase ? _styles["IndicatorSelected"] : _styles["IndicatorLocked"];
            }
        }

        /// <summary>
        /// Based on the current state of the UI shows different panels.
        /// </summary>
        void ShowPanel()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.MaxWidth(25f));
            GUILayout.BeginVertical(_styles["BackgroundPanel"], GUILayout.Height(250f));
            switch (_planet.Phase)
            {
                case PreviewDesignPhase.BASICS:
                    ShowCelestialMenu();
                    break;
                case PreviewDesignPhase.HEIGHTMAP:
                    ShowHeightMapMenu();
                    break;
                case PreviewDesignPhase.CLIMATE:
                    ShowClimateMenu();
                    break;
                case PreviewDesignPhase.BIOMES:
                    ShowBiomesMenu();
                    break;
                case PreviewDesignPhase.VEGETATION:
                    ShowVegetationMenu();
                    break;
            }
            GUILayout.EndVertical();
            GUILayout.Label("", GUILayout.MaxWidth(25f));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Shows bottom drawer which also enables switching between panels and enable different preview modes.
        /// </summary>
        void ShowNavigationMenu()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(50));
            GUILayout.Label("Randomize planet");
            if (GUILayout.Button(Resources.Load<Texture2D>("UI/RandomIcon"), GUILayout.Height(25f), GUILayout.Width(25f)))
            {
                _changed = true;
                RandomizeProperties(PreviewDesignPhase.NONE);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Resources.Load<Texture2D>("UI/LeftArrow"), GUILayout.Height(25f), GUILayout.Width(50f)))
            {
                if (_planet.Phase == PreviewDesignPhase.BASICS) return;
                _planet.Phase--;
                _changed = true;
            }
            if (GUILayout.Button(Resources.Load<Texture2D>(_planet.Phase == PreviewDesignPhase.VEGETATION ? "UI/Add" : "UI/RightArrow"), GUILayout.Height(25f), GUILayout.Width(50f)))
            {
                if (_planet.Phase != PreviewDesignPhase.VEGETATION) _planet.Phase++;
                _changed = true;
            }
            GUILayout.Label("", GUILayout.Width(50));
            GUILayout.EndHorizontal();
        }


        #region Menu Panels
        /// <summary>
        /// Exposes different celestial properties.
        /// Also contains the presets.
        /// </summary>
        void ShowCelestialMenu()
        {
            ShowPanelHeader("Basics - " + _planet.name);

            float oldRadius = _planet.Data.Radius;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius:", GUILayout.Width(100f));
            _planet.Data.Radius = EditorGUILayout.Slider(_planet.Data.Radius / 1000f, 1f, 100f) * 1000f;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Preset:", GUILayout.Width(100f));
            EditorGUILayout.EnumFlagsField(PlanetPresetConfigurations.NONE);
            GUILayout.EndHorizontal();

            if (oldRadius != _planet.Data.Radius) _changed = true;

            ShowPanelFooter(PreviewDesignPhase.BASICS);
        }

        /// <summary>
        /// Exposes the heightmap generative properties.
        /// </summary>
        void ShowHeightMapMenu()
        {
            ShowPanelHeader("Height map - " + _planet.name);

            int seed = _planet.Data.Seed;
            float scale = _planet.Data.ContinentScale;
            float difference = _planet.Data.heightDifference;
            bool ocean = _planet.Data.HasOcean;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed:", GUILayout.Width(100f));
            _planet.Data.Seed = EditorGUILayout.IntField(_planet.Data.Seed, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Continent size");
            _planet.Data.ContinentScale = EditorGUILayout.Slider(_planet.Data.ContinentScale, 0.1f, 5f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Height difference");
            _planet.Data.heightDifference = EditorGUILayout.Slider(_planet.Data.heightDifference, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Oceans");
            _planet.Data.HasOcean = EditorGUILayout.Toggle(_planet.Data.HasOcean);
            GUILayout.EndHorizontal();
            if (_planet.Data.HasOcean) ShowOceanGradient();

            if (seed != _planet.Data.Seed ||
                scale != _planet.Data.ContinentScale ||
                difference != _planet.Data.heightDifference ||
                ocean != _planet.Data.HasOcean) _changed = true;

            ShowPanelFooter(PreviewDesignPhase.HEIGHTMAP);
        }

        /// <summary>
        /// Exposes climate generative properties.
        /// Also contains multiple preview modes for humidity, heat and normal.
        /// </summary>
        void ShowClimateMenu()
        {
            ShowPanelHeader("Climate - " + _planet.name);

            float solarHeat = _planet.Data.SolarHeat;
            float heightCooling = _planet.Data.HeightCooling;
            float humidity = _planet.Data.HumidityTransfer;
            bool atmosphere = _planet.Data.HasAtmosphere;
            Color atmosphereColor = _planet.Data.AtmosphereColor;
            bool clouds = _planet.Data.HasClouds;
            float cloudsdensity = _planet.Data.CloudDensity;
            int cloudgradient = _planet.Data.CloudGradient.GetHashCode();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Solar heat");
            _planet.Data.SolarHeat = EditorGUILayout.Slider(_planet.Data.SolarHeat, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Height cooling");
            _planet.Data.HeightCooling = EditorGUILayout.Slider(_planet.Data.HeightCooling, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Humidity transfer");
            _planet.Data.HumidityTransfer = EditorGUILayout.Slider(_planet.Data.HumidityTransfer, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Atmosphere");
            _planet.Data.HasAtmosphere = EditorGUILayout.Toggle(_planet.Data.HasAtmosphere);
            GUILayout.EndHorizontal();

            if (_planet.Data.HasAtmosphere)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Atmosphere color");
                _planet.Data.AtmosphereColor = EditorGUILayout.ColorField(_planet.Data.AtmosphereColor);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Clouds");
                _planet.Data.HasClouds = EditorGUILayout.Toggle(_planet.Data.HasClouds);
                GUILayout.EndHorizontal();
                if (_planet.Data.HasClouds)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Clouds density");
                    _planet.Data.CloudDensity = EditorGUILayout.Slider(_planet.Data.CloudDensity, 0f, 1f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Clouds Gradient");
                    _planet.Data.CloudGradient = EditorGUILayout.GradientField(_planet.Data.CloudGradient);
                    GUILayout.EndHorizontal();
                }
            }

            if (solarHeat != _planet.Data.SolarHeat ||
                heightCooling != _planet.Data.HeightCooling ||
                humidity != _planet.Data.HumidityTransfer ||
                atmosphere != _planet.Data.HasAtmosphere ||
                atmosphereColor != _planet.Data.AtmosphereColor ||
                clouds != _planet.Data.HasClouds ||
                cloudsdensity != _planet.Data.CloudDensity ||
                cloudgradient != _planet.Data.CloudGradient.GetHashCode())
            {
                if (solarHeat != _planet.Data.SolarHeat || heightCooling != _planet.Data.HeightCooling) _planet.Data.PreviewHeat = true;
                if (humidity != _planet.Data.HumidityTransfer) _planet.Data.PreviewHeat = false;
                _changed = true;
            }

            ShowPanelFooter(PreviewDesignPhase.CLIMATE);
        }


        /// <summary>
        /// Exposes biome generative properties.
        /// </summary>
        void ShowBiomesMenu()
        {
            ShowPanelHeader("Biomes - " + _planet.name);

            ShowBiomeGradient();

            ShowPanelFooter(PreviewDesignPhase.BIOMES);
        }


        /// <summary>
        /// Exposes vegetation generative properties.
        /// </summary>
        void ShowVegetationMenu()
        {
            ShowPanelHeader("Vegetation - " + _planet.name);
            GUILayout.Label("Tree:");
            //EditorGUILayout.ObjectField(null);
            ShowPanelFooter();
        }
        #endregion


        /// <summary>
        /// Every panel has a title which is shown on top using this method.
        /// </summary>
        void ShowPanelHeader(string title)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(title);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(3));
        }

        /// <summary>
        /// Every panel can be randomized and preview mode can be changed. This is exposed using this method.
        /// </summary>
        /// <param name="phase">The current panel the UI is diplaying</param>
        void ShowPanelFooter(PreviewDesignPhase phase = PreviewDesignPhase.NONE)
        {
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Preview settings");
            if (GUILayout.Button("", _planet.PreviewCurrentPhase ? _styles["SelectOn"] : _styles["SelectOff"], GUILayout.Height(25f), GUILayout.Width(50f)))
            {
                _planet.PreviewCurrentPhase = !_planet.PreviewCurrentPhase;
                _changed = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.Box(Texture2D.whiteTexture, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Randomize settings");
            if (GUILayout.Button(Resources.Load<Texture2D>("UI/RandomIcon"), GUILayout.Height(25f), GUILayout.Width(25f)))
            {
                RandomizeProperties(phase);
                _changed = true;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void GenerateOceanGradientTexture()
        {
            _oceanTexture = _planet.Data.OceanGradient.GetTexture(256, 256);
            _styles["OceanTexture"].normal.background = _oceanTexture;
            _changed = true;
        }

        /// <summary>
        /// A UI element exposing the self implemented 2D gradient.
        /// Connected to the ocean settings.
        /// </summary>
        void ShowOceanGradient()
        {
            if (_oceanTexture == null) GenerateOceanGradientTexture();
            GUILayout.BeginHorizontal();
            Rect textureRect = ShowGradientField();
            Rect pointMenuRect = ShowPointProperties();
            HandleMouseInput(textureRect, pointMenuRect);

            Rect ShowGradientField()
            {
                // Check for change values
                float smooth = _planet.Data.OceanGradient.Smooth;
                float refleciveness = _planet.Data.OceanReflectiveness;

                GUILayout.BeginVertical();
                // Title
                GUILayout.Label("Ocean gradient");
                GUILayout.Space(10);

                // Graph
                GUILayout.Label("Depth");
                GUILayout.Label("", _styles["OceanTexture"], GUILayout.MinWidth(150), GUILayout.MinHeight(100));
                Rect textureRect = GUILayoutUtility.GetLastRect();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Temperature");
                GUILayout.EndHorizontal();

                // Smoothing Menu
                GUILayout.Label("Gradient smoothing");
                _planet.Data.OceanGradient.Smooth = GUILayout.HorizontalSlider(_planet.Data.OceanGradient.Smooth, 0.1f, 10f, GUILayout.Height(20));
                GUILayout.Label("Reflection");
                _planet.Data.OceanReflectiveness = GUILayout.HorizontalSlider(_planet.Data.OceanReflectiveness, 0.5f, 1f, GUILayout.Height(20));

                if (_planet.Data.OceanGradient.Smooth != smooth || _planet.Data.OceanReflectiveness != refleciveness) GenerateOceanGradientTexture();

                // Color points on graph
                for (int i = 0; i < _planet.Data.OceanGradient.Points.Count; i++)
                {
                    GradientPoint point = _planet.Data.OceanGradient.Points[i];
                    int size = i == _selectedPoint ? 30 : 20;
                    GUI.backgroundColor = Color.Lerp(point.Color, Color.white, 0.2f);
                    if (GUI.Button(new Rect(
                        textureRect.x + textureRect.width * point.Position.x - size / 2,
                        textureRect.y + textureRect.height * (1 - point.Position.y) - size / 2,
                        size, size), "", _styles["IndicatorFinished"]))
                    {
                        _selectedPoint = i;
                        _oceanGradientColor = point.Color;
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
                if (0 <= _selectedPoint)
                {
                    // Check for change values
                    GradientPoint point = _planet.Data.OceanGradient.Points[_selectedPoint];
                    GradientPoint prevPoint = new GradientPoint(point.Color, point.Position, point.Weight);

                    // Gradient point properties
                    GUILayout.Label("Color:");
                    _oceanGradientColor = EditorGUILayout.ColorField(_oceanGradientColor);
                    point.Color = _oceanGradientColor;
                    GUILayout.Label("Weight:");
                    point.Weight = GUILayout.HorizontalSlider(point.Weight, 0, 5, GUILayout.Height(20));

                    // Remove selected point
                    if (GUILayout.Button("-", GUILayout.Width(50)))
                    {
                        _planet.Data.OceanGradient.Points.RemoveAt(_selectedPoint);
                        _selectedPoint = -1;
                        GenerateOceanGradientTexture();
                    }
                    else
                    {
                        // If not removed check for changes and apply new properties
                        _planet.Data.OceanGradient.Points[_selectedPoint] = point;
                        if (prevPoint.Color != point.Color ||
                            prevPoint.Position != point.Position ||
                            prevPoint.Weight != point.Weight)
                        {
                            GenerateOceanGradientTexture();
                            _changed = false;
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
                        if (0 <= _selectedPoint)
                        {
                            GradientPoint point = _planet.Data.OceanGradient.Points[_selectedPoint];
                            _oceanGradientColor = point.Color;
                            point.Position = positionNormalized;
                            _planet.Data.OceanGradient.Points[_selectedPoint] = point;
                        }
                        // No point selected means adding new point
                        else
                        {
                            _planet.Data.OceanGradient.Points.Add(new GradientPoint(_oceanGradientColor, positionNormalized, 1f));
                            _selectedPoint = _planet.Data.OceanGradient.Points.Count - 1;
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
                        _selectedPoint = -1;
                        GUI.changed = true;
                    }
                }
            }
        }

        /// <summary>
        /// A UI element exposing the self implemented 2D gradient.
        /// Connected to the biome settings.
        /// TODO: should be more advanced using textures instead of colors.
        /// </summary>
        void ShowBiomeGradient()
        {
            if (_oceanTexture == null) GenerateOceanGradientTexture();
            GUILayout.BeginHorizontal();
            Rect textureRect = ShowGradientField();
            Rect pointMenuRect = ShowPointProperties();
            HandleMouseInput(textureRect, pointMenuRect);

            Rect ShowGradientField()
            {
                // Check for change values
                float smooth = _planet.Data.biomeGradient.Smooth;

                GUILayout.BeginVertical();
                // Title
                GUILayout.Label("Biome gradient");
                GUILayout.Space(10);

                // Graph
                GUILayout.Label("Humidity");
                GUILayout.Label("", _styles["BiomeTexture"], GUILayout.MinWidth(150), GUILayout.MinHeight(100));
                Rect textureRect = GUILayoutUtility.GetLastRect();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Temperature");
                GUILayout.EndHorizontal();

                // Smoothing Menu
                GUILayout.Label("Gradient smoothing");
                _planet.Data.biomeGradient.Smooth = GUILayout.HorizontalSlider(_planet.Data.biomeGradient.Smooth, 0.1f, 10f, GUILayout.Height(20));
                if (_planet.Data.biomeGradient.Smooth != smooth) GenerateBiomeGradientTexture();

                // Color points on graph
                for (int i = 0; i < _planet.Data.biomeGradient.Points.Count; i++)
                {
                    GradientPoint point = _planet.Data.biomeGradient.Points[i];
                    int size = i == _selectedPoint ? 30 : 20;
                    GUI.backgroundColor = Color.Lerp(point.Color, Color.white, 0.2f);
                    if (GUI.Button(new Rect(
                        textureRect.x + textureRect.width * point.Position.x - size / 2,
                        textureRect.y + textureRect.height * (1 - point.Position.y) - size / 2,
                        size, size), "", _styles["IndicatorFinished"]))
                    {
                        _selectedPoint = i;
                        _oceanGradientColor = point.Color;
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
                if (0 <= _selectedPoint)
                {
                    // Check for change values
                    GradientPoint point = _planet.Data.biomeGradient.Points[_selectedPoint];
                    GradientPoint prevPoint = new GradientPoint(point.Color, point.Position, point.Weight);

                    // Gradient point properties
                    GUILayout.Label("Color:");
                    _oceanGradientColor = EditorGUILayout.ColorField(_oceanGradientColor);
                    point.Color = _oceanGradientColor;
                    GUILayout.Label("Weight:");
                    point.Weight = GUILayout.HorizontalSlider(point.Weight, 0, 5, GUILayout.Height(20));

                    // Remove selected point
                    if (GUILayout.Button("-", GUILayout.Width(50)))
                    {
                        _planet.Data.biomeGradient.Points.RemoveAt(_selectedPoint);
                        _selectedPoint = -1;
                        GenerateBiomeGradientTexture();
                    }
                    else
                    {
                        // If not removed check for changes and apply new properties
                        _planet.Data.biomeGradient.Points[_selectedPoint] = point;
                        if (prevPoint.Color != point.Color ||
                            prevPoint.Position != point.Position ||
                            prevPoint.Weight != point.Weight)
                        {
                            GenerateBiomeGradientTexture();
                            _changed = false;
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
                        if (0 <= _selectedPoint)
                        {
                            GradientPoint point = _planet.Data.biomeGradient.Points[_selectedPoint];
                            _oceanGradientColor = point.Color;
                            point.Position = positionNormalized;
                            _planet.Data.biomeGradient.Points[_selectedPoint] = point;
                        }
                        // No point selected means adding new point
                        else
                        {
                            _planet.Data.biomeGradient.Points.Add(new GradientPoint(_oceanGradientColor, positionNormalized, 1f));
                            _selectedPoint = _planet.Data.biomeGradient.Points.Count - 1;
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
                        _selectedPoint = -1;
                        GUI.changed = true;
                    }
                }
            }
        }

        /// <summary>
        /// Displays the gradient for the biome.
        /// </summary>
        void GenerateBiomeGradientTexture()
        {
            _biomeTexture = _planet.Data.biomeGradient.GetTexture(256, 256);
            _styles["BiomeTexture"].normal.background = _biomeTexture;
            _changed = true;
        }

        /// <summary>
        /// Changes values of the planet of given phase.
        /// </summary>
        void RandomizeProperties(PreviewDesignPhase phase)
        {
            UnityEngine.Random.InitState(Time.frameCount);
            if (phase == PreviewDesignPhase.BASICS) _planet.Data.RandomizeCelestialProperties();
            else if (phase == PreviewDesignPhase.HEIGHTMAP) _planet.Data.RandomizeHeightMapProperties();
            else if (phase == PreviewDesignPhase.CLIMATE) _planet.Data.RandomizeClimateProperties();
            else if (phase == PreviewDesignPhase.BIOMES) _planet.Data.RandomizeBiomeProperties();
            else _planet.Data.Randomize();
        }
    }
}

