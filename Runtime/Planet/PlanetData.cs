using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// This class holds all properties for a procedural planet to be generated.
    /// </summary>
    [Serializable]
    public class PlanetData : ScriptableObject
    {
        #region Preview properties
        /// <summary>
        /// An indicator for the previewplanet to determine to show heat or humidity map.
        /// </summary>
        public bool PreviewHeat { set; get; } = true;
        #endregion

        #region Procedural Properties
        // Celestial
        /// <summary> Seed for noise generation. </summary>
        public int Seed { set; get; } = 0;
        /// <summary> The radius of the planet. </summary>
        public float Radius { set; get; } = 10000f;

        // Heightmap
        /// <summary> The size of the highest level noise. </summary>
        public float ContinentScale { set; get; } = 0.5f;
        /// <summary> The amplification of the noise. </summary>
        public float heightDifference { set; get; } = 0.2f;

        // Ocean
        /// <summary> Does the planet contain oceans? </summary>
        public bool HasOcean { set; get; } = false;
        /// <summary> How reflective is the ocean material. </summary>
        public float OceanReflectiveness { set; get; } = 0.7f;
        /// <summary> A color gradient for the water x=Heat, y=Depth. </summary>
        [NonSerialized]
        public Gradient2D OceanGradient;
        // Climate
        /// <summary> The intesity of heat reaching the planet. </summary>
        public float SolarHeat { set; get; } = 0.5f;
        /// <summary> The decrease in heat as terrain is elevated. </summary>
        public float HeightCooling { set; get; } = 0.5f;
        /// <summary> The rate at which the humidity from the oceans seams landinward. </summary>
        public float HumidityTransfer { set; get; } = 0.5f;
        // Atmosphere
        /// <summary> Does this planet has a atmosphere? </summary>
        public bool HasAtmosphere { set; get; } = false;
        /// <summary> Color of atmosphere. </summary>
        public Color AtmosphereColor { set; get; } = new Color(1, 1, 1, 0.1f);
        // Clouds
        /// <summary> Does the atmosphere has clouds? </summary>
        public bool HasClouds { set; get; } = true;
        /// <summary> The amount of clouds 0f=nothing, 1f=entire atmosphere. </summary>
        public float CloudDensity { set; get; } = 0.5f;
        /// <summary> The color of the clouds based on density (alpha included). </summary>
        public Gradient CloudGradient;
        // Biomes
        /// <summary> TEMP color of terrain. x=heat, y=humidity </summary>
        [NonSerialized]
        public Gradient2D biomeGradient;
        // Vegetation
        #endregion

        #region Rendering Properties
        /// <summary> The amount of subdivisions of the quad tree before max depht. </summary>
        public int MaxDepth { set; get; } = 5;
        /// <summary> The amount of LOD versions of the planet. </summary>
        public int LODSphereCount { set; get; } = 3;
        #endregion

        private void Awake()
        {
            OceanGradient = CreateInstance<Gradient2D>();
            biomeGradient = CreateInstance<Gradient2D>();
            CloudGradient = new Gradient();
        }
        #region IO

        /// <summary>
        /// Saves the class properties to a file named by the parameter and following 
        /// convention: Assets/PlanetEngineData/"name"-planetData.json
        /// </summary>
        /// <param name="name">The name of the planet</param>
        public void SaveData(string name)
        {
            string dataContent = JsonUtility.ToJson(this, true);
            string oceanGradientContent = JsonUtility.ToJson(OceanGradient, true);
            string biomeGradientContent = JsonUtility.ToJson(biomeGradient, true);
            string content = dataContent;
            content += "\n$\n" + oceanGradientContent;
            content += "\n$\n" + biomeGradientContent;
            File.WriteAllText("Assets/PlanetEngineData/" + name + "-planetData.json", content);
        }

        /// <summary>
        /// Loads properties from file into the class. The file follows following naming
        /// convention: Assets/PlanetEngineData/"name"-planetData.json
        /// </summary>
        /// <param name="name">The name of the planet</param>
        /// <exception>When file can't be found throws exception</exception>
        public void LoadData(string name)
        {
            if (File.Exists("Assets/PlanetEngineData/" + name + "-planetData.json"))
            {
                string content = File.ReadAllText("Assets/PlanetEngineData/" + name + "-planetData.json");
                string[] parsedContent = content.Split('$');
                JsonUtility.FromJsonOverwrite(parsedContent[0], this);
                JsonUtility.FromJsonOverwrite(parsedContent[1], OceanGradient);
                JsonUtility.FromJsonOverwrite(parsedContent[2], biomeGradient);
            }
            else
            {
                throw new FileNotFoundException();
            }
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
            OceanGradient = CreateInstance<Gradient2D>();
            List<GradientPoint> points = new List<GradientPoint>();
            for (int i = 0; i < UnityEngine.Random.Range(3, 10); i++)
            {
                Color randColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.8f, 1, 0.8f, 1f, 1f, 1f);
                Vector2 randVec = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                float randWeight = UnityEngine.Random.Range(0, 5f);
                points.Add(new GradientPoint(randColor, randVec, randWeight));
            }
            OceanGradient.Points = points;
            OceanGradient.Smooth = UnityEngine.Random.Range(0.1f, 5f);
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
            HasAtmosphere = UnityEngine.Random.Range(0, 2) == 0;
            AtmosphereColor = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f, 0f, 0.2f);
            HasClouds = UnityEngine.Random.Range(0, 2) == 0;
            CloudDensity = UnityEngine.Random.Range(0f, 1f);
        }

        /// <summary>
        /// This method randomizes biome properties of the planetdata.
        /// </summary>
        public void RandomizeBiomeProperties()
        {
            biomeGradient = CreateInstance<Gradient2D>();
            List<GradientPoint> points = new List<GradientPoint>();
            for (int i = 0; i < UnityEngine.Random.Range(3, 10); i++)
            {
                Color randColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1, 0.5f, 1f, 1f, 1f);
                Vector2 randVec = new Vector2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                float randWeight = UnityEngine.Random.Range(0, 5f);
                points.Add(new GradientPoint(randColor, randVec, randWeight));
            }
            biomeGradient.Points = points;
            biomeGradient.Smooth = UnityEngine.Random.Range(0.1f, 5f);
        }
        #endregion
    }
}