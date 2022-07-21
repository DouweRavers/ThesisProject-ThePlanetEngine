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
        // TODO: this should take the actual textures given by the biome gradient instead of colors.
        public static TerrainLayer[] GenerateTerrainLayers(PlanetData planetData)
        {
            Texture2D red = new Texture2D(1, 1);
            Texture2D green = new Texture2D(1, 1);
            Texture2D blue = new Texture2D(1, 1);
            red.SetPixel(0, 0, Color.red);
            green.SetPixel(0, 0, Color.green);
            blue.SetPixel(0, 0, Color.blue);
            red.Apply();
            green.Apply();
            blue.Apply();

            TerrainLayer[] layers = new TerrainLayer[3];
            layers[0] = new TerrainLayer();
            layers[0].diffuseTexture = red;
            layers[1] = new TerrainLayer();
            layers[1].diffuseTexture = green;
            layers[2] = new TerrainLayer();
            layers[2].diffuseTexture = blue;

            return layers;
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
            Texture2D colorTexture = ProceduralTexture.GetBiomeTextureColored(heightTexture, heatTexture, humidityTexture, planetData);
            Color[] buffer = colorTexture.GetPixels(0);
            float[,,] alphaValueArray = new float[baseTexture.width, baseTexture.height, 3];
            for (int i = 0; i < buffer.Length; i++)
            {
                int x = i % baseTexture.width;
                int y = i / baseTexture.width;
                alphaValueArray[x, y, 0] = buffer[i].r;
                alphaValueArray[x, y, 1] = buffer[i].g;
                alphaValueArray[x, y, 2] = buffer[i].b;
            }
            return alphaValueArray;
        }

        /// <summary>
        /// This takes the tree objects and creates prototypes for the terrain.
        /// </summary>
        /// <returns>An array of tree prototypes</returns>
        public static TreePrototype[] GenerateTreePrototypes()
        {
            List<TreePrototype> treePrototypes = new List<TreePrototype>();
            TreePrototype treePrototype = new TreePrototype();
            treePrototype.prefab = Resources.Load<GameObject>("tree");
            treePrototypes.Add(treePrototype);
            return treePrototypes.ToArray();
        }

        /// <summary>
        /// This takes the detail (foliage) objects and creates prototypes for the terrain.
        /// </summary>
        /// <returns>An array of detail (foliage) prototypes</returns>
        public static DetailPrototype[] GenerateDetailPrototypes()
        {
            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
            DetailPrototype detailPrototype = new DetailPrototype();
            detailPrototype.prototypeTexture = Resources.Load<Texture2D>("grass");
            detailPrototypes.Add(detailPrototype);
            return detailPrototypes.ToArray();
        }

        /// <summary>
        /// Generates an array of tree instances based on the prototype that are placed onto the terrain 
        /// with a randomizer.
        /// </summary>
        /// <param name="resolution">The amount of trees placed per square meter</param>
        /// <returns>An array of tree instances</returns>
        public static TreeInstance[] GenerateTreeInstances(int resolution)
        {
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
                    instance.prototypeIndex = 0;
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
        public static int[,] GenerateDetailInstances(int detailResolution)
        {
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
