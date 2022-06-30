using UnityEngine;

namespace PlanetEngine
{
    internal static class ProceduralMaterial
    {
        static Texture2D heightTexture, heatmapTexture, humidityTexture;

        internal static Material GetMaterial(PlanetData data, Texture2D baseTexture = null, PreviewDesignPhase phase = PreviewDesignPhase.NONE, int textureSize = 256)
        {
            Texture2D colorTexture = null, normalTexture = null, specularTexture = null;

            // Basic planet
            if (baseTexture == null)
            {
                baseTexture = ProceduralTexture.GetBaseTexture(textureSize, textureSize * 3 / 4);
            }
            if (phase == PreviewDesignPhase.BASICS)
            {
                colorTexture = baseTexture;
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

        internal static Material GetLandMaterial(PlanetData data, Texture2D baseTexture = null, int textureSize = 256)
        {
            baseTexture = GenerateDataTextures(data, textureSize, baseTexture);
            Texture2D colorTexture = ProceduralTexture.GetGroundTextureColored(heatmapTexture, humidityTexture, data);
            return GenerateMaterial(colorTexture, null, null);
        }

        internal static Material GetOceanMaterial(PlanetData data, Texture2D baseTexture = null, int textureSize = 256)
        {
            baseTexture = GenerateDataTextures(data, textureSize, baseTexture);
            Texture2D colorTexture = ProceduralTexture.GetOceanTextureColored(heightTexture, heatmapTexture, data);
            return GenerateMaterial(colorTexture, null, null, data.OceanReflectiveness);
        }

        internal static Material GetAtmosphereMaterial(PlanetData data)
        {
            // Material and textures
            Material material = new Material(Shader.Find("Standard"));
            material.color = data.AtmosphereColor;
            if (data.HasClouds)
            {
                Texture2D baseTexture = ProceduralTexture.GetBaseTexture(256, 256);
                Texture2D heightTexture = ProceduralTexture.GetHeightTexture(baseTexture, data);
                material.mainTexture = ProceduralTexture.GetHeightTextureColored(baseTexture, heightTexture, data);
            }

            // Enable fade mode on standard shader
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;

            return material;
        }

        static Texture2D GenerateDataTextures(PlanetData data, int textureSize, Texture2D baseTexture)
        {
            if (baseTexture == null) baseTexture = ProceduralTexture.GetBaseTexture(textureSize, textureSize * 3 / 4);
            heightTexture = ProceduralTexture.GetHeightTexture(baseTexture, data);
            heatmapTexture = ProceduralTexture.GetHeatTexture(baseTexture, heightTexture, data);
            humidityTexture = ProceduralTexture.GetHumidityTexture(heightTexture, data);
            return baseTexture;
        }

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

