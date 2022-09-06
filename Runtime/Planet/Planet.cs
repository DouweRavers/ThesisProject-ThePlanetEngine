using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// A component that manages the in game planet/terrain engine. It assumes the planet properties do not change.
    /// </summary>
    public class Planet : BasePlanet
    {
        public bool useTerrainSystem = false;

        /// <summary>
        /// The target for rendering the quad tree
        /// </summary>
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

        /// <summary>
        /// Is a terrain or the quad tree/LOD.
        /// </summary>
        bool _terrainMode = false;

        private void Start()
        {
            Vector3 vector = GetClosestPointToSurface(Target.position);
            Debug.DrawLine(transform.position, vector, Color.red, Mathf.Infinity);
            Data.MaxDepth = 6;
        }

        /// <summary>
        /// Destroys previous generated planet and generates a new one instead.
        /// Planet data is set to default but seed is applied.
        /// </summary>
        /// <param name="seed">Seed for randomizer process.</param>
        public void CreateNewPlanet(int seed)
        {
            // Create new data and set seed.
            Data.Seed = seed;
            CreateNewPlanet();
        }

        /// <summary>
        /// Destroys previous generated planet and generates a new one instead.
        /// Planet data is set to given data.
        /// </summary>
        /// <param name="data">A struct containing generation properties for the planet.</param>
        public void CreateNewPlanet(ProceduralData data)
        {
            Data = data;
            CreateNewPlanet();
        }

        /// <summary>
        /// Destroys previous generated planet and generates a new one instead.
        /// Planet data is set to default.
        /// </summary>
        void CreateNewPlanet()
        {
            // Remove previous structures
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++) DestroyImmediate(transform.GetChild(0).gameObject);

            CreatePlanetFromData();
        }

        /// <summary>
        /// Takes a vector in 3D space and returns the closest point on the terrain surface to that point.
        /// </summary>
        public Vector3 GetClosestPointToSurface(Vector3 position)
        {
            Vector3 planetToTargetDirection = (position - transform.position).normalized;
            Vector3 localVertex = planetToTargetDirection * Data.Radius;
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] { localVertex };
            mesh = MeshModifier.ApplyHeightmap(mesh, Data, transform.position, transform);
            localVertex = mesh.vertices[0];
            return transform.TransformPoint(localVertex);
        }

        /// <summary>
        /// The planet goes from tree/LOD rendering to terrain rendering.
        /// </summary>
        public void SwitchToTerrain()
        {
            if (!useTerrainSystem) return;
            if (_terrainMode) return;
            _terrainMode = true;
            GetComponentInChildren<PlanetTerrain>().CreateTerrain();
            GetComponentInChildren<TreeRoot>().RemoveRootBranches();

            GameObject shifter = new GameObject();
            Transform parent = Target.parent;
            shifter.transform.SetParent(Target);
            shifter.transform.localPosition = Vector3.zero;

            shifter.transform.SetParent(transform);
            shifter.transform.LookAt(GetClosestPointToSurface(Target.position));
            Target.SetParent(shifter.transform);

            // Position player at center
            float distance = Vector3.Distance(GetClosestPointToSurface(Target.position), Target.position);
            Vector3 terrainPos = GetComponentInChildren<PlanetTerrain>().transform.position;
            terrainPos += GetComponentInChildren<Terrain>().terrainData.size / 2f;
            shifter.transform.position = terrainPos + Vector3.up * distance;

            shifter.transform.LookAt(terrainPos);
            Target.SetParent(parent);
            Destroy(shifter);

        }

        /// <summary>
        /// The planet goes from terrain rendering to tree/LOD rendering.
        /// </summary>
        public void SwitchToTree()
        {
            if (!_terrainMode) return;
            _terrainMode = false;
            TreeRoot tree = GetComponentInChildren<TreeRoot>();
            PlanetTerrain terrain = GetComponentInChildren<PlanetTerrain>();
            tree.CreateRootBranches();
            terrain.DestroyTerrain();

            GameObject shifter = new GameObject();
            Transform parent = Target.parent;
            shifter.transform.SetParent(Target);
            shifter.transform.localPosition = Vector3.zero;

            shifter.transform.SetParent(transform);
            shifter.transform.LookAt(GetClosestPointToSurface(Target.position));
            Target.SetParent(shifter.transform);

            // Position player at center
            float distance = Vector3.Distance(GetClosestPointToSurface(Target.position), Target.position);
            Vector3 terrainPos = GetComponentInChildren<PlanetTerrain>().transform.position;
            terrainPos += GetComponentInChildren<Terrain>().terrainData.size / 2f;
            shifter.transform.position = terrainPos + Vector3.up * distance;

            shifter.transform.LookAt(terrainPos);
            Target.SetParent(parent);
            Destroy(shifter);
        }

        /// <summary>
        /// Creates LOD spheres and root of the quad tree.
        /// </summary>
        private void CreatePlanetFromData()
        {
            List<Transform> LevelsOfDetail = new List<Transform>();
            CreateSingleMeshObjects(LevelsOfDetail);
            CreateQuadTreeObject(LevelsOfDetail);
            CreateTerrainObject();
            LevelsOfDetail.Reverse();
            ConfigureLOD(LevelsOfDetail);
        }

        /// <summary>
        /// Creates LOD spheres as child objects.
        /// </summary>
        private void CreateSingleMeshObjects(List<Transform> LODlist)
        {
            for (int i = 0; i < Data.LODSphereCount; i++)
            {
                GameObject singleMeshObject = new GameObject(gameObject.name + " - LODSphere: " + i);
                singleMeshObject.tag = "PlanetEngine";
                singleMeshObject.transform.SetParent(transform);
                singleMeshObject.AddComponent<PlanetMesh>().Create(2 * i + 1, 256 * (int)Mathf.Pow(2, i));
                LODlist.Add(singleMeshObject.transform);
            }
        }

        /// <summary>
        /// Creates the root of the quad tree as child object.
        /// </summary>
        private void CreateQuadTreeObject(List<Transform> LODlist)
        {
            GameObject quadRootObject = new GameObject(gameObject.name + " - QuadRoot");
            quadRootObject.tag = "PlanetEngine";
            quadRootObject.transform.SetParent(transform);
            quadRootObject.AddComponent<TreeRoot>().CreateRootBranches();
            LODlist.Add(quadRootObject.transform);
        }

        /// <summary>
        /// Creates a terrain object as child object. It is not part of the LOD system.
        /// </summary>
        private void CreateTerrainObject()
        {
            GameObject terrainObject = new GameObject(gameObject.name + " - Terrain");
            terrainObject.tag = "PlanetEngine";
            terrainObject.transform.SetParent(transform);
            terrainObject.AddComponent<PlanetTerrain>();
        }

        /// <summary>
        /// Adds meshes of the LOD spheres and quad tree to the LOD component.
        /// </summary>
        private void ConfigureLOD(List<Transform> LODlist)
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