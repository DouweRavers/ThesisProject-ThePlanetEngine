using UnityEngine;

namespace PlanetEngine {

	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class SingleMeshNode : MonoBehaviour {

		public void Create(int subdivisions, int textureSize, PlanetData data) {
			GetComponent<MeshFilter>().mesh = ProceduralMesh.GetPlanetMesh(data, subdivisions);
			GetComponent<MeshRenderer>().sharedMaterial = ProceduralMaterial.GetPhasedMaterial(data, textureSize:textureSize);
		}
	}
}