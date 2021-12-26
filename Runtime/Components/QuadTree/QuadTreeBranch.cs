using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {

	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class QuadTreeBranch : MonoBehaviour {
		public new Renderer renderer { get { return GetComponent<MeshRenderer>(); } }
		public bool divided = false;
		QuadTreeBranch[] branches;
		Planet planet;
		Mesh planeMesh;
		int quadDepth = 0;
		// should be called after object has right parent
		public void Init(Planet planet, int quadDepth, Mesh planeMesh) {
			// assing values
			this.planet = planet;
			this.quadDepth = quadDepth;
			this.planeMesh = planeMesh;

			if (GetComponent<MeshFilter>().sharedMesh == null) {
				Mesh curvedMesh = Instantiate(planeMesh);
				curvedMesh = MeshGenerator.NormalizeAndAmplify(curvedMesh, 1);
				curvedMesh = MeshGenerator.SubdivideGPU(curvedMesh);
				curvedMesh = MeshGenerator.ApplyHeightmap(curvedMesh, planet.data.heightTexture);
				curvedMesh.RecalculateBounds();
				Vector3 localMeshCenter = curvedMesh.bounds.center;
				curvedMesh = MeshGenerator.OffsetMesh(curvedMesh, -localMeshCenter);
				transform.position = transform.TransformPoint(localMeshCenter) - transform.parent.position;
				curvedMesh.RecalculateBounds();
				curvedMesh.RecalculateNormals();
				curvedMesh.RecalculateTangents();
				curvedMesh.Optimize();
				GetComponent<MeshFilter>().mesh = curvedMesh;
				Material material = new Material(Shader.Find("Standard"));
				material.mainTexture = planet.data.terrainColorTexture;
				GetComponent<MeshRenderer>().material = material;
			}
		}

		public void UpdateQuadTree(Transform target) {
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
					foreach (QuadTreeBranch branch in branches) {
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
				foreach (QuadTreeBranch branch in branches) {
					branch.renderer.enabled = true;
					branch.UpdateQuadTree(target);
				}
			}
		}

		void CreateChildQuads() {
			Mesh[] meshes = MeshGenerator.SplitPlaneMeshInFour(planeMesh);
			branches = new QuadTreeBranch[4];
			LODGroup lodGroup = planet.GetComponent<LODGroup>();
			LOD[] lods = lodGroup.GetLODs();
			List<Renderer> quadRenderers = new List<Renderer>(lods[0].renderers);
			for (int i = 0; i < 4; i++) {
				GameObject submeshObject = new GameObject();
				submeshObject.name = name + "." + i;
				submeshObject.transform.SetParent(transform);
				submeshObject.transform.localPosition = Vector3.zero;
				submeshObject.transform.localEulerAngles = Vector3.zero;
				Mesh planeSubMesh = meshes[i];
				planeSubMesh = MeshGenerator.SubdivideGPU(planeSubMesh);
				branches[i] = submeshObject.AddComponent<QuadTreeBranch>();
				branches[i].Init(planet, quadDepth + 1, planeSubMesh);
				quadRenderers.Add(branches[i].renderer);
			}
			lods[0].renderers = quadRenderers.ToArray();
			lodGroup.SetLODs(lods);
		}
	}
}