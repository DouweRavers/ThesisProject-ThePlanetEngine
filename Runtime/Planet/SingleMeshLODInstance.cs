using UnityEngine;

namespace PlanetEngine {

	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	internal class SingleMeshLODInstance : MonoBehaviour {
		public void ApplyMesh(Mesh mesh, PlanetData data) {
			mesh = MeshTool.SubdivideGPU(mesh, 2);
			Mesh local_mesh = Instantiate(mesh);
			local_mesh = MeshTool.SubdivideGPU(local_mesh);
			local_mesh = MeshTool.NormalizeAndAmplify(local_mesh, data.Radius);
			CreateSea(local_mesh);
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

		public void CreateSea(Mesh mesh) {
			mesh = Instantiate(mesh);
			GameObject seaObject = new GameObject();
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			mesh.Optimize();
			seaObject.AddComponent<MeshFilter>().mesh = mesh;
			Material material = new Material(Shader.Find("Standard"));
			material.color = Color.blue;
			seaObject.AddComponent<MeshRenderer>().material = material;
			seaObject.transform.parent = transform;
			seaObject.transform.localPosition = Vector3.zero;
			seaObject.transform.localEulerAngles = Vector3.zero;
		}
	}
}