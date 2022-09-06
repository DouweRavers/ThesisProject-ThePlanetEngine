using UnityEngine;

namespace PlanetEngine
{
    internal static class ProceduralMaterial
    {
        static Texture2D s_heightTexture, s_heatmapTexture, s_humidityTexture;

        /// <summary>
        /// Get material for full mesh planet. Depending on the phase some properties will be skipped.
        /// </summary>
        /// <param name="data">Planet data</param>
        /// <param name="baseTexture">A texture containing vertex values</param>
        /// <param name="phase">The phase for which the material should be generated</param>
        /// <param name="textureSize">The size of the textures used in the matertial.</param>
        /// <returns>The procedurally generated material</returns>
        public static Material GetMaterial(ProceduralData data, Texture2D baseTexture = null, PreviewDesignPhase phase = PreviewDesignPhase.NONE, int textureSize = 256)
        {
            Texture2D colorTexture = null, normalTexture = null, specularTexture = null;

            // Basic planet
            if (baseTexture == null)
            {
                baseTexture = ProceduralTexture.GetBaseTexture(textureSize, textureSize * 3 / 4);
            }
            if (phase == PreviewDesignPhase.BASICS)
            {
                colorTexture = baseTexture;//Texture2D.whiteTexture;
                return GenerateMaterial(colorTexture, normalTexture, specularTexture);
            }

            // Generate heights
            Texture2D heightTexture = ProceduralTexture.GetHeightTexture(baseTexture, data);
            normalTexture = ProceduralTexture.GetNormalTexture(heightTexture, data);
            if (data.HasOcean) specularTexture = ProceduralTexture.GetOceanReflectiveTexture(heightTexture, data);
            if (phase == PreviewDesignPhase.HEIGHTMAP)
            {
                colorTexture = ProceduralTexture.GetHeightTextureColored(baseTexture, heightTexture, data);
                return GenerateMaterial(colorTexture, normalTexture, specularTexture);
            }

            // Climate textures
            Texture2D heatmapTexture = ProceduralTexture.GetHeatTexture(baseTexture, heightTexture, data);
            Texture2D humidityTexture = ProceduralTexture.GetHumidityTexture(heightTexture, data);

            if (phase == PreviewDesignPhase.CLIMATE)
            {
                if (data.PreviewHeat) colorTexture = ProceduralTexture.GetHeatTextureColored(heatmapTexture);
                else colorTexture = ProceduralTexture.GetHumidityTextureColored(humidityTexture);
                specularTexture = null;
                return GenerateMaterial(colorTexture, normalTexture, specularTexture);
            }

            // Biome textures
            colorTexture = ProceduralTexture.GetBiomeTextureColored(heightTexture, heatmapTexture, humidityTexture, data);
            if (phase == PreviewDesignPhase.BIOMES)
            {
                return GenerateMaterial(colorTexture, normalTexture, specularTexture);
            }
            return GenerateMaterial(colorTexture, normalTexture, specularTexture);
        }

        /// <summary>
        /// Get material for the land mass of a (partial) planet.
        /// </summary>
        /// <param name="data">Planet data</param>
        /// <param name="baseTexture">A texture containing vertex values</param>
        /// <param name="textureSize">The size of the textures used in the matertial.</param>
        /// <returns>The procedurally generated material</returns>
        public static Material GetLandMaterial(ProceduralData data, Texture2D baseTexture = null, int textureSize = 256)
        {
            _ = GenerateDataTextures(data, baseTexture, textureSize);
            Texture2D colorTexture = ProceduralTexture.GetGroundTextureColored(s_heatmapTexture, s_humidityTexture, data);
            Texture2D normalTexture = ProceduralTexture.GetNormalTexture(s_heightTexture, data);

            return GenerateMaterial(colorTexture, normalTexture, null);
        }

        /// <summary>
        /// Get material for the ocean of a (partial) planet.
        /// </summary>
        /// <param name="data">Planet data</param>
        /// <param name="baseTexture">A texture containing vertex values</param>
        /// <param name="textureSize">The size of the textures used in the matertial.</param>
        /// <returns>The procedurally generated material</returns>
        public static Material GetOceanMaterial(ProceduralData data, Texture2D baseTexture = null, int textureSize = 256)
        {
            _ = GenerateDataTextures(data, baseTexture, textureSize);
            Texture2D colorTexture = ProceduralTexture.GetOceanTextureColored(s_heightTexture, s_heatmapTexture, data);
            return GenerateMaterial(colorTexture, null, null, data.OceanReflectiveness);
        }

        /// <summary>
        /// This method generates the base data textures: heightmap, heatmap and humiditymap.
        /// They get stored staticly.
        /// </summary>
        /// <param name="data">Planet data</param>
        /// <param name="baseTexture">A texture containing vertex values</param>
        /// <param name="textureSize">The size of the textures used in the matertial.</param>
        /// <returns>The procedurally generated material</returns>
        static Texture2D GenerateDataTextures(ProceduralData data, Texture2D baseTexture, int textureSize)
        {
            if (baseTexture == null) baseTexture = ProceduralTexture.GetBaseTexture(textureSize, textureSize * 3 / 4);
            s_heightTexture = ProceduralTexture.GetHeightTexture(baseTexture, data);
            s_heatmapTexture = ProceduralTexture.GetHeatTexture(baseTexture, s_heightTexture, data);
            s_humidityTexture = ProceduralTexture.GetHumidityTexture(s_heightTexture, data);
            return baseTexture;
        }

        /// <summary>
        /// Generates the final procedural material using the given textures.
        /// </summary>
        /// <param name="colorTexture">Colors of the material</param>
        /// <param name="normalTexture">Surface normals of the material</param>
        /// <param name="specularTexture">The shinyness of the material</param>
        /// <param name="specularValue">The overall shinyness</param>
        /// <returns>The procedural material</returns>
        static Material GenerateMaterial(Texture2D colorTexture, Texture2D normalTexture, Texture2D specularTexture, float specularValue = 0)
        {
            // Material and textures
            Material material = new Material(Shader.Find("Standard"));
            if (colorTexture != null)
            {
                material.SetTexture("_MainTex", colorTexture);
            }
            if (specularTexture != null)
            {
                material.EnableKeyword("_METALLICGLOSSMAP");
                material.SetTexture("_MetallicGlossMap", specularTexture);
            }
            if (specularValue != 0)
            {
                material.EnableKeyword("_METALLIC");
                material.SetFloat("_Metallic", 1f);
                material.EnableKeyword("_GLOSSINESS");
                material.SetFloat("_Glossiness", specularValue);
            }
            if (normalTexture != null)
            {
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_BumpMap", normalTexture);
            }
            return material;
        }
    }
}

