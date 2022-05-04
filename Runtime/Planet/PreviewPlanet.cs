using UnityEngine;

namespace PlanetEngine
{
    public enum PreviewPhase { BASICS, HEIGHTMAP, CLIMATE, BIOMES, VEGETATION, NONE }

    /// <summary>
    /// This component is used for displaying the planet as it is being designed (in the editor). It can quickly 
    /// generate low poly versions of the planet and handles changes to the planet settings by regenerating the mesh.
    /// 
    /// No terrain features are incorperated. So no colliders or LODs are generated. 
    /// </summary>

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PreviewPlanet : BasePlanet
    {
        public bool PreviewSettings = true;
        public PreviewPhase Phase = PreviewPhase.BASICS;

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
            
            GetComponent<MeshFilter>().mesh = ProceduralMesh.GetSizedSphereMesh(this);
            PreviewPhase phase = PreviewSettings ? Phase : PreviewPhase.NONE;
            GetComponent<MeshRenderer>().sharedMaterial = ProceduralMaterial.GetMaterial(Data, phase: phase, textureSize: 1024);
            if ((!PreviewSettings || PreviewPhase.CLIMATE <= Phase) && Data.HasAtmosphere)
            {
                GameObject Atmosphere = new GameObject("Atmosphere");
                Atmosphere.tag = "PlanetEngine";
                Atmosphere.transform.SetParent(transform);
                Atmosphere.transform.localScale = Vector3.one * 1.1f;
                Atmosphere.AddComponent<MeshFilter>().mesh = ProceduralMesh.GetSizedSphereMesh(this);
                Atmosphere.AddComponent<MeshRenderer>().sharedMaterial = ProceduralMaterial.GetAtmosphereMaterial(Data);
                Atmosphere.hideFlags = HideFlags.HideInHierarchy;
            }
        }
    }
}