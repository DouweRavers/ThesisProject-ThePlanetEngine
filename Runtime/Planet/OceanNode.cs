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

		public void CreateOcean(Mesh mesh, bool isBranch) {
			if (isBranch) {
				mesh.RecalculateBounds();
				Vector3 localMeshCenter = mesh.bounds.center;
				mesh = MeshTool.OffsetMesh(mesh, -localMeshCenter);
				transform.position = transform.parent.TransformPoint(localMeshCenter) - transform.parent.position;
			} else transform.localPosition = Vector3.zero;
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
			material.mainTexture = TextureTool.GenerateColorTexture(TextureTool.GenerateHeightTexture(texture), Color.blue, Color.black);
			GetComponent<MeshRenderer>().sharedMaterial = material;
		}
	}
}
