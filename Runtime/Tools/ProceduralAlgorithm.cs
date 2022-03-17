using UnityEngine;
using UnityEditor;

namespace PlanetEngine {
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
                    Texture2D baseTexture = TextureTool.GenerateBaseTexture(256, 256);
                    Texture2D heightTexture = TextureTool.GenerateHeightTexture(baseTexture, data.Seed);
                    Texture2D colorTexture = heightTexture;
                    Texture2D normalTexture = Texture2D.whiteTexture;
                    Texture2D specularTexture = heightTexture;
                    material.SetTexture("_MainTex", colorTexture);
                    material.SetTexture("_BumpMap", normalTexture);
                    material.SetTexture("_MetallicGlossMap", specularTexture);
                    AssetDatabase.CreateAsset(material, "Assets/ThesisProject-ThePlanetEngine/Runtime/Tools/Resources/material.mat");
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

