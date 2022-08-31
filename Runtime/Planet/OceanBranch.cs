using UnityEngine;

namespace PlanetEngine
{

    /// <summary>
    /// The ocean branch is a component used for displaying a ocean in the Quad tree.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    internal class OceanBranch : BaseBranch
    {
        public override bool Visible
        {
            get { return GetComponent<MeshRenderer>().enabled; }
            set { GetComponent<MeshRenderer>().enabled = value; }
        }

        /// <summary>
        /// Generates a procedural mesh and material for this ocean node.
        /// </summary>
        /// <param name="planeMesh">The base mesh the procedural mesh is based upon.</param>
        /// <param name="baseTexture">A texture containing normalized vertex representing the planemesh.</param>
        public void CreateOcean(Mesh planeMesh, Texture2D baseTexture)
        {
            Planet planet = transform.GetComponentInParent<Planet>();
            GetComponent<MeshFilter>().mesh = ProceduralMesh.GetBranchMesh(this, planeMesh, true);
            GetComponent<MeshRenderer>().material = ProceduralMaterial.GetOceanMaterial(planet.Data, baseTexture);
        }
    }
}
