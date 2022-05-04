using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {

	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))] 
	internal class OceanBranch : BaseBranch
    {
		internal override bool Visible {
			get { return GetComponent<MeshRenderer>().enabled; }
			set { GetComponent<MeshRenderer>().enabled = value; } 
		}

		internal void CreateOcean(Mesh planeMesh, Texture2D baseTexture) {
			Planet planet = transform.GetComponentInParent<Planet>();
			GetComponent<MeshFilter>().mesh = ProceduralMesh.GetBranchMesh(this, planeMesh, true);
			GetComponent<MeshRenderer>().material = ProceduralMaterial.GetOceanMaterial(planet.Data, baseTexture);
		}
	}
}
