using UnityEngine;
namespace PlanetEngine
{
    public enum PreviewPhase { BASICS, HEIGHTMAP, CLIMATE, BIOMES, VEGETATION }

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PreviewPlanet : MonoBehaviour
    {
        #region Properties
        public PlanetData Data = new PlanetData(0);

        public bool PreviewSettings = false;
        public PreviewPhase Phase = PreviewPhase.BASICS;
        #endregion

        #region Private attributes
        #endregion

        #region Events
        void Start()
        {
            Regenerate();
        }
        #endregion

        #region Public methods
        public void Regenerate()
        {
            if (PreviewSettings) PreviewCurrentSettings();
            else PreviewFullProcess();
        }
        #endregion

        #region Private methods
        void PreviewCurrentSettings() {
            GetComponent<MeshFilter>().mesh = ProceduralAlgorithm.GeneratePreviewMesh(Data, Phase);
            GetComponent<MeshRenderer>().sharedMaterial = ProceduralAlgorithm.GeneratePreviewMaterial(Data, Phase);
        }

        void PreviewFullProcess() {
            GetComponent<MeshFilter>().mesh = ProceduralAlgorithm.GeneratePreviewMesh(Data, PreviewPhase.BASICS);
            GetComponent<MeshRenderer>().sharedMaterial = ProceduralAlgorithm.GenerateMaterial(Data);
        }
        #endregion

    }
}