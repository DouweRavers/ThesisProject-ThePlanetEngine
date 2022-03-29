using UnityEngine;

namespace PlanetEngine {

	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	internal class SingleMeshNode : MonoBehaviour {

		public void Create(int subdivisions, int textureSize, PlanetData data) {
			GetComponent<MeshFilter>().mesh = ProceduralAlgorithm.GenerateHeightenedSphereMesh(data, subdivisions);
			GetComponent<MeshRenderer>().sharedMaterial = ProceduralAlgorithm.GenerateMaterial(data, textureSize:textureSize);
		}
	}
}