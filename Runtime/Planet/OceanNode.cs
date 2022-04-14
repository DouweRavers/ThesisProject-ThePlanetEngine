using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {

	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))] 
	public class OceanNode : MonoBehaviour
    {
		public bool Visible {
			get { return GetComponent<MeshRenderer>().enabled; }
			set { GetComponent<MeshRenderer>().enabled = value; } 
		}

		public void CreateOcean(Mesh planeMesh, Texture2D baseTexture) {
			Planet planet = transform.GetComponentInParent<Planet>();

			GetComponent<MeshFilter>().mesh = ProceduralMesh.GetBranchMesh(planeMesh, transform, true);
			GetComponent<MeshRenderer>().material = ProceduralMaterial.GetOceanMaterial(planet.Data, baseTexture);
		}
	}
}
