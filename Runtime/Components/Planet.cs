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
		public Texture2D ColorTexture { get { return _colorTexture; } }
		Texture2D _colorTexture;
		#endregion

		#region Celestial Properties
		public float Radius;
		#endregion

		#region Planet settings
		public int MaxDepth;
		public int LODSphereCount;
		#endregion
		public PlanetData(RectInt textureSize, float radius, int maxDepth, int lodSphereCount) {
			_baseTexture = TextureTool.GenerateBaseTextureGPU(textureSize.width, textureSize.height);
			_heightTexture = TextureTool.GenerateHeightTextureThreaded(_baseTexture, 10);
			_biomeTexture = TextureTool.GenerateBiomeTextureGPU(_baseTexture, 100);
			_colorTexture = _baseTexture;
			Radius = radius;
			MaxDepth = maxDepth;
			LODSphereCount = lodSphereCount;
		}
	}

	[ExecuteInEditMode]
	public class Planet : MonoBehaviour {

		#region DefaultValues
		const float defaultRadius_c = 1f;
		const int defaultMaxDepth_c = 3;
		const int defaultLODlevels_c = 3;
		#endregion

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
			// Create texture size based on polycount
			float estimatePolyCount = 2 * 9 * 6 * Mathf.Pow(4, defaultLODlevels_c * 2);
			// Cubemap has only 50% used spaces so area x2
			float estimateArea = 2 * estimatePolyCount;
			// For equal area per face, a 4x3 format is required
			float height = Mathf.Sqrt(3f / 4 * estimateArea);
			float width = 4f / 3 * height;
			Debug.Log("PC:" + estimatePolyCount + ", A: " + estimateArea + ", H:" + height + ", W: " + width);
			Data = new PlanetData(new RectInt(0, 0, (int)width, (int)height), defaultRadius_c, defaultMaxDepth_c, defaultLODlevels_c);
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
				singleMeshLODInstance.ApplyTexture(_data.HeightTexture);
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