using UnityEngine;
using System;


namespace PlanetEngine {
	public struct PlanetData {
		public Texture2D baseTexture;
		public Texture2D heightTexture;
		public Texture2D biomeTexture;
		public Texture2D terrainColorTexture;

		public PlanetData(Texture2D baseTexture) {
			this.baseTexture = baseTexture;
			heightTexture = TextureTool.GenerateHeightTexture(baseTexture);
			biomeTexture = TextureTool.GenerateBiomeTexture(baseTexture, heightTexture);
			terrainColorTexture = TextureTool.GenerateTerrainColorTexture(biomeTexture, heightTexture);
		}
	}

	[ExecuteInEditMode]
	public class Planet : MonoBehaviour {
		public PlanetData data;
		public int maxDepth = 3, maxLOD = 4;
		public Transform target;
		void Start() {
			GeneratePlanetData();
			CreatePlanet();
		}

		public void CreatePlanet() {
			GameObject[] LODSpheres = CreateLODSphereMeshes(maxLOD);
			GameObject QuadTree = CreateQuadTree();
			// Create A LOD group that manages the spherical and Quad tree mesh
			LOD[] lodArray = new LOD[LODSpheres.Length + 1];
			for (int i = 0; i < lodArray.Length; i++) {
				lodArray[i].screenRelativeTransitionHeight = 0.8f * Mathf.Pow(1 - ((float)i / LODSpheres.Length), 3);
				if (i == 0) lodArray[0].renderers = (Renderer[])QuadTree.GetComponentsInChildren<MeshRenderer>();
				else lodArray[i].renderers = new Renderer[] { (Renderer)LODSpheres[i - 1].GetComponent<MeshRenderer>() };
			}
			LODGroup lodComponent = gameObject.GetComponent<LODGroup>();
			if (lodComponent == null) lodComponent = gameObject.AddComponent<LODGroup>();
			lodComponent.SetLODs(lodArray);
			lodComponent.RecalculateBounds();
		}

		void GeneratePlanetData() {
			Mesh mesh = MeshGenerator.GenerateUnitCubeMesh();
			Texture2D baseTexture = TextureTool.GenerateBaseTextureGPU(100, 75); // texture should be 4 x 3
			data = new PlanetData(baseTexture);
		}

		GameObject[] CreateLODSphereMeshes(int count) {
			Mesh mesh = MeshGenerator.GenerateUnitCubeMesh();

			GameObject[] LODSpheres = new GameObject[count];
			for (int i = 0; i < count; i++) {
				// Create object holding the mesh
				GameObject meshObject = new GameObject();
				meshObject.name = gameObject.name + " - LODSphere: " + i;
				meshObject.tag = "PlanetEngine";
				meshObject.transform.SetParent(transform);
				LODSpheres[i] = meshObject;

				// Create sphere mesh with certain complexity (subdivisions) 
				mesh = MeshGenerator.SubdivideGPU(mesh);
				Mesh local_mesh = Instantiate(mesh);
				local_mesh = MeshGenerator.SubdivideGPU(local_mesh);
				local_mesh = MeshGenerator.NormalizeAndAmplify(local_mesh, 1);
				local_mesh = MeshGenerator.ApplyHeightmap(local_mesh, data.heightTexture);
				local_mesh.Optimize();
				local_mesh.RecalculateBounds();
				local_mesh.RecalculateNormals();
				local_mesh.RecalculateTangents();
				meshObject.AddComponent<MeshFilter>().mesh = local_mesh;

				// Create material for current mesh
				Material material = new Material(Shader.Find("Standard"));
				material.mainTexture = data.terrainColorTexture;
				meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;
			}
			// resort from big to small
			Array.Reverse(LODSpheres);
			return LODSpheres;
		}

		GameObject CreateQuadTree() {
			GameObject QuadRootObject = new GameObject();
			QuadRootObject.name = gameObject.name + " - QuadRoot";
			QuadRootObject.tag = "PlanetEngine";
			QuadTreeRoot quadTreeRoot = QuadRootObject.AddComponent<QuadTreeRoot>();
			quadTreeRoot.CreateQuadTree(this);
			QuadRootObject.transform.SetParent(transform);
			return QuadRootObject;
		}
	}
}