using UnityEngine;
namespace PlanetEngine
{
    public enum PreviewPhase { BASICS, HEIGHTMAP, CLIMATE, BIOMES, VEGETATION, NONE }

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PreviewPlanet : MonoBehaviour
    {
        public PlanetData Data = null;
        public bool PreviewSettings = true;
        public PreviewPhase Phase = PreviewPhase.BASICS;

        void Start()
        {
            Data = ScriptableObject.CreateInstance<PlanetData>();
            try
            {
                Data.LoadData(name);
            }
            catch (System.Exception)
            {
                Data = ScriptableObject.CreateInstance<PlanetData>();
            }
            Regenerate();
        }

        /// <summary>
        /// Destroys current preview mesh and recreates a new one.
        /// </summary>
        public void Regenerate()
        {
            if (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);
            GeneratePreview();
        }

        // Creates a preview mesh according to current phase. If preview settings is disabled it will generate for the last phase.
        void GeneratePreview()
        {
            GetComponent<MeshFilter>().mesh = ProceduralAlgorithm.GenerateHeightenedSphereMesh(Data);
            PreviewPhase phase = PreviewSettings ? Phase : PreviewPhase.NONE;
            GetComponent<MeshRenderer>().sharedMaterial = ProceduralAlgorithm.GenerateMaterial(Data, phase, textureSize: 1024);
            if ((!PreviewSettings || PreviewPhase.CLIMATE <= Phase) && Data.HasAtmosphere)
            {
                GameObject Atmosphere = new GameObject("Atmosphere");
                Atmosphere.tag = "PlanetEngine";
                Atmosphere.transform.SetParent(transform);
                Atmosphere.AddComponent<MeshFilter>().mesh = ProceduralAlgorithm.GenerateSphereMesh(Data.Radius * 1.1f);
                Atmosphere.AddComponent<MeshRenderer>().sharedMaterial = ProceduralAlgorithm.GenerateAtmosphereMaterial(Data);
                Atmosphere.hideFlags = HideFlags.HideInHierarchy;
            }
        }
    }
}