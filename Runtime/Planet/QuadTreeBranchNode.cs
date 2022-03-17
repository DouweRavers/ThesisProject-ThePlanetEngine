using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {

	internal struct BranchData {
		public int QuadDepth;
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
			QuadDepth = parentData.QuadDepth + 1;
		}
	}

	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	internal class QuadTreeBranchNode : MonoBehaviour {
		#region Properties
		public bool Visible
		{
			get { return GetComponent<MeshRenderer>().enabled; }
			set { 
				GetComponent<MeshRenderer>().enabled = value;
				OceanNode oceanNode = GetComponentInChildren<OceanNode>();
				if (oceanNode != null) oceanNode.Visible = value;
			}
		}
		public bool Divided = false;

		Transform target = null;
		#endregion

		#region Planet engine Interface
		public BranchData Data { get { return _data; } }
		BranchData _data;
        #endregion

        #region Branching Process
        public void UpdateQuadTree() {
			if(target == null) target = Camera.main.transform;
			float targetDistance = Vector3.Distance(target.position, transform.position);
			if (Divided) {
				if (targetDistance > GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude * 1f) QuadTreeShrink();
				else QuadTreeUpdateChildBranches();
			} else if (targetDistance < GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude * 0.8f) QuadTreeExpand();
		}

		void QuadTreeShrink() {
			foreach (QuadTreeBranchNode branch in GetComponentsInChildren<QuadTreeBranchNode>()) {
				branch.Divided = false;
				branch.Visible = false;
			}
			Visible = true;
		}

		void QuadTreeUpdateChildBranches() {
			if (Visible) {
				Visible = false;
				foreach (QuadTreeBranchNode branch in GetComponentsInChildren<QuadTreeBranchNode>()) branch.Visible = true;
			}
			foreach (QuadTreeBranchNode branch in GetComponentsInChildren<QuadTreeBranchNode>()) if(branch != this) branch.UpdateQuadTree();
		}

		void QuadTreeExpand() {
			if (_data.QuadDepth == 0) Visible = true;
			if (_data.QuadDepth == GetComponentInParent<Planet>().Data.MaxDepth) return;
			if (GetComponentsInChildren<QuadTreeBranchNode>().Length < 2) CreateChildQuads();
			Divided = true;
			Visible = false;
			foreach (QuadTreeBranchNode branch in GetComponentsInChildren<QuadTreeBranchNode>()) {
				if (branch == this) continue;
				branch.Visible = true;
				branch.UpdateQuadTree();
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
		public void CreateBranch(QuadTreeBranchNode parent, Rect zone) {
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
			//curvedMesh = MeshTool.ApplyHeightmap(curvedMesh, planetData, transform.localToWorldMatrix);
			curvedMesh.RecalculateBounds();
			Vector3 localMeshCenter = curvedMesh.bounds.center;
			curvedMesh = MeshTool.OffsetMesh(curvedMesh, -localMeshCenter);
			transform.position = transform.TransformPoint(localMeshCenter) - transform.parent.position;
			curvedMesh.RecalculateBounds(); 
			curvedMesh.RecalculateNormals();
			curvedMesh.RecalculateTangents();
			curvedMesh.Optimize();
			GetComponent<MeshFilter>().mesh = curvedMesh;
			
			GameObject seaObject = new GameObject("Ocean");
			seaObject.transform.parent = transform;
			OceanNode ocean = seaObject.AddComponent<OceanNode>();
			ocean.CreateOcean(seaMesh);
			ocean.ApplyTexture(_data.BaseTexture);
		}

		void ApplyTexture(Texture2D texture) {
			Material material = new Material(Shader.Find("Standard"));
			PlanetData planetData = GetComponentInParent<Planet>().Data;
			//material.mainTexture = TextureTool.GenerateColorTexture(TextureTool.GenerateHeightTexture(texture, planetData.Seed), planetData.ColorA, planetData.ColorB);
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
				ChildQuadObject.AddComponent<QuadTreeBranchNode>().CreateBranch(this, zone);
			}
			LODGroup lodGroup = GetComponentInParent<Planet>().GetComponent<LODGroup>();
			LOD[] lods = lodGroup.GetLODs();
			List<Renderer> quadRenderers = new List<Renderer>(lods[0].renderers);
			quadRenderers.AddRange(GetComponentsInChildren<Renderer>());
			lods[0].renderers = quadRenderers.ToArray();
			lodGroup.SetLODs(lods);
		}
		#endregion

	}
}