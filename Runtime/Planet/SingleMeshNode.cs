using UnityEngine;

namespace PlanetEngine {

	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	internal class SingleMeshNode : MonoBehaviour {

		public void CreateMesh(int subdivisions, int textureSize, PlanetData data) {
			ApplyMesh(subdivisions, data);
			ApplyTexture(TextureTool.GenerateBaseTexture(textureSize, textureSize));
		}

		void ApplyMesh(int subdivisions, PlanetData data)
		{
			Mesh mesh = MeshTool.SubdivideGPU(MeshTool.GenerateUnitCubeMesh(), subdivisions);
			mesh = MeshTool.NormalizeAndAmplify(mesh, data.Radius);
			//mesh = MeshTool.ApplyHeightmap(mesh, data, transform.localToWorldMatrix);
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			mesh.Optimize();
			GetComponent<MeshFilter>().mesh = mesh;
		}

		void ApplyTexture(Texture2D texture) {
			Material material = new Material(Shader.Find("Standard"));
			PlanetData planetData = GetComponentInParent<Planet>().Data;
			//material.mainTexture = TextureTool.GenerateColorTexture(TextureTool.GenerateHeightTexture(texture, planetData.Seed), planetData.ColorA, planetData.ColorB);
			GetComponent<MeshRenderer>().sharedMaterial = material;
		}
	}
}