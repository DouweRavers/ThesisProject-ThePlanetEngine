using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {

	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshCollider))]
	public class QuadTreeBranch : MonoBehaviour {
		public new Renderer renderer { get { return GetComponent<MeshRenderer>(); } }
		public bool divided = false;
		public Mesh PlaneMesh {
			set {
				planeMesh = value;
				Mesh[] meshes = MeshGenerator.SplitPlaneMeshInFour(planeMesh);
				for (int i = 0; i < transform.childCount; i++) {
					QuadTreeBranch branch = transform.GetChild(i).GetComponent<QuadTreeBranch>();
					if (branch != null) {
						Mesh planeSubMesh = meshes[i];
						branch.PlaneMesh = MeshGenerator.SubdivideGPU(planeSubMesh);
					}
				}
			}
		}
		Texture2D baseTexture;

		Mesh planeMesh;
		int quadDepth = 0;

		// should be called after object has right parent
		public void Init(int quadDepth, Mesh planeMesh, Texture2D parentBaseTexture, Rect zone) {
			Planet planet = GetComponentInParent<Planet>();
			// assing values
			this.planeMesh = planeMesh;
			this.quadDepth = quadDepth;
			this.baseTexture = TextureTool.RegenerateBaseTextureForSubSurface(parentBaseTexture, zone, new RectInt(0, 0, 1024, 1024));
			Texture2D heightmap = TextureTool.GenerateHeightTextureThreaded(baseTexture, 3);


			Mesh curvedMesh = Instantiate(planeMesh);
			curvedMesh = MeshGenerator.NormalizeAndAmplify(curvedMesh, planet.radius);
			curvedMesh = MeshGenerator.SubdivideGPU(curvedMesh);
			curvedMesh = MeshGenerator.ApplyHeightmap(curvedMesh, heightmap, planet.radius);
			curvedMesh.RecalculateBounds();
			Vector3 localMeshCenter = curvedMesh.bounds.center;
			curvedMesh = MeshGenerator.OffsetMesh(curvedMesh, -localMeshCenter);
			transform.position = transform.TransformPoint(localMeshCenter) - transform.parent.position;
			curvedMesh.RecalculateBounds();
			curvedMesh.RecalculateNormals();
			curvedMesh.RecalculateTangents();
			curvedMesh.Optimize();
			GetComponent<MeshFilter>().mesh = curvedMesh;
			GetComponent<MeshCollider>().sharedMesh = curvedMesh;

			Material material = new Material(Shader.Find("Standard"));
			material.mainTexture = heightmap;
			GetComponent<MeshRenderer>().material = material;

		}

		public void UpdateQuadTree(Transform target) {
			Planet planet = GetComponentInParent<Planet>();
			float targetDistance = Vector3.Distance(target.position, transform.position);
			if (divided) {
				if (targetDistance > GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude * 1.2f) {
					divided = false;
					foreach (QuadTreeBranch branch in GetComponentsInChildren<QuadTreeBranch>()) {
						branch.renderer.enabled = false;
						branch.divided = false;
					}
					renderer.enabled = true;
				} else {
					renderer.enabled = false;
					foreach (Transform child in transform) {
						QuadTreeBranch branch = child.GetComponent<QuadTreeBranch>();
						branch.renderer.enabled = true;
						branch.UpdateQuadTree(target);
					}
				}
			} else if (targetDistance < GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude) {
				if (quadDepth == 0) renderer.enabled = true;
				if (quadDepth == planet.maxDepth) return;
				if (transform.childCount == 0) CreateChildQuads();
				divided = true;
				renderer.enabled = false;
				foreach (Transform child in transform) {
					QuadTreeBranch branch = child.GetComponent<QuadTreeBranch>();
					branch.renderer.enabled = true;
					branch.UpdateQuadTree(target);
				}
			}
		}

		void CreateChildQuads() {
			QuadTreeBranch parentBranch = GetComponentInParent<QuadTreeBranch>();
			Planet planet = GetComponentInParent<Planet>();
			Mesh[] meshes = MeshGenerator.SplitPlaneMeshInFour(planeMesh);
			LODGroup lodGroup = planet.GetComponent<LODGroup>();
			LOD[] lods = lodGroup.GetLODs();
			List<Renderer> quadRenderers = new List<Renderer>(lods[0].renderers);
			for (int i = 0; i < 4; i++) {
				GameObject submeshObject = new GameObject();
				submeshObject.name = name + "." + i;
				submeshObject.transform.SetParent(transform);
				submeshObject.transform.localPosition = Vector3.zero;
				submeshObject.transform.localEulerAngles = Vector3.zero;

				Bounds meshBounds = meshes[i].bounds;

				Mesh planeSubMesh = MeshGenerator.GenerateUnitQuadMesh();
				planeSubMesh = MeshGenerator.NormalizeAndAmplify(planeSubMesh, meshBounds.size.magnitude / 2);
				planeSubMesh = MeshGenerator.OffsetMesh(planeSubMesh, meshBounds.center);
				for (int subdiv = 0; subdiv < 5; subdiv++) { planeSubMesh = MeshGenerator.SubdivideGPU(planeSubMesh); }

				// Mesh planeSubMesh = meshes[i];
				// planeSubMesh = MeshGenerator.SubdivideGPU(planeSubMesh);



				QuadTreeBranch branch = submeshObject.AddComponent<QuadTreeBranch>();
				Rect zone = Rect.zero;
				if (i == 0) zone = new Rect(0, 0, 0.5f, 0.5f);
				else if (i == 1) zone = new Rect(0, 0.5f, 0.5f, 0.5f);
				else if (i == 2) zone = new Rect(0.5f, 0, 0.5f, 0.5f);
				else zone = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				branch.Init(quadDepth + 1, planeSubMesh, baseTexture, zone);
				quadRenderers.Add(branch.renderer);
			}
			lods[0].renderers = quadRenderers.ToArray();
			lodGroup.SetLODs(lods);
		}
	}
}