using UnityEngine;

namespace PlanetEngine {

	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class SingleMeshLODInstance : MonoBehaviour {
		public void ApplyMesh(Mesh mesh, ref PlanetData data) {
			mesh = MeshTool.SubdivideGPU(mesh);
			mesh = MeshTool.SubdivideGPU(mesh);
			Mesh local_mesh = Instantiate(mesh);
			local_mesh = MeshTool.SubdivideGPU(local_mesh);
			local_mesh = MeshTool.NormalizeAndAmplify(local_mesh, data.Radius);
            local_mesh = MeshTool.ApplyHeightmap(local_mesh, data.Radius, transform.localToWorldMatrix);
            local_mesh.RecalculateBounds();
			local_mesh.RecalculateNormals();
			local_mesh.RecalculateTangents();
			local_mesh.Optimize();
			GetComponent<MeshFilter>().mesh = local_mesh;
		}

		public void ApplyTexture(Texture2D texture) {
			Material material = new Material(Shader.Find("Standard"));
			material.mainTexture = texture;
			GetComponent<MeshRenderer>().sharedMaterial = material;
		}
	}
}