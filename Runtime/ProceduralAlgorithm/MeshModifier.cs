using UnityEngine;

namespace PlanetEngine
{
    internal class MeshModifier : ScriptableObject
    {
        /// <summary>
        /// Splits the triangles of the mesh into 4 smaller triangles. Increasing the surface detail.
        /// </summary>
        /// <param name="mesh">The target mesh to be subdivided</param>
        /// <param name="iterations">The amount of subdivisions</param>
        /// <returns>The subdivided mesh</returns>
        public static Mesh Subdivide(Mesh mesh, int iterations = 1)
        {
            MeshCompute generator = CreateInstance<MeshCompute>();
            generator.SetKernel("SubdivideMesh");
            generator.SetInputMesh(mesh);
            int outputIndexSize = mesh.triangles.Length;
            generator.SetOutputMeshProperties(outputIndexSize * 2, outputIndexSize * 4);
            for (int i = 0; i < iterations - 1; i++)
            {
                generator.Execute(outputIndexSize / 3);
                outputIndexSize = outputIndexSize * 4;
                generator.SetOutputMeshAsInput(outputIndexSize * 2, outputIndexSize * 4);
            }
            return generator.GetOutputMesh(outputIndexSize / 3);
        }

        /// <summary>
        /// Normalizes every vertex and multiplies it by an amplifier value.
        /// </summary>
        /// <param name="mesh">The source mesh</param>
        /// <param name="amplifier">The amplification value</param>
        /// <returns>The modified mesh</returns>
        public static Mesh NormalizeAndAmplify(Mesh mesh, float amplifier)
        {
            MeshCompute generator = CreateInstance<MeshCompute>();
            generator.SetKernel("NormalizeAndAmplify");
            generator.SetInputMesh(vertices: mesh.vertices);
            generator.AddValue("amplifier", amplifier);
            generator.SetOutputMeshProperties();
            return generator.GetOutputMesh(mesh.vertexCount, mesh);
        }

        /// <summary>
        /// Offset the mesh by a given offset. It adds the offset vector to every vertex.
        /// </summary>
        /// <param name="mesh">The source mesh</param>
        /// <param name="offset">The offset to be applied</param>
        /// <returns>The modified mesh</returns>
        public static Mesh Offset(Mesh mesh, Vector3 offset)
        {
            MeshCompute generator = CreateInstance<MeshCompute>();
            generator.SetKernel("Offset");
            generator.SetInputMesh(vertices: mesh.vertices);
            generator.AddValue("offset_vector", offset);
            generator.SetOutputMeshProperties();
            return generator.GetOutputMesh(mesh.vertexCount, mesh);
        }

        /// <summary>
        /// Applies the heightmap from the planetsettings. Assuming full or partial sphere as input mesh.
        /// </summary>
        /// <param name="mesh">The source mesh</param>
        /// <param name="data">The planet data</param>
        /// <param name="planetPosition">The center of the planet object</param>
        /// <param name="transform">The transfrom of the planet</param>
        /// <returns></returns>
        public static Mesh ApplyHeightmap(Mesh mesh, PlanetData data, Vector3 planetPosition, Transform transform)
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