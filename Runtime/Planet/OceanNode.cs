using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	internal class OceanNode : MonoBehaviour
    {
		public bool Visible {
			get { return GetComponent<MeshRenderer>().enabled; }
			set { GetComponent<MeshRenderer>().enabled = value; } 
		}

		public void CreateOcean(Mesh mesh) {
			mesh.RecalculateBounds();
			Vector3 localMeshCenter = mesh.bounds.center;
			mesh = MeshTool.OffsetMesh(mesh, -localMeshCenter);
			transform.position = transform.parent.TransformPoint(localMeshCenter) - transform.parent.position;
			transform.localEulerAngles = Vector3.zero;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			mesh.Optimize();
			GetComponent<MeshFilter>().mesh = mesh;

			Material material = new Material(Shader.Find("Standard"));
			material.color = Color.blue;
			GetComponent<MeshRenderer>().material = material;
		}

		public void ApplyTexture(Texture2D texture)
		{
			Material material = new Material(Shader.Find("Standard"));
			PlanetData planetData = GetComponentInParent<Planet>().Data;
			//material.mainTexture = TextureTool.GenerateColorTexture(TextureTool.GenerateHeightTexture(texture, planetData.Seed), planetData.ColorC, planetData.ColorD);
			GetComponent<MeshRenderer>().sharedMaterial = material;
		}
	}
}
