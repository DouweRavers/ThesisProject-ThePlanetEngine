using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    internal class ProceduralTerrainData : ScriptableObject
    {
        internal static float[,] GenerateHeightValues(Texture2D baseTexture, PlanetData planetData)
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

        internal static TerrainLayer[] GenerateTerrainLayers(PlanetData planetData)
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

        internal static float[,,] GenerateAlphaValues(Texture2D baseTexture, PlanetData planetData)
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

        internal static TreePrototype[] GenerateTreePrototypes()
        {
            List<TreePrototype> treePrototypes = new List<TreePrototype>();
            TreePrototype treePrototype = new TreePrototype();
            treePrototype.prefab = Resources.Load<GameObject>("tree");
            treePrototypes.Add(treePrototype);
            return treePrototypes.ToArray();
        }

        internal static DetailPrototype[] GenerateDetailPrototypes()
        {
            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
            DetailPrototype detailPrototype = new DetailPrototype();
            detailPrototype.prototypeTexture = Resources.Load<Texture2D>("grass");
            detailPrototypes.Add(detailPrototype);
            return detailPrototypes.ToArray();
        }

        internal static TreeInstance[] GenerateTreeInstances(int resolution)
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

        internal static int[,] GenerateDetailInstances(int detailResolution)
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
