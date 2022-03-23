using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    public class Planet : MonoBehaviour
    {

        #region Properties
        public PlanetData Data { get { return _data; } set { _data = value; } }
        PlanetData _data = new PlanetData();
        #endregion

        #region Events
        void Start()
        {
            CreateNewPlanet();
        }
        #endregion

        #region Public methods
        public void CreateNewPlanet()
        {
            // Remove previous structures
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++) DestroyImmediate(transform.GetChild(0).gameObject);

            if (_data.Seed == 0) _data = new PlanetData();
            CreatePlanetFromData();
        }

        public void CreateNewPlanet(int seed)
        {
            _data = new PlanetData();
            CreateNewPlanet();
        }

        public void CreateNewPlanet(PlanetData data)
        {
            _data = data;
            CreateNewPlanet();
        }
        #endregion

        #region Private methods
        void CreatePlanetFromData()
        {
            List<Transform> LevelsOfDetail = new List<Transform>();
            CreateSingleMeshObjects(LevelsOfDetail);
            CreateQuadTreeObject(LevelsOfDetail);
            LevelsOfDetail.Reverse();
            ConfigureLOD(LevelsOfDetail);
        }

        void CreateSingleMeshObjects(List<Transform> LODlist)
        {
            for (int i = 0; i < _data.LODSphereCount; i++)
            {
                GameObject singleMeshObject = new GameObject(gameObject.name + " - LODSphere: " + i);
                singleMeshObject.tag = "PlanetEngine";
                singleMeshObject.transform.SetParent(transform);
                singleMeshObject.AddComponent<SingleMeshNode>().CreateMesh(2 * i + 1, 256 * (int)Mathf.Pow(2, i), _data);
                LODlist.Add(singleMeshObject.transform);
            }
        }

        void CreateQuadTreeObject(List<Transform> LODlist)
        {
            GameObject quadRootObject = new GameObject(gameObject.name + " - QuadRoot");
            quadRootObject.tag = "PlanetEngine";
            quadRootObject.transform.SetParent(transform);
            quadRootObject.AddComponent<QuadTreeRootNode>().CreateQuadTree();
            LODlist.Add(quadRootObject.transform);
        }

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
        #endregion
    }
}