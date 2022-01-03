using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {
	[ExecuteInEditMode]
	public class QuadTreeRoot : MonoBehaviour {
		Vector3 prevPosition = Vector3.zero;

		void Start() {
			for (int i = 0; i < transform.childCount; i++) {
				transform.GetChild(i).GetComponent<QuadTreeBranch>().PlaneMesh = CreateRootMesh(i);
			}
		}

		void Update() {
			Planet planet = GetComponentInParent<Planet>();
			planet.target = Camera.main.transform;
			if (!prevPosition.Equals(planet.target.position)) {
				prevPosition = planet.target.position;
				foreach (Transform child in transform) {
					child.GetComponent<QuadTreeBranch>().UpdateQuadTree(planet.target);
				}
			}
		}

		public void CreateQuadTree() {
			Planet planet = GetComponentInParent<Planet>();
			GameObject[] rootBranchObjects = new GameObject[6];
			QuadTreeBranch[] rootBranches = new QuadTreeBranch[6];
			for (int i = 0; i < 6; i++) {
				// Create branch object
				rootBranchObjects[i] = new GameObject();
				rootBranchObjects[i].name = planet.name + " - Branch: " + i;
				rootBranchObjects[i].transform.SetParent(transform);

				// Create mesh for current branch and set right orientation
				Mesh mesh = CreateRootMesh(i);
				Rect zone = Rect.zero;
				switch (i) {
					case 0:
						rootBranchObjects[0].transform.eulerAngles = new Vector3(0, 180, 0);
						zone = new Rect(1f / 4, 2f / 3, 1f / 4, 1f / 3);
						break;
					case 1:
						rootBranchObjects[1].transform.eulerAngles = new Vector3(180, 180, 0);
						zone = new Rect(1f / 4, 0, 1f / 4, 1f / 3);
						break;
					case 2:
						rootBranchObjects[2].transform.eulerAngles = new Vector3(-90, 180, 0);
						zone = new Rect(1f / 4, 1f / 3, 1f / 4, 1f / 3);
						break;
					case 3:
						rootBranchObjects[3].transform.eulerAngles = new Vector3(-90, 0, 0);
						zone = new Rect(3f / 4, 1f / 3, 1f / 4, 1f / 3);
						break;
					case 4:
						rootBranchObjects[4].transform.eulerAngles = new Vector3(-90, 0, -90);
						zone = new Rect(0, 1f / 3, 1f / 4, 1f / 3);
						break;
					case 5:
						rootBranchObjects[5].transform.eulerAngles = new Vector3(-90, 0, 90);
						zone = new Rect(2f / 4, 1f / 3, 1f / 4, 1f / 3);
						break;
				}

				rootBranchObjects[i].transform.localPosition = Vector3.zero;
				rootBranches[i] = rootBranchObjects[i].AddComponent<QuadTreeBranch>();
				rootBranches[i].Init(0, mesh, planet.data.baseTexture, zone);
			}
		}

		public Mesh CreateRootMesh(int surface) {
			Mesh mesh = null;
			switch (surface) {
				case 0:
					mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							// TopFace
							new Vector2(1f/4, 2f/3), // 1 x0y0
							new Vector2(1f/4, 1), // 2 x0y1
							new Vector2(2f/4, 1), // 3 x1y1
							new Vector2(2f/4, 2f/3), // 4 x1y0
						});
					break;
				case 1:
					mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							// BottomFace
							new Vector2(1f/4, 0), // 1 x0y0
							new Vector2(1f/4, 1f/3), // 2 x0y1
							new Vector2(2f/4, 1f/3), // 3 x1y1
							new Vector2(2f/4, 0), // 4 x1y0
						});
					break;
				case 2:
					mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							 // FrontFace
							new Vector2(1f/4, 1f/3), // 1 x0y0
							new Vector2(1f/4, 2f/3), // 2 x0y1
							new Vector2(2f/4, 2f/3), // 3 x1y1
							new Vector2(2f/4, 1f/3), // 4 x1y0
						});
					break;
				case 3:
					mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							 // BackFace
							new Vector2(3f/4, 1f/3), // 1 x0y0
							new Vector2(3f/4, 2f/3), // 2 x0y1
							new Vector2(1, 2f/3), // 3 x1y1
							new Vector2(1, 1f/3), // 4 x1y0
						});
					break;
				case 4:
					mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							// LeftFace
							new Vector2(0, 1f/3), // 1 x0y0
							new Vector2(0, 2f/3), // 2 x0y1
							new Vector2(1f/4, 2f/3), // 3 x1y1
							new Vector2(1f/4, 1f/3), // 4 x1y0
						});
					break;
				case 5:
					mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							// RightFace
							new Vector2(2f/4, 1f/3), // 1 x0y0
							new Vector2(2f/4, 2f/3), // 2 x0y1
							new Vector2(3f/4, 2f/3), // 3 x1y1
							new Vector2(3f/4, 1f/3), // 4 x1y0
						});
					break;
			}
			mesh = MeshGenerator.GenerateUnitQuadMesh();
			mesh = MeshGenerator.OffsetMesh(mesh, Vector3.up * 0.5f);
			for (int subdiv = 0; subdiv < 5; subdiv++) { mesh = MeshGenerator.SubdivideGPU(mesh); }
			return mesh;
		}
	}

}