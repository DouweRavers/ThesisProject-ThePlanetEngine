using UnityEngine;

namespace PlanetEngine
{
    internal static class ProceduralAlgorithm
    {
        public static Material GenerateMaterial(PlanetData data, PreviewPhase phase = PreviewPhase.NONE, int textureSize = 256)
        {
            // Material and textures
            Material material = new Material(Shader.Find("Standard"));
            Texture2D colorTexture = null;
            Texture2D normalTexure = null;
            Texture2D specularTexture = null;


            // Basic planet
            Texture2D baseTexture = ProceduralTexture.GetBaseTexture(textureSize, textureSize * 3 / 4);
            if (phase == PreviewPhase.BASICS)
            {
                colorTexture = baseTexture;
                goto ReturnMaterial;
            }

            // Generate heights
            Texture2D heightTexture = ProceduralTexture.GetHeightTexture(baseTexture, data);
            normalTexure = ProceduralTexture.GetNormalTexture(heightTexture, data);
            if (data.HasOcean) specularTexture = ProceduralTexture.GetOceanReflectiveTexture(heightTexture, data);
            if (phase == PreviewPhase.HEIGHTMAP)
            {
                colorTexture = ProceduralTexture.GetHeightTextureColored(baseTexture, heightTexture, data);
                goto ReturnMaterial;
            }

            // Climate textures
            Texture2D heatmapTexture = ProceduralTexture.GetHeatTexture(baseTexture, heightTexture, data);
            Texture2D humidityTexture = ProceduralTexture.GetHumidityTexture(heightTexture, data);

            if (phase == PreviewPhase.CLIMATE)
            {
                if(data.previewHeat) colorTexture = ProceduralTexture.GetHeatTextureColored(heatmapTexture);
                else colorTexture = ProceduralTexture.GetHumidityTextureColored(humidityTexture);
                specularTexture = null;
                goto ReturnMaterial;
            }

            // Biome textures
            colorTexture = ProceduralTexture.GetBiomeTextureColored(heightTexture, heatmapTexture, humidityTexture, data);
            if (phase == PreviewPhase.BIOMES)
            {
                goto ReturnMaterial;
            }
        ReturnMaterial:

            material.SetTexture("_MainTex", colorTexture);
            if (specularTexture != null) material.EnableKeyword("_METALLICGLOSSMAP");
            material.SetTexture("_MetallicGlossMap", specularTexture);
            if (normalTexure != null) material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_BumpMap", normalTexure);
            return material;
        }

        public static Material GenerateBranchMaterial(PlanetData data, Texture2D baseTexture, int textureSize = 256)
        {
            // Material and textures
            Material material = new Material(Shader.Find("Standard"));
            Texture2D heightTexture = ProceduralTexture.GetHeightTexture(baseTexture, data);
            Texture2D specularTexture = null;
            if (data.HasOcean) specularTexture = ProceduralTexture.GetOceanReflectiveTexture(heightTexture, data);
            Texture2D heatmapTexture = ProceduralTexture.GetHeatTexture(baseTexture, heightTexture, data);
            Texture2D humidityTexture = ProceduralTexture.GetHumidityTexture(heightTexture, data);
            Texture2D colorTexture = ProceduralTexture.GetBiomeTextureColored(heightTexture, heatmapTexture, humidityTexture, data);
            material.SetTexture("_MainTex", colorTexture);
            if (specularTexture != null) material.EnableKeyword("_METALLICGLOSSMAP");
            material.SetTexture("_MetallicGlossMap", specularTexture);
            return material;
        }

        public static Material GenerateAtmosphereMaterial(PlanetData data)
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

        public static Mesh GenerateSphereMesh(float radius, int subdivisions = 6)
        {
            Mesh mesh = MeshTool.SubdivideGPU(MeshTool.GenerateUnitCubeMesh(), subdivisions);
            mesh = MeshTool.NormalizeAndAmplify(mesh, radius);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.Optimize();
            return mesh;
        }

        public static Mesh GenerateHeightenedSphereMesh(PlanetData data, int subdivisions = 6)
        {
            Mesh mesh = MeshTool.SubdivideGPU(MeshTool.GenerateUnitCubeMesh(), subdivisions);
            mesh = MeshTool.NormalizeAndAmplify(mesh, data.Radius);
            // add height
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.Optimize();
            return mesh;
        }

        public static Mesh ApplyPlanetPropertiesOnBranchMesh(PlanetData data, Mesh baseMesh, Matrix4x4 transformMatrix)
        {
            return baseMesh;
        }
    }
}

