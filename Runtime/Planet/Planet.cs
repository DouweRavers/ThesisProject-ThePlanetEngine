using UnityEngine;
using System;
using System.Collections.Generic;

namespace PlanetEngine {
	public struct PlanetData {
		#region Celestial Properties
		public float Radius;
		#endregion

		#region Planet settings
		public int MaxDepth;
		public int LODSphereCount;
		#endregion

		#region Textures
		public Texture2D BaseTexture { get { return _baseTexture; } }
		Texture2D _baseTexture;
		#endregion

		#region Constructors
		public PlanetData(float radius, int maxDepth, int lodSphereCount) {
			_baseTexture = TextureTool.GenerateBaseTexture(256, 256);
			Radius = radius;
			MaxDepth = maxDepth;
			LODSphereCount = lodSphereCount;
		}
		#endregion
	}

    [ExecuteInEditMode]
	public class Planet : MonoBehaviour {

		#region DefaultValues
		const float defaultRadius_c = 10f;
		const int defaultMaxDepth_c = 12;
		const int defaultLODlevels_c = 3;
		#endregion

		#region Planet Data access parameters
		public int MaxDepth { get { return _data.MaxDepth; } set { _data.MaxDepth = value; } }
		public float Radius { get { return _data.Radius; } set { _data.Radius = value; } }
		public int LODSphereCount { get { return _data.LODSphereCount; } set { _data.LODSphereCount = value; } }
		#endregion

		#region Planet Engine Interface parameters
		[HideInInspector]
		public PlanetData Data { get { return _data; } set { _data = value; } }
		PlanetData _data;
		#endregion

		#region Planet Engine Interface methods
		public void CreateNewPlanet() {
			CreateNewPlanetData();
			CreatePlanetFromData();
		}
		#endregion

		#region Data processing methods
		void CreateNewPlanetData() {
			Data = new PlanetData(defaultRadius_c, defaultMaxDepth_c, defaultLODlevels_c);
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