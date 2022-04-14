using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// A component that manages the in game planet/terrain engine. It assumes the planet properties do not change.
    /// </summary>
    public class Planet : MonoBehaviour
    {
        // The target for rendering the quad tree
        public Transform Target
        {
            get
            {
                if (_target == null) _target = Camera.main.transform;
                return _target;
            }
            set { _target = value; }
        }
        Transform _target;

        // Holds all data conserning the planet generation process.
        public PlanetData Data
        {
            get
            {
                if (_data == null)
                {
                    _data = ScriptableObject.CreateInstance<PlanetData>();
                    try
                    {
                        _data.LoadData(name);
                    }
                    catch (System.Exception)
                    {
                        Debug.LogWarning("No data file exists for this planet, new one is created.");
                    }
                }
                return _data;
            }
            set
            {
                _data = value;
                CreateNewPlanet();
            }

        }
        PlanetData _data;

        /// <summary>
        /// Destroys previous generated planet and generates a new one instead.
        /// Planet data is set to default.
        /// </summary>
        public void CreateNewPlanet()
        {
            // Remove previous structures
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++) DestroyImmediate(transform.GetChild(0).gameObject);

            // if no data was loaded create new data. 
            if (_data == null) _data = ScriptableObject.CreateInstance<PlanetData>();
            CreatePlanetFromData();
        }

        /// <summary>
        /// Destroys previous generated planet and generates a new one instead.
        /// Planet data is set to default but seed is applied.
        /// </summary>
        /// <param name="seed">Seed for randomizer process.</param>
        public void CreateNewPlanet(int seed)
        {
            // Create new data and set seed.
            _data = ScriptableObject.CreateInstance<PlanetData>();
            _data.Seed = seed;
            CreateNewPlanet();
        }

        /// <summary>
        /// Destroys previous generated planet and generates a new one instead.
        /// Planet data is set to given data.
        /// </summary>
        /// <param name="data">A struct containing generation properties for the planet.</param>
        public void CreateNewPlanet(PlanetData data)
        {
            _data = data;
            CreateNewPlanet();
        }

        // Creates LOD spheres and root of the quad tree.
        void CreatePlanetFromData()
        {
            List<Transform> LevelsOfDetail = new List<Transform>();
            CreateSingleMeshObjects(LevelsOfDetail);
            CreateQuadTreeObject(LevelsOfDetail);
            LevelsOfDetail.Reverse();
            ConfigureLOD(LevelsOfDetail);
        }

        // Creates LOD spheres as child objects.
        void CreateSingleMeshObjects(List<Transform> LODlist)
        {
            for (int i = 0; i < _data.LODSphereCount; i++)
            {
                GameObject singleMeshObject = new GameObject(gameObject.name + " - LODSphere: " + i);
                singleMeshObject.tag = "PlanetEngine";
                singleMeshObject.transform.SetParent(transform);
                singleMeshObject.AddComponent<SingleMeshNode>().Create(2 * i + 1, 256 * (int)Mathf.Pow(2, i), _data);
                LODlist.Add(singleMeshObject.transform);
            }
        }

        // Creates the root of the quad tree as child object.
        void CreateQuadTreeObject(List<Transform> LODlist)
        {
            GameObject quadRootObject = new GameObject(gameObject.name + " - QuadRoot");
            quadRootObject.tag = "PlanetEngine";
            quadRootObject.transform.SetParent(transform);
            quadRootObject.AddComponent<QuadTreeRootNode>().CreateQuadTree();
            LODlist.Add(quadRootObject.transform);
        }

        // Adds meshes of the LOD spheres and quad tree to the LOD component.
        void ConfigureLOD(List<Transform> LODlist)
        {
            LOD[] lodArray = new LOD[LODlist.Count];
            for (int i = 0; i < LODlist.Count; i++)
            {
                lodArray[i].screenRelativeTransitionHeight = 0.8f * Mathf.Pow(1 - ((float)i / LODlist.Count), 3);
                lodArray[i].renderers = LODlist[i].GetComponentsInChildren<MeshRenderer>();
            }
            LODGroup lodComponent;
            if (!TryGetComponent(out lodComponent)) lodComponent = gameObject.AddComponent<LODGroup>();
            lodComponent.SetLODs(lodArray);
            lodComponent.RecalculateBounds();
        }
    }
}