using UnityEngine;

namespace PlanetEngine
{
	[RequireComponent(typeof(LODGroup))]
	public class Planet : MonoBehaviour
	{
		public Shader baseMaterialShader;
		void Start()
		{
		}

		void Update()
		{
		}

		public void CreatePlanet()
		{
			// Create A LOD group that manages the spherical and Quad tree mesh
			LOD[] lodArray = new LOD[4];
			// Create a base mesh to generate the different LOD levels out of.
			Mesh mesh = MeshGenerator.GenerateUnitCubeMesh();
			// Generate 3 LOD levels
			for (int i = 0; i < 3; i++)
			{
				// Create a object for the current LOD mesh 
				GameObject meshObject = new GameObject();
				meshObject.transform.SetParent(transform);
				meshObject.name = "LODSphere" + i;
				meshObject.tag = "PlanetEngine";
				MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
				meshRenderer.material = new Material(baseMaterialShader);
				MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
				// Increase level of detail of mesh
				Mesh local_mesh = MeshGenerator.SubdivideGPU(mesh);
				local_mesh = MeshGenerator.SubdivideGPU(local_mesh);
				mesh = Instantiate(local_mesh); // copy divided cube for next iteration
				local_mesh = MeshGenerator.NormalizeAndAmplify(local_mesh, 1);
				local_mesh = MeshGenerator.OffsetMesh(local_mesh, Vector3.down);
				local_mesh.RecalculateBounds();
				local_mesh.RecalculateNormals();
				local_mesh.RecalculateTangents();
				local_mesh.Optimize();
				meshFilter.mesh = local_mesh;
				// Set LOD properties
				lodArray[3 - i].screenRelativeTransitionHeight = (i == 0 ? 0.005f : (i == 1 ? .1f : 0.3f));
				lodArray[3 - i].renderers = new Renderer[] { (Renderer)meshRenderer };
			}
			// Create the Quad Tree Object it uses a dummy renderer to trigger internal rendering
			GameObject QuadRootObject = new GameObject();
			QuadRootObject.name = "QuadRoot";
			QuadRootObject.tag = "PlanetEngine";
			Renderer triggerRenderer = QuadRootObject.AddComponent<MeshRenderer>();
			QuadRootObject.AddComponent<MeshFilter>();
			QuadRootObject.transform.SetParent(transform);
			// Make highest LOD level the Quad trigger
			lodArray[0].screenRelativeTransitionHeight = 0.99f;
			lodArray[0].renderers = new Renderer[] { (Renderer)triggerRenderer };
			// Apply LOD group
			GetComponent<LODGroup>().SetLODs(lodArray);
			GetComponent<LODGroup>().RecalculateBounds();
		}
	}
}