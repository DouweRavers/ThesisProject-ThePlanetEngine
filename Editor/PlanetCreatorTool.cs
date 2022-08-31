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
        // Collection of styles used by the UI
        public Dictionary<string, GUIStyle> Styles { get; private set; }

        // Used for limited updates
        static float s_lastUpdate = 0;

        // Indicator if values are changed
        bool _changed = false;

        // Gradient UIs
        OceanGradientUI _oceanGradient;
        BiomeGradientUI _biomeGradient;

        // reference to component
        PreviewPlanet _planet;

        public override void OnInspectorGUI()
        {
            ShowGenerationMenu();

            if (s_lastUpdate + 0.2f < Time.realtimeSinceStartup && _changed)
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
            _planet.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
            _planet.GetComponent<MeshRenderer>().hideFlags = HideFlags.HideInInspector;
            SceneView.lastActiveSceneView.Frame(_planet.GetComponent<MeshRenderer>().bounds, false);
            _oceanGradient = CreateInstance<OceanGradientUI>();
            _oceanGradient.Initialize(_planet, this);
            _biomeGradient = CreateInstance<BiomeGradientUI>();
            _biomeGradient.Initialize(_planet, this);
            CreateStyles();
        }

        public void UpdateUI() { _changed = true; }

        /// <summary>
        /// Generates a dictionary with named styles which are used by different components in the UI.
        /// </summary>
        void CreateStyles()
        {
            Styles = new Dictionary<string, GUIStyle>();

            GUIStyle style = new GUIStyle();
            style.padding = new RectOffset(25, 25, 25, 25);
            style.normal.background = Resources.Load<Texture2D>("UI/BackgroundPanel");
            Styles["BackgroundPanel"] = style;

            style = new GUIStyle();
            //style.padding = new RectOffset(10,10,10,10);
            style.normal.background = Texture2D.whiteTexture;
            Styles["line"] = style;

            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/IndicatorLocked");
            Styles["IndicatorLocked"] = style;
            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/IndicatorFinished");
            Styles["IndicatorFinished"] = style;
            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/IndicatorSelected");
            Styles["IndicatorSelected"] = style;

            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/SelectOn");
            Styles["SelectOn"] = style;
            style = new GUIStyle();
            style.normal.background = Resources.Load<Texture2D>("UI/SelectOff");
            Styles["SelectOff"] = style;

            style = new GUIStyle();
            style.margin = new RectOffset(20, 20, 20, 20);
            Styles["OceanTexture"] = style;
            if (_planet.Data != null && _planet.Data.OceanGradient.Points != null)
            {
                _oceanGradient.GenerateTexture();
                _changed = false;
            }

            style = new GUIStyle();
            style.margin = new RectOffset(20, 20, 20, 20);
            Styles["BiomeTexture"] = style;
            if (_planet.Data != null && _planet.Data.BiomeGradient.Points != null)
            {
                _biomeGradient.GenerateTexture();
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
                return _planet.Phase == phase ? Styles["IndicatorSelected"] : Styles["IndicatorLocked"];
            }
        }

        /// <summary>
        /// Based on the current state of the UI shows different panels.
        /// </summary>
        void ShowPanel()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.MaxWidth(25f));
            GUILayout.BeginVertical(Styles["BackgroundPanel"], GUILayout.Height(250f));
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
            if (_planet.Phase != PreviewDesignPhase.BASICS && GUILayout.Button(Resources.Load<Texture2D>("UI/LeftArrow"), GUILayout.Height(25f), GUILayout.Width(50f)))
            {
                if (_planet.Phase == PreviewDesignPhase.BASICS) return;
                _planet.Phase--;
                _changed = true;
            }
            if (_planet.Phase != PreviewDesignPhase.VEGETATION && GUILayout.Button(Resources.Load<Texture2D>("UI/RightArrow"), GUILayout.Height(25f), GUILayout.Width(50f)))
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
            if (_planet.Data.HasOcean) _oceanGradient.OnGUI();

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

            _biomeGradient.OnGUI();

            ShowPanelFooter(PreviewDesignPhase.BIOMES);
        }


        /// <summary>
        /// Exposes vegetation generative properties.
        /// </summary>
        void ShowVegetationMenu()
        {
            ShowPanelHeader("Vegetation - " + _planet.name);
            GUILayout.BeginHorizontal();

            // Foliage input menu
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Foliage:");
                for (int i = 0; i < _planet.Data.FoliageTypes.Length; i++)
                {
                    VegetationReference foliage = _planet.Data.FoliageTypes[i];
                    foliage.SetFoliage(EditorGUILayout.ObjectField(foliage.GetFoliage(), typeof(Texture2D), false) as Texture2D);
                    if (foliage.GetFoliage() == null) continue;
                    GUILayout.Label("Heat:");
                    EditorGUILayout.MinMaxSlider(ref foliage.MinHeat, ref foliage.MaxHeat, 0f, 1f);
                    GUILayout.Label("Humidity:");
                    EditorGUILayout.MinMaxSlider(ref foliage.MinHumidity, ref foliage.MaxHumidity, 0f, 1f);
                    if (!foliage.Equals(_planet.Data.FoliageTypes[i])) UpdateUI();
                    _planet.Data.FoliageTypes[i] = foliage;
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+"))
                {
                    List<VegetationReference> list = new List<VegetationReference>(_planet.Data.FoliageTypes);
                    list.Add(new VegetationReference());
                    _planet.Data.FoliageTypes = list.ToArray();
                    UpdateUI();
                }
                if (0 < _planet.Data.FoliageTypes.Length && GUILayout.Button("-"))
                {
                    List<VegetationReference> list = new List<VegetationReference>(_planet.Data.FoliageTypes);
                    list.RemoveAt(list.Count - 1);
                    _planet.Data.FoliageTypes = list.ToArray();
                    UpdateUI();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            // Tree input menu
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Tree:");
                for (int i = 0; i < _planet.Data.TreeTypes.Length; i++)
                {
                    VegetationReference tree = _planet.Data.TreeTypes[i];

                    _planet.Data.TreeTypes[i].SetTree(EditorGUILayout.ObjectField(_planet.Data.TreeTypes[i].GetTree(), typeof(GameObject), false) as GameObject);
                    if (_planet.Data.TreeTypes[i].GetTree() == null) continue;
                    GUILayout.Label("Heat:");
                    EditorGUILayout.MinMaxSlider(ref _planet.Data.TreeTypes[i].MinHeat, ref _planet.Data.TreeTypes[i].MaxHeat, 0f, 1f);
                    GUILayout.Label("Humidity:");
                    EditorGUILayout.MinMaxSlider(ref _planet.Data.TreeTypes[i].MinHumidity, ref _planet.Data.TreeTypes[i].MaxHumidity, 0f, 1f);
                    if (!_planet.Data.TreeTypes[i].Equals(tree)) UpdateUI();
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+"))
                {
                    List<VegetationReference> list = new List<VegetationReference>(_planet.Data.TreeTypes);
                    list.Add(new VegetationReference());
                    _planet.Data.TreeTypes = list.ToArray();
                    UpdateUI();
                }
                if (0 < _planet.Data.TreeTypes.Length && GUILayout.Button("-"))
                {
                    List<VegetationReference> list = new List<VegetationReference>(_planet.Data.TreeTypes);
                    list.RemoveAt(list.Count - 1);
                    _planet.Data.TreeTypes = list.ToArray();
                    UpdateUI();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
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
            if (GUILayout.Button("", _planet.PreviewCurrentPhase ? Styles["SelectOn"] : Styles["SelectOff"], GUILayout.Height(25f), GUILayout.Width(50f)))
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

