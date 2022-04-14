using UnityEngine;

namespace PlanetEngine
{
    public class ProceduralMesh : ScriptableObject
    {
        public static Mesh GetPlanetMesh(PlanetData data, int subdivisions = 6)
        {
            Mesh mesh = MeshTool.SubdivideGPU(MeshTool.GenerateUnitCubeMesh(), subdivisions);
            mesh = MeshTool.NormalizeAndAmplify(mesh, data.Radius);
            // Add height.
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.Optimize();
            return mesh;
        }

        public static Mesh GetBranchMesh(Mesh planeMesh, Transform transform, bool flat = false)
        {
            // Keep reference the planet
            Planet planet = transform.GetComponentInParent<Planet>();
            // Create new mesh and apply procedural changes
            Mesh mesh = Instantiate(planeMesh);
            mesh = MeshTool.NormalizeAndAmplify(mesh, planet.Data.Radius);
            mesh = MeshTool.SubdivideGPU(mesh);
            if (!flat)
            {
                /* Here should procedural change be added.*/
            }

            // Recenter mesh so the mesh center is the center of the geometric shape.
            mesh.RecalculateBounds();
            Vector3 localMeshCenter = mesh.bounds.center;
            mesh = MeshTool.OffsetMesh(mesh, -localMeshCenter);
            // Set the branch position so the geometric shape will still appear in the same place.
            Vector3 planetPosition = planet.transform.position;
            transform.position = transform.TransformPoint(localMeshCenter) - transform.parent.position + planetPosition;

            // Polish mesh
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.Optimize();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
