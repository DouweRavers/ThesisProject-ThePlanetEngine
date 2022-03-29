using System.IO;
using UnityEngine;
namespace PlanetEngine
{
    public enum PreviewPhase { BASICS, HEIGHTMAP, CLIMATE, BIOMES, VEGETATION, NONE }

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PreviewPlanet : MonoBehaviour
    {
        #region Properties
        public PlanetData Data = null;
        public bool PreviewSettings = true;
        public PreviewPhase Phase = PreviewPhase.BASICS;
        #endregion

        #region Private attributes
        #endregion

        #region Events
        void Start()
        {
            Data = ScriptableObject.CreateInstance<PlanetData>();
            if (File.Exists("Assets/PlanetEngineData/" + name + "-planetData.json"))
            {
                Data.LoadData(name);
            }
            Regenerate();
        }
        #endregion

        #region Public methods
        public void Regenerate()
        {
            if (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);
            if (PreviewSettings) PreviewCurrentSettings();
            else PreviewFullProcess();
        }
        #endregion

        #region Private methods
        void PreviewCurrentSettings()
        {
            GetComponent<MeshFilter>().mesh = ProceduralAlgorithm.GenerateHeightenedSphereMesh(Data);
            GetComponent<MeshRenderer>().sharedMaterial = ProceduralAlgorithm.GenerateMaterial(Data, Phase, textureSize:1024);
            if (Data.HasAtmosphere)
            {
                GameObject Atmosphere = new GameObject("Atmosphere");
                Atmosphere.tag = "PlanetEngine";
                Atmosphere.transform.SetParent(transform);
                Atmosphere.AddComponent<MeshFilter>().mesh = ProceduralAlgorithm.GenerateSphereMesh(Data.Radius * 1.1f);
                Atmosphere.AddComponent<MeshRenderer>().sharedMaterial = ProceduralAlgorithm.GenerateAtmosphereMaterial(Data);
                Atmosphere.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        void PreviewFullProcess()
        {
            GetComponent<MeshFilter>().mesh = ProceduralAlgorithm.GenerateHeightenedSphereMesh(Data);
            GetComponent<MeshRenderer>().sharedMaterial = ProceduralAlgorithm.GenerateMaterial(Data, textureSize: 1024);
        }
        #endregion

    }
}