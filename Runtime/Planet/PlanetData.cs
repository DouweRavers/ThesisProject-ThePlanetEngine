using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanetEngine
{
    public enum PlanetPresetConfigurations { NONE, EARTH, MARS, MOON }

    [Serializable]
    public struct VegetationReference
    {
        public float MinHeat;
        public float MaxHeat;
        public float MinHumidity;
        public float MaxHumidity;

        [SerializeField]
        string _path;

        public Texture2D GetFoliage() { return Resources.Load<Texture2D>(_path); }
        public GameObject GetTree() { return Resources.Load<GameObject>(_path); }

        public bool Equals(VegetationReference dataEntry)
        {
            return _path == dataEntry._path
                && MinHeat == dataEntry.MinHeat
                && MaxHeat == dataEntry.MaxHeat
                && MinHumidity == dataEntry.MinHumidity
                && MaxHumidity == dataEntry.MaxHumidity;
        }

#if UNITY_EDITOR
        public void SetFoliage(Texture2D texture)
        {
            SaveAsset(texture, ".png");
        }

        public void SetTree(GameObject tree)
        {
            SaveAsset(tree, ".prefab");
        }


        private void SaveAsset(UnityEngine.Object asset, string fileType)
        {
            // Get asset path
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (assetPath == null || assetPath.Length == 0) return;
            // Check if part of resource folder and convert assetpath to resource path
            if (assetPath.StartsWith(ProceduralData.AssetPath))
                _path = assetPath.Replace(ProceduralData.AssetPath, "").ToLower().Replace(fileType, "");
            else
            {
                string assetName = assetPath.Substring(assetPath.LastIndexOf('/') + 1).ToLower();
                if (!AssetDatabase.CopyAsset(assetPath, ProceduralData.AssetPath + assetName))
                    Debug.LogError("Copy failed: " + assetName);
                _path = assetName.Replace(fileType, "");
            }
        }
#endif
    }

    /// <summary>
    /// This class holds all properties for a procedural planet to be generated.
    /// </summary>
    [Serializable]
    public class ProceduralData : ScriptableObject
    {
        public const string PackageAssetPath = "Assets/ThesisProject-ThePlanetEngine/Editor/Resources/RuntimeAssets/"; // "Packages/com.douwco.theplanetengine/Editor/Resources/RuntimeAssets/"
        public const string AssetPath = "Assets/PlanetEngineData/Resources/";

        #region Preview properties
        /// <summary>
        /// An indicator for the previewplanet to determine to show heat or humidity map.
        /// </summary>
        public bool PreviewHeat = true;

        /// <summary>
        /// An configuration wich resembles known planets.
        /// </summary>
        public PlanetPresetConfigurations preset = PlanetPresetConfigurations.NONE;
        #endregion

        #region Procedural Properties
        // Celestial
        /// <summary> Seed for noise generation. </summary>
        public int Seed = 0;
        /// <summary> The radius of the planet. </summary>
        public float Radius = 10000f;

        // Heightmap
        /// <summary> The size of the highest level noise. </summary>
        public float ContinentScale = 0.5f;
        /// <summary> The amplification of the noise. </summary>
        public float heightDifference = 0.2f;

        // Ocean
        /// <summary> Does the planet contain oceans? </summary>
        public bool HasOcean = false;
        /// <summary> How reflective is the ocean material. </summary>
        public float OceanReflectiveness = 0.7f;
        /// <summary> A color gradient for the water x=Heat, y=Depth. </summary>
        public Gradient2D OceanGradient;
        // Climate
        /// <summary> The intesity of heat reaching the planet. </summary>
        public float SolarHeat = 0.5f;
        /// <summary> The decrease in heat as terrain is elevated. </summary>
        public float HeightCooling = 0.5f;
        /// <summary> The rate at which the humidity from the oceans seams landinward. </summary>
        public float HumidityTransfer = 0.5f;
        // Biomes
        /// <summary>color of terrain. x=heat, y=humidity </summary>
        public Gradient2D BiomeGradient;
        // Vegetation
        public VegetationReference[] FoliageTypes;
        public VegetationReference[] TreeTypes;
        #endregion

        #region Rendering Properties
        /// <summary> The amount of subdivisions of the quad tree before max depht. </summary>
        public int MaxDepth = 5;
        /// <summary> The amount of LOD versions of the planet. </summary>
        public int LODSphereCount = 3;
        #endregion

        public void SetupDefaults()
        {
            OceanGradient = new Gradient2D(Color.blue);
            BiomeGradient = new Gradient2D(Resources.Load<Texture2D>("Presets/green"));
            FoliageTypes = new VegetationReference[0];
            TreeTypes = new VegetationReference[0];
        }

        #region IO
        /// <summary>
        /// Saves the class properties to a file named by the parameter and following 
        /// convention: Assets/PlanetEngineData/"name"-planetData.json
        /// </summary>
        /// <param name="name">The name of the planet</param>
        public void SaveData(string name)
        {
            TextAsset file = new TextAsset(JsonUtility.ToJson(this, true));
            AssetDatabase.CreateAsset(file, $"Assets/PlanetEngineData/Resources/{name}-planetData.asset");
        }

        /// <summary>
        /// Loads properties from file into the class. The file follows following naming
        /// convention: Assets/PlanetEngineData/"name"-planetData.json
        /// </summary>
        /// <param name="name">The name of the planet</param>
        /// <exception>When file can't be found throws exception</exception>
        public void LoadData(string name)
        {
            TextAsset file = Resources.Load<TextAsset>($"{name}-planetData.asset");
            if (file == null) return;
            JsonUtility.FromJsonOverwrite(file.text, this);
        }

        public void LoadData(PlanetPresetConfigurations preset)
        {
            TextAsset file;
            switch (preset)
            {
                default:
                case PlanetPresetConfigurations.NONE:
                    return;
                case PlanetPresetConfigurations.EARTH:
                    file = Resources.Load<TextAsset>($"Presets/earth");
                    break;
                case PlanetPresetConfigurations.MARS:
                    file = Resources.Load<TextAsset>($"Presets/mars");
                    break;
                case PlanetPresetConfigurations.MOON:
                    file = Resources.Load<TextAsset>($"Presets/moon");
                    break;
            }
            JsonUtility.FromJsonOverwrite(file.text, this);
        }
        #endregion

        #region Randomize
        /// <summary>
        /// This method randomizes all properties of the planetdata.
        /// </summary>
        public void Randomize()
        {
            RandomizeCelestialProperties();
            RandomizeHeightMapProperties();
            RandomizeClimateProperties();
            RandomizeBiomeProperties();
        }

        /// <summary>
        /// This method randomizes celestial properties of the planetdata.
        /// </summary>
        public void RandomizeCelestialProperties()
        {
            Radius = UnityEngine.Random.Range(1000f, 100000f);
            preset = PlanetPresetConfigurations.NONE;
        }

        /// <summary>
        /// This method randomizes heightmap properties of the planetdata.
        /// </summary>
        public void RandomizeHeightMapProperties()
        {
            Seed = UnityEngine.Random.Range(1, 1000);
            ContinentScale = UnityEngine.Random.Range(0.1f, 5f);
            heightDifference = UnityEngine.Random.Range(0f, 1f);
            HasOcean = UnityEngine.Random.Range(0, 2) == 0;
            OceanGradient = new Gradient2D();
            List<GradientPoint> points = new List<GradientPoint>();
            int count = UnityEngine.Random.Range(3, 10);
            for (int i = 0; i < count; i++)
            {
                Color randColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.8f, 1, 0.8f, 1f, 1f, 1f);
                Vector2 randVec = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                float randWeight = UnityEngine.Random.Range(0, 5f);
                points.Add(new GradientPoint(randColor, randVec, randWeight));
            }
            OceanGradient.Points = points.ToArray();
            OceanGradient.Smooth = UnityEngine.Random.Range(0.1f, 10f);
            OceanReflectiveness = UnityEngine.Random.Range(0.5f, 1f);
        }

        /// <summary>
        /// This method randomizes climate properties of the planetdata.
        /// </summary>
        public void RandomizeClimateProperties()
        {
            SolarHeat = UnityEngine.Random.Range(0f, 1f);
            HeightCooling = UnityEngine.Random.Range(0f, 1f);
            HumidityTransfer = UnityEngine.Random.Range(0f, 1f);
        }

        /// <summary>
        /// This method randomizes biome properties of the planetdata.
        /// </summary>
        public void RandomizeBiomeProperties()
        {
            BiomeGradient = new Gradient2D(Color.clear);
            List<GradientPoint> points = new List<GradientPoint>();
            int count = UnityEngine.Random.Range(3, 10);
            for (int i = 0; i < count; i++)
            {
                Color randColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1, 0.5f, 1f, 1f, 1f);
                Texture2D colorTex = new Texture2D(1, 1);
                colorTex.SetPixel(0, 0, randColor);
                colorTex.Apply();
                Vector2 randVec = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                float randWeight = UnityEngine.Random.Range(0, 5f);
                points.Add(new GradientPoint(colorTex, randVec, randWeight));
            }
            BiomeGradient.Points = points.ToArray();
            BiomeGradient.Smooth = UnityEngine.Random.Range(0.1f, 10f);
        }
        #endregion
    }
}