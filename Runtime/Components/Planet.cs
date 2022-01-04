using UnityEngine;
using System;
using System.Collections.Generic;


namespace PlanetEngine {
	public struct PlanetData {
		#region Data Textures
		// Expl
		public Texture2D BaseTexture {
			get { return _baseTexture; }
			set {
				_baseTexture = value;
				_heightTexture = TextureTool.GenerateHeightTextureThreaded(_baseTexture, 10);
				_biomeTexture = TextureTool.GenerateBiomeTextureGPU(_baseTexture, 100);
			}
		}
		Texture2D _baseTexture;

		public Texture2D HeightTexture { get { return _heightTexture; } }
		Texture2D _heightTexture;

		public Texture2D BiomeTexture { get { return _biomeTexture; } }
		Texture2D _biomeTexture;
		#endregion

		#region Celestial Properties
		public float Radius;
		#endregion

		#region Planet settings
		public int MaxDepth;
		public int LODSphereCount;
		#endregion
		public PlanetData(RectInt textureSize, float radius = 1f, int maxDepth = 3, int lodSphereCount = 2) {
			_baseTexture = TextureTool.GenerateBaseTextureGPU(textureSize.width, textureSize.height);
			_heightTexture = TextureTool.GenerateHeightTextureThreaded(_baseTexture, 10);
			_biomeTexture = TextureTool.GenerateBiomeTextureGPU(_baseTexture, 100);
			Radius = radius;
			MaxDepth = maxDepth;
			LODSphereCount = lodSphereCount;
		}
	}

	[ExecuteInEditMode]
	public class Planet : MonoBehaviour {

		#region Unity Interface
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
			Data = new PlanetData(new RectInt(0, 0, 1200, 900));
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
				SingleMeshLODInstance singleMeshLODInstance = singleMeshObject.AddComponent<SingleMeshLODInstance>();
				singleMeshLODInstance.ApplyMesh(mesh, ref _data);
				singleMeshLODInstance.ApplyTexture(_data.BaseTexture);
				LODlist.Add(singleMeshObject.transform);
			}
		}

		void CreateQuadTreeObject(List<Transform> LODlist) {
			GameObject quadRootObject = new GameObject(gameObject.name + " - QuadRoot");
			quadRootObject.tag = "PlanetEngine";
			quadRootObject.transform.SetParent(transform);
			quadRootObject.AddComponent<QuadTreeRoot>().CreateQuadTree();
			LODlist.Add(quadRootObject.transform);
		}

		void ConfigureLOD(List<Transform> LODlist) {
			LOD[] lodArray = new LOD[LODlist.Count];
			for (int i = 0; i < LODlist.Count; i++) {
				lodArray[i].screenRelativeTransitionHeight = 0.8f * Mathf.Pow(1 - ((float)i / LODlist.Count), 3);
				lodArray[i].renderers = (Renderer[])LODlist[i].GetComponentsInChildren<MeshRenderer>();
			}
			LODGroup lodComponent = gameObject.AddComponent<LODGroup>();
			lodComponent.SetLODs(lodArray);
			lodComponent.RecalculateBounds();
		}
		#endregion
	}
}