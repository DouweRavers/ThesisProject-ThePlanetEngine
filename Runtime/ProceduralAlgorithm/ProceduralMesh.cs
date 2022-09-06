using UnityEngine;

namespace PlanetEngine
{
    internal class ProceduralMesh : ScriptableObject
    {
        /// <summary>
        /// Generates a sphere mesh with given planet radius.
        /// </summary>
        /// <param name="data">The planet data</param>
        /// <returns></returns>
        public static Mesh GetSizedSphereMesh(ProceduralData data)
        {
            Mesh mesh = MeshModifier.Subdivide(MeshPrimitives.UnitCube, 6);
            mesh = MeshModifier.NormalizeAndAmplify(mesh, data.Radius);
            return Polish(mesh);
        }

        /// <summary>
        /// Generates a spherical mesh representing the planet. It also applies the heightmap onto the sphere surface.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="planetTranform"></param>
        /// <param name="subdivisions"></param>
        /// <returns></returns>
        public static Mesh GetPlanetMesh(ProceduralData data, Transform planetTranform, int subdivisions = 6)
        {
            Mesh mesh = MeshModifier.Subdivide(MeshPrimitives.UnitCube, subdivisions);
            mesh = MeshModifier.NormalizeAndAmplify(mesh, data.Radius);
            mesh = MeshModifier.ApplyHeightmap(mesh, data, planetTranform.position, planetTranform);
            return Polish(mesh);
        }

        /// <summary>
        /// Generates a planemesh that is part on a sized cube.
        /// </summary>
        /// <param name="size">The size of the full cube mesh</param>
        /// <param name="offset">The offset of the current mesh onto the cube mesh</param>
        /// <returns>A offset planemesh</returns>
        public static Mesh GetBranchPlaneMesh(float size, Vector3 offset)
        {
            Mesh planeMesh = MeshPrimitives.UnitQuad;
            planeMesh = MeshModifier.NormalizeAndAmplify(planeMesh, size);
            planeMesh = MeshModifier.Offset(planeMesh, offset);
            planeMesh = MeshModifier.Subdivide(planeMesh, 4);
            return planeMesh;
        }

        /// <summary>
        /// Generates a partial spherical  branched mesh with heightmap.
        /// </summary>
        /// <param name="branch">A reference to the branch object</param>
        /// <param name="planeMesh">A planemesh associated with current mesh</param>
        /// <param name="flat">If true a smooth surface is generated otherwise a heightmap is applied</param>
        /// <returns>A partial spherical mesh</returns>
        public static Mesh GetBranchMesh(BaseBranch branch, Mesh planeMesh, bool flat = false)
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

        /// <summary>
        /// Calculates the normals, tangents, optimizes the mesh and recalculates the mesh bounds.
        /// </summary>
        /// <param name="mesh">The unpolished mesh</param>
        /// <returns>The polished mesh</returns>
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
