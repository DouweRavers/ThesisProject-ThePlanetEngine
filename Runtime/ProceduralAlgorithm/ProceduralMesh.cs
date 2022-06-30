using UnityEngine;

namespace PlanetEngine
{
    internal class ProceduralMesh : ScriptableObject
    {
        internal static Mesh GetSizedSphereMesh(BasePlanet planet)
        {
            Mesh mesh = MeshModifier.Subdivide(MeshPrimitives.UnitCube, 6);
            mesh = MeshModifier.NormalizeAndAmplify(mesh, planet.Data.Radius);
            return Polish(mesh);
        }

        internal static Mesh GetPlanetMesh(BasePlanet planet, int subdivisions = 6)
        {
            Mesh mesh = MeshModifier.Subdivide(MeshPrimitives.UnitCube, subdivisions);
            mesh = MeshModifier.NormalizeAndAmplify(mesh, planet.Data.Radius);
            mesh = MeshModifier.ApplyHeightmap(mesh, planet.Data, planet.transform.position, planet.transform);
            return Polish(mesh);
        }

        internal static Mesh GetBranchPlaneMesh(float size, Vector3 offset)
        {
            Mesh planeMesh = MeshPrimitives.UnitQuad;
            planeMesh = MeshModifier.NormalizeAndAmplify(planeMesh, size);
            planeMesh = MeshModifier.Offset(planeMesh, offset);
            planeMesh = MeshModifier.Subdivide(planeMesh, 5);
            return planeMesh;
        }

        internal static Mesh GetBranchMesh(BaseBranch branch, Mesh planeMesh, bool flat = false)
        {
            // Keep reference the planet
            Planet planet = branch.GetComponentInParent<Planet>();
            // Create new mesh and apply procedural changes
            Mesh mesh = MeshModifier.NormalizeAndAmplify(planeMesh, planet.Data.Radius);
            mesh = MeshModifier.Subdivide(mesh);
            if (!flat)
            {
                /* Here should procedural change be added.*/
                mesh = MeshModifier.ApplyHeightmap(mesh, planet.Data, planet.transform.position, branch.transform);
            }
            // Recenter mesh so the mesh center is the center of the geometric shape.
            mesh.RecalculateBounds();
            Vector3 localMeshCenter = mesh.bounds.center;
            mesh = MeshModifier.Offset(mesh, -localMeshCenter);
            // Set the branch position so the geometric shape will still appear in the same place.
            Vector3 planetPosition = planet.transform.position;
            Vector3 parentPosition = branch.transform.parent.position;
            branch.transform.position = branch.transform.TransformPoint(localMeshCenter) - parentPosition + planetPosition;
            return Polish(mesh);
        }

        static Mesh Polish(Mesh mesh)
        {
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.Optimize();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
