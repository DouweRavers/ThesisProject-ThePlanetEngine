using UnityEngine;

namespace PlanetEngine {

	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	internal class SingleMeshNode : MonoBehaviour {

		public void CreateMesh(Mesh mesh, PlanetData data) {
			ApplyMesh(mesh, data);
			ApplyTexture(TextureTool.GenerateBaseTexture(255,255));
		}

		void ApplyMesh(Mesh mesh, PlanetData data)
		{
			mesh = MeshTool.SubdivideGPU(mesh, 2);
			Mesh localMesh = Instantiate(mesh);
			localMesh = MeshTool.SubdivideGPU(localMesh);
			localMesh = MeshTool.NormalizeAndAmplify(localMesh, data.Radius);
			Mesh seaMesh = Instantiate(localMesh);
			localMesh = MeshTool.ApplyHeightmap(localMesh, data.Radius, transform.localToWorldMatrix);
			localMesh.RecalculateBounds();
			localMesh.RecalculateNormals();
			localMesh.RecalculateTangents();
			localMesh.Optimize();
			GetComponent<MeshFilter>().mesh = localMesh;

			GameObject seaObject = new GameObject("Ocean");
			seaObject.transform.parent = transform;
			OceanNode ocean = seaObject.AddComponent<OceanNode>();
			ocean.CreateOcean(seaMesh, false);
			ocean.ApplyTexture(TextureTool.GenerateBaseTexture(255, 255));

		}

		void ApplyTexture(Texture2D texture) {
			Material material = new Material(Shader.Find("Standard"));
			material.mainTexture = TextureTool.GenerateColorTexture(TextureTool.GenerateHeightTexture(texture), Color.yellow, Color.green); 
			GetComponent<MeshRenderer>().sharedMaterial = material;
		}
	}
}