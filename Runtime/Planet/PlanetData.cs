using System;
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
        public bool previewHeat = true;
        #endregion

        #region Procedural Properties
        // Celestial
        /// <summary> Seed for noise generation. </summary>
        public int Seed = 0;
        /// <summary> The radius of the planet. </summary>
        public float Radius = 10f;

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
        [NonSerialized]
        public Gradient2D OceanGradient;
        // Climate
        /// <summary> The intesity of heat reaching the planet. </summary>
        public float SolarHeat = 0.5f;
        /// <summary> The decrease in heat as terrain is elevated. </summary>
        public float HeightCooling = 0.5f;
        /// <summary> The rate at which the humidity from the oceans seams landinward. </summary>
        public float HumidityTransfer = 0.5f;
        // Atmosphere
        /// <summary> Does this planet has a atmosphere? </summary>
        public bool HasAtmosphere = false;
        /// <summary> Color of atmosphere. </summary>
        public Color AtmosphereColor = new Color(1, 1, 1, 0.1f);
        // Clouds
        /// <summary> Does the atmosphere has clouds? </summary>
        public bool HasClouds = true;
        /// <summary> The amount of clouds 0f=nothing, 1f=entire atmosphere. </summary>
        public float CloudDensity = 0.5f;
        /// <summary> The color of the clouds based on density (alpha included). </summary>
        public Gradient CloudGradient;
        // Biomes
        /// <summary> TEMP color of terrain. x=heat, y=humidity </summary>
        [NonSerialized]
        public Gradient2D biomeGradient;
        #endregion

        #region Rendering Properties
        /// <summary> The amount of subdivisions of the quad tree before max depht. </summary>
        public int MaxDepth = 12;
        /// <summary> The amount of LOD versions of the planet. </summary>
        public int LODSphereCount = 3;
        #endregion

        void Awake()
        {
            OceanGradient = CreateInstance<Gradient2D>();
            biomeGradient = CreateInstance<Gradient2D>();
            CloudGradient = new Gradient();
        }

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
    }
}