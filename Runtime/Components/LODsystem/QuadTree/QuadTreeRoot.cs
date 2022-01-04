using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {

	[ExecuteInEditMode]
	public class QuadTreeRoot : MonoBehaviour {
		void Update() {
			foreach (Transform child in transform) {
				child.GetComponent<QuadTreeBranch>().UpdateQuadTree();
			}
		}

		public void CreateQuadTree() {
			Planet planet = GetComponentInParent<Planet>();
			for (int i = 0; i < 6; i++) {
				GameObject rootBranchObject = new GameObject(planet.name + " - Branch: " + i);
				rootBranchObject.transform.SetParent(transform);
				rootBranchObject.transform.localPosition = Vector3.zero;
				Rect zone = Rect.zero;
				switch (i) {
					case 0:
						rootBranchObject.transform.eulerAngles = new Vector3(0, 180, 0);
						zone = new Rect(1f / 4, 2f / 3, 1f / 4, 1f / 3);
						break;
					case 1:
						rootBranchObject.transform.eulerAngles = new Vector3(180, 180, 0);
						zone = new Rect(1f / 4, 0, 1f / 4, 1f / 3);
						break;
					case 2:
						rootBranchObject.transform.eulerAngles = new Vector3(-90, 180, 0);
						zone = new Rect(1f / 4, 1f / 3, 1f / 4, 1f / 3);
						break;
					case 3:
						rootBranchObject.transform.eulerAngles = new Vector3(-90, 0, 0);
						zone = new Rect(3f / 4, 1f / 3, 1f / 4, 1f / 3);
						break;
					case 4:
						rootBranchObject.transform.eulerAngles = new Vector3(-90, 0, -90);
						zone = new Rect(0, 1f / 3, 1f / 4, 1f / 3);
						break;
					case 5:
						rootBranchObject.transform.eulerAngles = new Vector3(-90, 0, 90);
						zone = new Rect(2f / 4, 1f / 3, 1f / 4, 1f / 3);
						break;
				}
				rootBranchObject.AddComponent<QuadTreeBranch>().CreateBranch(planet, zone);
			}
		}
	}

}