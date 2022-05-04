using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PlanetEngine
{
    internal class MeshModifier : ScriptableObject
    {
        internal static Mesh Subdivide(Mesh mesh, int iterations = 1)
        {
            MeshCompute generator = CreateInstance<MeshCompute>();
            generator.SetKernel("SubdivideMesh");
            generator.SetInputMesh(mesh);
            int outputIndexSize = mesh.triangles.Length;
            generator.SetOutputMeshProperties(outputIndexSize * 2, outputIndexSize * 4);
            for (int i = 0; i < iterations - 1; i++)
            {
                generator.Execute(outputIndexSize/3);
                outputIndexSize = outputIndexSize * 4;
                generator.SetOutputMeshAsInput(outputIndexSize * 2, outputIndexSize * 4);
            }
            return generator.GetOutputMesh(outputIndexSize / 3);
        }

        internal static Mesh NormalizeAndAmplify(Mesh mesh, float amplifier)
        {
            MeshCompute generator = CreateInstance<MeshCompute>();
            generator.SetKernel("NormalizeAndAmplify");
            generator.SetInputMesh(vertices:mesh.vertices);
            generator.AddValue("amplifier", amplifier);
            generator.SetOutputMeshProperties();
            return generator.GetOutputMesh(mesh.vertexCount, mesh);
        }

        internal static Mesh Offset(Mesh mesh, Vector3 offset)
        {
            MeshCompute generator = CreateInstance<MeshCompute>();
            generator.SetKernel("Offset");
            generator.SetInputMesh(vertices: mesh.vertices);
            generator.AddValue("offset_vector", offset);
            generator.SetOutputMeshProperties();
            return generator.GetOutputMesh(mesh.vertexCount, mesh);
        }

        internal static Mesh ApplyHeightmap(Mesh mesh, PlanetData data, Vector3 planetPosition, Transform transform)
        {
            MeshCompute generator = CreateInstance<MeshCompute>();
            generator.SetKernel("ApplyHeightmap");
            generator.SetInputMesh(vertices: mesh.vertices);
            generator.AddValue("seed", data.Seed);
            generator.AddValue("continent_scale", data.ContinentScale);
            generator.AddValue("octaves", 10);
            generator.AddValue("height_difference", data.heightDifference);
            generator.AddValue("planet_center", planetPosition);
            generator.AddValue("object_to_world", transform.localToWorldMatrix);
            generator.AddValue("world_to_object", transform.worldToLocalMatrix);
            generator.SetOutputMeshProperties();
            return generator.GetOutputMesh(mesh.vertexCount, mesh);
        }
    }
}