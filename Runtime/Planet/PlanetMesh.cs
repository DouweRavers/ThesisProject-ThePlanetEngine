using UnityEngine;

namespace PlanetEngine
{

    /// <summary>
    /// This component contians a single mesh repersenting the entire planet.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    internal class PlanetMesh : MonoBehaviour
    {
        /// <summary>
        /// Generates a single mesh and material for the entire planet. It generates a cube sphere.
        /// </summary>
        /// <param name="subdivisions">amount of divisions staring from a 6 plane cube</param>
        /// <param name="textureSize">Size of the texture</param>
        public void Create(int subdivisions, int textureSize)
        {
            Planet planet = GetComponentInParent<Planet>();
            GetComponent<MeshFilter>().mesh = ProceduralMesh.GetPlanetMesh(planet.Data, planet.transform, subdivisions);
            GetComponent<MeshRenderer>().sharedMaterial = ProceduralMaterial.GetMaterial(planet.Data, textureSize: textureSize);
        }
    }
}