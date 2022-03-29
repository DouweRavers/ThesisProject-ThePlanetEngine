using System;
using System.IO;
using UnityEngine;

namespace PlanetEngine
{
    [Serializable]
    public class PlanetData : ScriptableObject
    {
        #region Preview properties
        public bool previewHeat = true;
        #endregion

        #region Procedural Properties
        // Celestial
        public int Seed = 0;
        public float Radius = 10f;
        // Heightmap
        public float ContinentScale = 0.5f;
        // Ocean
        public bool HasOcean = false;
        public float OceanReflectiveness = 0.7f;
        [NonSerialized]
        public Gradient2D OceanGradient;
        // Climate
        public float SolarHeat = 0.5f;
        public float HeightCooling = 0.5f;
        public float HumidityTransfer = 0.5f;
        // Atmosphere
        public bool HasAtmosphere = false;
        public Color AtmosphereColor = new Color(1, 1, 1, 0.1f);
        // Clouds
        public bool HasClouds = true;
        public float CloudDensity = 0.5f;
        public Gradient CloudGradient;
        // Biomes
        [NonSerialized]
        public Gradient2D biomeGradient;
        #endregion

        #region Rendering Properties
        public int MaxDepth = 12;
        public int LODSphereCount = 3;
        #endregion

        void Awake()
        {
            OceanGradient = CreateInstance<Gradient2D>();
            biomeGradient = CreateInstance<Gradient2D>();
            CloudGradient = new Gradient();
        }

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

        public void LoadData(string name) {
            string content = File.ReadAllText("Assets/PlanetEngineData/" + name + "-planetData.json");
            string[] parsedContent = content.Split('$');
            JsonUtility.FromJsonOverwrite(parsedContent[0], this);
            JsonUtility.FromJsonOverwrite(parsedContent[1], OceanGradient);
            JsonUtility.FromJsonOverwrite(parsedContent[2], biomeGradient);
        }
    }
}