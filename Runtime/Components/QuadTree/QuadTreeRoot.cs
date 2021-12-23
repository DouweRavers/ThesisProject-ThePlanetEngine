using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {
	[ExecuteInEditMode]
	public class QuadTreeRoot : MonoBehaviour {
		Planet planet;
		QuadTreeBranch[] rootBranches;
		Vector3 prevPosition = Vector3.zero;

		void Update() {
			planet.target = Camera.main.transform;
			if (!prevPosition.Equals(planet.target.position)) {
				prevPosition = planet.target.position;
				foreach (QuadTreeBranch branch in rootBranches) {
					branch.UpdateQuadTree(planet.target);
				}
			}
		}

		public void CreateQuadTree(Planet planet) {
			this.planet = planet;
			GameObject[] rootBranchObjects = new GameObject[6];
			rootBranches = new QuadTreeBranch[6];
			for (int i = 0; i < 6; i++) {
				// Create branch object
				rootBranchObjects[i] = new GameObject();
				rootBranchObjects[i].name = planet.name + " - Branch: " + i;
				rootBranchObjects[i].transform.SetParent(transform);

				// Create mesh for current branch and set right orientation
				Mesh mesh = null;
				switch (i) {
					case 0:
						mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							// TopFace
							new Vector2(1f/4, 2f/3), // 1 x0y0
							new Vector2(1f/4, 1), // 2 x0y1
							new Vector2(2f/4, 1), // 3 x1y1
							new Vector2(2f/4, 2f/3), // 4 x1y0
						});
						rootBranchObjects[0].transform.eulerAngles = new Vector3(0, 180, 0);
						break;
					case 1:
						mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							// BottomFace
							new Vector2(1f/4, 0), // 1 x0y0
							new Vector2(1f/4, 1f/3), // 2 x0y1
							new Vector2(2f/4, 1f/3), // 3 x1y1
							new Vector2(2f/4, 0), // 4 x1y0
						});
						rootBranchObjects[1].transform.eulerAngles = new Vector3(180, 0, 0);
						break;
					case 2:
						mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							 // FrontFace
							new Vector2(1f/4, 1f/3), // 1 x0y0
							new Vector2(1f/4, 2f/3), // 2 x0y1
							new Vector2(2f/4, 2f/3), // 3 x1y1
							new Vector2(2f/4, 1f/3), // 4 x1y0
						});
						rootBranchObjects[2].transform.eulerAngles = new Vector3(-90, 180, 0);
						break;
					case 3:
						mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							 // BackFace
							new Vector2(3f/4, 1f/3), // 1 x0y0
							new Vector2(3f/4, 2f/3), // 2 x0y1
							new Vector2(1, 2f/3), // 3 x1y1
							new Vector2(1, 1f/3), // 4 x1y0
						});
						rootBranchObjects[3].transform.eulerAngles = new Vector3(-90, 0, 0);
						break;
					case 4:
						mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							// LeftFace
							new Vector2(0, 1f/3), // 1 x0y0
							new Vector2(0, 2f/3), // 2 x0y1
							new Vector2(1f/4, 2f/3), // 3 x1y1
							new Vector2(1f/4, 1f/3), // 4 x1y0
						});
						rootBranchObjects[4].transform.eulerAngles = new Vector3(-90, 0, -90);
						break;
					case 5:
						mesh = MeshGenerator.GenerateUnitQuadMesh(new Vector2[] {
							// RightFace
							new Vector2(2f/4, 1f/3), // 1 x0y0
							new Vector2(2f/4, 2f/3), // 2 x0y1
							new Vector2(3f/4, 2f/3), // 3 x1y1
							new Vector2(3f/4, 1f/3), // 4 x1y0
						});
						rootBranchObjects[5].transform.eulerAngles = new Vector3(-90, 0, 90);
						break;
				}

				rootBranchObjects[i].transform.localPosition = Vector3.zero;
				mesh = MeshGenerator.OffsetMesh(mesh, Vector3.up * 0.5f);

				// Subdivide to right detail level
				for (int subdiv = 0; subdiv < 5; subdiv++) { mesh = MeshGenerator.SubdivideGPU(mesh); }
				rootBranches[i] = rootBranchObjects[i].AddComponent<QuadTreeBranch>();
				rootBranches[i].Init(planet, 0, mesh);
			}
		}
	}

}