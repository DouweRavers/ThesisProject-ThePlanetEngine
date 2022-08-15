using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    internal class ProceduralTerrainData : ScriptableObject
    {
        /// <summary>
        /// Creates a 2D float array with heightvalues for unity terrain using planet data.
        /// </summary>
        /// <param name="baseTexture">A texture containing the vertex values</param>
        /// <param name="planetData">The planet data used for generating heightmaps</param>
        /// <returns></returns>
        public static float[,] GenerateHeightValues(Texture2D baseTexture, PlanetData planetData)
        {
            Texture2D heightTexture = ProceduralTexture.GetHeightTexture(baseTexture, planetData);
            float[] buffer = heightTexture.GetPixelData<float>(0).ToArray();
            float[,] heightArray = new float[baseTexture.width, baseTexture.height];
            for (int i = 0; i < buffer.Length; i++)
            {
                int x = i % baseTexture.width;
                int y = i / baseTexture.width;
                heightArray[x, y] = 0.5f + buffer[i] / 2;
            }
            return heightArray;
        }

        /// <summary>
        /// Generates an array of textures used for coloring the surface.
        /// </summary>
        /// <param name="planetData">The planet data contianing the textures</param>
        /// <returns>A array of terrainlayers</returns>
        public static TerrainLayer[] GenerateTerrainLayers(PlanetData planetData)
        {
            List<TerrainLayer> layers = new List<TerrainLayer>();
            foreach (GradientPoint point in planetData.BiomeGradient.Points)
            {
                TerrainLayer layer = new TerrainLayer();
                layer.diffuseTexture = point.GetTexture();
                layers.Add(layer);
            }

            return layers.ToArray();
        }

        /// <summary>
        /// Using the textureLayers an 3D array of float values is generated.
        /// Every x,y poisition has a float value for every terrainlayer in the z dimention.
        /// float values are betweeen 0f and 1f.
        /// </summary>
        /// <param name="baseTexture">A texture with the vertex values in texture coordinates</param>
        /// <param name="planetData">The planet data</param>
        /// <returns>A 3D float array containing a z value for every layer for every x,y value.</returns>
        public static float[,,] GenerateAlphaValues(Texture2D baseTexture, PlanetData planetData)
        {
            Texture2D heightTexture = ProceduralTexture.GetHeightTexture(baseTexture, planetData);
            Texture2D heatTexture = ProceduralTexture.GetHeatTexture(baseTexture, heightTexture, planetData);
            Texture2D humidityTexture = ProceduralTexture.GetHumidityTexture(baseTexture, planetData);
            float[,,] alphaValueArray = new float[baseTexture.width, baseTexture.height, planetData.BiomeGradient.Points.Length];
            for (int x = 0; x < baseTexture.width; x++)
            {
                for (int y = 0; y < baseTexture.height; y++)
                {
                    float totalWeight = 0;
                    for (int i = 0; i < planetData.BiomeGradient.Points.Length; i++)
                    {
                        alphaValueArray[x, y, i] = planetData.BiomeGradient.GetPointValueAt(heatTexture.GetPixel(x, y).r, humidityTexture.GetPixel(x, y).r, i);
                        totalWeight += alphaValueArray[x, y, i];
                    }

                    for (int i = 0; i < planetData.BiomeGradient.Points.Length; i++)
                    {
                        alphaValueArray[x, y, i] /= totalWeight;
                    }
                }
            }
            return alphaValueArray;
        }

        /// <summary>
        /// This takes the tree objects and creates prototypes for the terrain.
        /// </summary>
        /// <returns>An array of tree prototypes</returns>
        public static TreePrototype[] GenerateTreePrototypes(PlanetData planetData)
        {
            if (planetData.TreeTypes.Length == 0) return null;
            TreePrototype[] treePrototypes = new TreePrototype[planetData.TreeTypes.Length];
            for (int i = 0; i < treePrototypes.Length; i++)
            {
                TreePrototype treePrototype = new TreePrototype();
                treePrototype.prefab = planetData.TreeTypes[i].GetTree();
                treePrototypes[i] = treePrototype;
            }
            return treePrototypes;
        }

        /// <summary>
        /// This takes the detail (foliage) objects and creates prototypes for the terrain.
        /// </summary>
        /// <returns>An array of detail (foliage) prototypes</returns>
        public static DetailPrototype[] GenerateDetailPrototypes(PlanetData planetData)
        {
            if (planetData.FoliageTypes.Length == 0) return null;
            DetailPrototype[] detailPrototypes = new DetailPrototype[planetData.FoliageTypes.Length];
            for (int i = 0; i < planetData.FoliageTypes.Length; i++)
            {
                DetailPrototype detailPrototype = new DetailPrototype();
                detailPrototype.prototypeTexture = planetData.FoliageTypes[i].GetFoliage();
                detailPrototypes[i] = detailPrototype;
            }
            return detailPrototypes;
        }

        /// <summary>
        /// Generates an array of tree instances based on the prototype that are placed onto the terrain 
        /// with a randomizer.
        /// </summary>
        /// <param name="resolution">The amount of trees placed per square meter</param>
        /// <returns>An array of tree instances</returns>
        public static TreeInstance[] GenerateTreeInstances(int resolution, PlanetData planetData)
        {
            if (planetData.TreeTypes.Length == 0) return null;
            List<TreeInstance> treeInstanceList = new List<TreeInstance>();
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    float x_norm = ((float)x) / resolution;
                    float y_norm = ((float)y) / resolution;

                    TreeInstance instance = new TreeInstance();
                    instance.color = Color.white;
                    instance.lightmapColor = Color.white;
                    instance.heightScale = 1;
                    instance.widthScale = 1;
                    instance.position = new Vector3(x_norm, 0, y_norm);
                    instance.prototypeIndex = Random.Range(0, planetData.TreeTypes.Length);
                    treeInstanceList.Add(instance);
                }
            }
            return treeInstanceList.ToArray();
        }

        /// <summary>
        /// Generates an array of detail (foliage) instances based on the prototype that are 
        /// placed onto the terrain with a randomizer.
        /// </summary>
        /// <param name="detailResolution">The amount of foliage per square meter</param>
        /// <returns>An 2D array of detail instances</returns>
        public static int[,] GenerateDetailInstances(int detailResolution, PlanetData planetData)
        {
            if (planetData.FoliageTypes.Length == 0) return null;
            int[,] detailLevel = new int[detailResolution, detailResolution];
            for (int x = 0; x < detailResolution; x++)
            {
                for (int y = 0; y < detailResolution; y++)
                {
                    detailLevel[x, y] = 1;
                }
            }
            return detailLevel;
        }
    }
}
