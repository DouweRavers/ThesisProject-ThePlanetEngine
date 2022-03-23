using UnityEngine;

namespace PlanetEngine
{
    internal static class ProceduralAlgorithm
    {
        public static Material GenerateMaterial(PlanetData data)
        {
            Material material = new Material(Shader.Find("Standard"));
            Texture2D baseTexture = TextureTool.GenerateBaseTexture(256, 256);
            //            Texture2D heightTexture = TextureTool.GenerateHeightTexture(texture, data.Seed);

            material.mainTexture = baseTexture;
            return material;
        }

        public static Mesh GenerateMesh(PlanetData data)
        {
            return null;
        }

        public static Material GeneratePreviewMaterial(PlanetData data, PreviewPhase phase)
        {
            Material material = new Material(Shader.Find("Standard"));
            switch (phase)
            {
                case PreviewPhase.BASICS:
                    material.color = Color.white;
                    break;
                case PreviewPhase.HEIGHTMAP:
                    Texture2D baseTexture = BaseTexture.GetTexture(256, 256);
                    Texture2D heightTexture = HeightmapTexture.GetTextureHeightValue(baseTexture, data);

                    Texture2D colorTexture = HeightmapTexture.GetTextureColored(baseTexture, heightTexture, data);
                    material.SetTexture("_MainTex", colorTexture);
                    Texture2D normalTexture = null;
                    material.SetTexture("_BumpMap", normalTexture);
                    if (data.HasOcean)
                    {
                        Texture2D specularTexture = TextureTool.GenerateHeightmapReflectiveTexture(heightTexture);
                        material.EnableKeyword("_METALLICGLOSSMAP");
                        material.SetTexture("_MetallicGlossMap", specularTexture);
                        material.SetFloat("_Glossiness", 0.7f);
                    }
                    break;
                case PreviewPhase.CLIMATE:
                    break;
                case PreviewPhase.BIOMES:
                    break;
                case PreviewPhase.VEGETATION:
                    break;
                default:
                    break;
            }
            return material;
        }

        public static Mesh GeneratePreviewMesh(PlanetData data, PreviewPhase phase)
        {

            Mesh mesh = new Mesh();
            switch (phase)
            {
                case PreviewPhase.BASICS:
                    mesh = MeshTool.SubdivideGPU(MeshTool.GenerateUnitCubeMesh(), 6);
                    mesh = MeshTool.NormalizeAndAmplify(mesh, data.Radius);
                    break;
                case PreviewPhase.HEIGHTMAP:
                    mesh = MeshTool.SubdivideGPU(MeshTool.GenerateUnitCubeMesh(), 6);
                    mesh = MeshTool.NormalizeAndAmplify(mesh, data.Radius);
                    break;
                case PreviewPhase.CLIMATE:
                    mesh = MeshTool.SubdivideGPU(MeshTool.GenerateUnitCubeMesh(), 6);
                    mesh = MeshTool.NormalizeAndAmplify(mesh, data.Radius);
                    break;
                case PreviewPhase.BIOMES:
                    break;
                case PreviewPhase.VEGETATION:
                    break;
                default:
                    break;
            }
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.Optimize();
            return mesh;
        }
    }
}

