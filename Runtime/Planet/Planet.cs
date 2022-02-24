using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace PlanetEngine {
	
	public struct PlanetData {
		#region Procedural Properties
		public int Seed;
		#endregion
        
		#region Celestial Properties
        public float Radius;
		#endregion

		#region Rendering Properties
		public int MaxDepth;
		public int LODSphereCount;
		#endregion


		#region Default Values
		const float defaultRadius_c = 10f;
		const int defaultMaxDepth_c = 12;
		const int defaultLODlevels_c = 3;
		#endregion

		public PlanetData(	
			int seed, 
			float radius = defaultRadius_c, 
			int maxDepth = defaultMaxDepth_c, 
			int lodSphereCount = defaultLODlevels_c)
		{
			Seed = seed;
			Radius = radius;
			MaxDepth = maxDepth;
			LODSphereCount = lodSphereCount;
		}
	}

    [ExecuteInEditMode]
	public class Planet : MonoBehaviour {

        #region Properties
        
		#endregion
        
		#region Planet Data 
        [HideInInspector]
		public PlanetData Data { get { return _data; } set { _data = value; } }
		PlanetData _data;
        #endregion

        IEnumerator Start() {

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			CreateNewPlanet();

			stopwatch.Stop();

			int vertices = 0;
			int triangles = 0;
			foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
			{
				vertices += meshFilter.sharedMesh.vertexCount;
				triangles += meshFilter.sharedMesh.triangles.Length;
			}
			UnityEngine.Debug.Log("Created planet with " + vertices + " vertices and " + triangles + " triagles in " + stopwatch.Elapsed.TotalSeconds + "seconds");
			yield return null;
		}

        #region Planet methods
        public void CreateNewPlanet() {
			// Remove previous structures
			LODGroup lodComponent;
			if (TryGetComponent(out lodComponent)) DestroyImmediate(lodComponent);
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++) DestroyImmediate(transform.GetChild(0).gameObject);

			// (Re)generate
			CreateNewPlanetData();
			CreatePlanetFromData();
		}
		#endregion

		#region Data processing methods
		void CreateNewPlanetData() {
			_data = new PlanetData(seed:100);
		}

		void LoadPlanetData() {
			throw new NotImplementedException();
		}

		void SavePlanetData() {
			throw new NotImplementedException();
		}
		#endregion

		#region Planet Generation methods
		void CreatePlanetFromData() {
			List<Transform> LevelsOfDetail = new List<Transform>();
			CreateSingleMeshObjects(LevelsOfDetail);
			CreateQuadTreeObject(LevelsOfDetail);
			LevelsOfDetail.Reverse();
			ConfigureLOD(LevelsOfDetail);
		}

		void CreateSingleMeshObjects(List<Transform> LODlist) {
			Mesh mesh = MeshTool.GenerateUnitCubeMesh();
			for (int i = 0; i < _data.LODSphereCount; i++) {
				GameObject singleMeshObject = new GameObject(gameObject.name + " - LODSphere: " + i);
				singleMeshObject.tag = "PlanetEngine";
				singleMeshObject.transform.SetParent(transform);
				singleMeshObject.AddComponent<SingleMeshNode>().CreateMesh(mesh, _data);
				LODlist.Add(singleMeshObject.transform);
			}
		}

		void CreateQuadTreeObject(List<Transform> LODlist) {
			GameObject quadRootObject = new GameObject(gameObject.name + " - QuadRoot");
			quadRootObject.tag = "PlanetEngine";
			quadRootObject.transform.SetParent(transform);
			quadRootObject.AddComponent<QuadTreeRootNode>().CreateQuadTree();
			LODlist.Add(quadRootObject.transform);
		}

		void ConfigureLOD(List<Transform> LODlist) {
			LOD[] lodArray = new LOD[LODlist.Count];
			for (int i = 0; i < LODlist.Count; i++) {
				lodArray[i].screenRelativeTransitionHeight = 0.8f * Mathf.Pow(1 - ((float)i / LODlist.Count), 3);
				lodArray[i].renderers = LODlist[i].GetComponentsInChildren<MeshRenderer>();
			}
			LODGroup lodComponent = gameObject.AddComponent<LODGroup>();
			lodComponent.SetLODs(lodArray);
			lodComponent.RecalculateBounds();
		}
		#endregion
	}
}