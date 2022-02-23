using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {

	internal struct BranchData {

		#region Branch Properties
		public bool Divided;
		public int QuadDepth;
		#endregion

		#region Mesh Data
		public Mesh planeMesh;
		#endregion

		#region Textures
		public Texture2D BaseTexture { get { return _baseTexture; } }
		Texture2D _baseTexture;
		#endregion

		public BranchData(CubeSides side) {
			_baseTexture = TextureTool.GenerateBaseTexture(128, 128, side);
			// Generate mesh
			planeMesh = MeshTool.GenerateUnitQuadMesh();
			planeMesh = MeshTool.OffsetMesh(planeMesh, Vector3.up * 0.5f);
			for (int subdiv = 0; subdiv < 5; subdiv++) { planeMesh = MeshTool.SubdivideGPU(planeMesh); }
			Divided = false;
			QuadDepth = 0;
		}

		public BranchData(BranchData parentData, Rect zone) {
			_baseTexture = TextureTool.GenerateBaseTexture(parentData.BaseTexture, zone);
			// Generate mesh
			Bounds parentMeshBounds = parentData.planeMesh.bounds;
			planeMesh = MeshTool.GenerateUnitQuadMesh();
			planeMesh = MeshTool.NormalizeAndAmplify(planeMesh, parentMeshBounds.size.magnitude / 4f);
			planeMesh = MeshTool.OffsetMesh(planeMesh, parentMeshBounds.center + new Vector3(zone.x - 0.25f, 0, zone.y - 0.25f) * 2f * parentMeshBounds.extents.x);
			for (int subdiv = 0; subdiv < 5; subdiv++) { planeMesh = MeshTool.SubdivideGPU(planeMesh); }
			Divided = false;
			QuadDepth = parentData.QuadDepth + 1;
		}
	}

	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	internal class QuadTreeBranch : MonoBehaviour {
		#region Planet engine Interface
		public bool Divided { get { return _data.Divided; } private set { _data.Divided = value; } }
		public BranchData Data { get { return _data; } }
		BranchData _data;
		#endregion

		#region Branching Process
		public void UpdateQuadTree() {
			PlanetData planetData = GetComponentInParent<Planet>().Data;
			MeshRenderer renderer = GetComponent<MeshRenderer>();
			Transform target = Camera.main.transform;
			float targetDistance = Vector3.Distance(target.position, transform.position);
			if (Divided) {
				if (targetDistance > GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude * 1f) {
					Divided = false;
					foreach (QuadTreeBranch branch in GetComponentsInChildren<QuadTreeBranch>()) branch.Divided = false;
					foreach (MeshRenderer childRenderer in GetComponentsInChildren<MeshRenderer>()) childRenderer.enabled = false;
					renderer.enabled = true;
				} else {
					renderer.enabled = false;
					foreach (Transform child in transform) {
						child.GetComponent<MeshRenderer>().enabled = true;
						QuadTreeBranch quadTreeBranch;
						if (child.TryGetComponent(out quadTreeBranch)) quadTreeBranch.UpdateQuadTree();
					}
				}
			} else if (targetDistance < GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude * 0.8f) {
				if (_data.QuadDepth == 0) renderer.enabled = true;
				if (_data.QuadDepth == planetData.MaxDepth) return;
				if (GetComponentsInChildren<QuadTreeBranch>().Length == 1) CreateChildQuads();
				Divided = true;
				renderer.enabled = false;
				foreach (Transform child in transform) {
					child.GetComponent<MeshRenderer>().enabled = true;
					QuadTreeBranch quadTreeBranch;
					if (child.TryGetComponent(out quadTreeBranch)) quadTreeBranch.UpdateQuadTree();
					}
			}
		}
		#endregion

		#region Branch Creation methods
		// Root branch
		public void CreateBranch(CubeSides side) {
			_data = new BranchData(side);
			ApplyBranchData();
		}

		// Non root branches
		public void CreateBranch(QuadTreeBranch parent, Rect zone) {
			_data = new BranchData(parent.Data, zone);
			ApplyBranchData();
		}

		void ApplyBranchData() {
			ApplyMesh(_data.planeMesh);
			ApplyTexture(_data.BaseTexture);
		}

		void ApplyMesh(Mesh planeMesh) {
			PlanetData planetData = GetComponentInParent<Planet>().Data;
			Mesh curvedMesh = Instantiate(planeMesh);
			curvedMesh = MeshTool.NormalizeAndAmplify(curvedMesh, planetData.Radius);
			curvedMesh = MeshTool.SubdivideGPU(curvedMesh);
			Mesh seaMesh = Instantiate(curvedMesh);
			curvedMesh = MeshTool.ApplyHeightmap(curvedMesh, planetData.Radius, transform.localToWorldMatrix);
			curvedMesh.RecalculateBounds();
			Vector3 localMeshCenter = curvedMesh.bounds.center;
			curvedMesh = MeshTool.OffsetMesh(curvedMesh, -localMeshCenter);
			transform.position = transform.TransformPoint(localMeshCenter) - transform.parent.position;
			curvedMesh.RecalculateBounds();
			curvedMesh.RecalculateNormals();
			curvedMesh.RecalculateTangents();
			curvedMesh.Optimize();
			GetComponent<MeshFilter>().mesh = curvedMesh;
			CreateSea(seaMesh);
		}

		void ApplyTexture(Texture2D texture) {
			Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = texture;
            GetComponent<MeshRenderer>().material = material;
		}

		void CreateChildQuads() {
			for (int i = 0; i < 4; i++) {
				GameObject ChildQuadObject = new GameObject(this.name + "." + i);
				ChildQuadObject.transform.SetParent(transform);
				ChildQuadObject.transform.localPosition = Vector3.zero;
				ChildQuadObject.transform.localEulerAngles = Vector3.zero;

				Rect zone = Rect.zero;
				if (i == 0) zone = new Rect(0, 0, 0.5f, 0.5f);
				else if (i == 1) zone = new Rect(0, 0.5f, 0.5f, 0.5f);
				else if (i == 2) zone = new Rect(0.5f, 0, 0.5f, 0.5f);
				else zone = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				ChildQuadObject.AddComponent<QuadTreeBranch>().CreateBranch(this, zone);
			}
			LODGroup lodGroup = GetComponentInParent<Planet>().GetComponent<LODGroup>();
			LOD[] lods = lodGroup.GetLODs();
			List<Renderer> quadRenderers = new List<Renderer>(lods[0].renderers);
			quadRenderers.AddRange(GetComponentsInChildren<Renderer>());
			lods[0].renderers = quadRenderers.ToArray();
			lodGroup.SetLODs(lods);
		}

		public void CreateSea(Mesh mesh)
		{
			mesh.RecalculateBounds();
			Vector3 localMeshCenter = mesh.bounds.center;
			mesh = MeshTool.OffsetMesh(mesh, -localMeshCenter);
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			mesh.Optimize();
			GameObject seaObject = new GameObject("Ocean");
			seaObject.AddComponent<MeshFilter>().mesh = mesh;
			Material material = new Material(Shader.Find("Standard"));
			material.color = Color.blue;
			seaObject.AddComponent<MeshRenderer>().material = material;
			seaObject.transform.parent = transform;
			seaObject.transform.position = transform.TransformPoint(localMeshCenter) - transform.position;
			seaObject.transform.localEulerAngles = Vector3.zero;
		}

		#endregion

	}
}