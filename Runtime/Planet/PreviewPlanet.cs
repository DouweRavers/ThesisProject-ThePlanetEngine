using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// The different steps you have to go through to design a planet using the UI.
    /// </summary>
    public enum PreviewDesignPhase { BASICS, HEIGHTMAP, CLIMATE, BIOMES, VEGETATION, NONE }

    /// <summary>
    /// This component is used for displaying the planet as it is being designed in the editor. It can quickly 
    /// generate low poly versions of the planet and handles changes to the planet settings by regenerating.
    /// </summary>

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PreviewPlanet : BasePlanet
    {
        /// <summary>
        /// preview dedicated to settings of current phase {true} or overall preview {false}.
        /// </summary>
        public bool PreviewCurrentPhase { get; set; } = true;
        /// <summary>
        /// The current visual preview state.
        /// </summary>
        public PreviewDesignPhase Phase { get; set; } = PreviewDesignPhase.BASICS;

        void Start()
        {
            if (Application.isPlaying) GeneratePlanet();
            else Regenerate();
        }

        /// <summary>
        /// Replaces the preview planet with the actual planet. Should only be called at runtime.
        /// </summary>
        public void GeneratePlanet()
        {
            GameObject generatedPlanetObject = new GameObject(name);
            generatedPlanetObject.tag = "PlanetEngine";
            generatedPlanetObject.AddComponent<Planet>().CreateNewPlanet(Data);
            generatedPlanetObject.transform.parent = transform.parent;
            generatedPlanetObject.transform.position = transform.position;
            Destroy(gameObject);
        }

        /// <summary>
        /// Creates a preview mesh according to current phase. If preview settings is disabled it will generate for the last phase.
        /// </summary>
        public void Regenerate()
        {
            GetComponent<MeshFilter>().mesh = ProceduralMesh.GetSizedSphereMesh(Data);
            PreviewDesignPhase phase = PreviewCurrentPhase ? Phase : PreviewDesignPhase.NONE;
            GetComponent<MeshRenderer>().sharedMaterial = ProceduralMaterial.GetMaterial(Data, phase: phase, textureSize: 1024);

        }
    }
}