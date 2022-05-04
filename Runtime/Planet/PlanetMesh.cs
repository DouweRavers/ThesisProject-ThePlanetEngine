using UnityEngine;

namespace PlanetEngine {

	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	internal class PlanetMesh : MonoBehaviour {
		internal void Create(int subdivisions, int textureSize) {
			Planet planet = GetComponentInParent<Planet>();
			GetComponent<MeshFilter>().mesh = ProceduralMesh.GetPlanetMesh(planet, subdivisions);
			GetComponent<MeshRenderer>().sharedMaterial = ProceduralMaterial.GetMaterial(planet.Data, textureSize:textureSize);
		}
	}
}