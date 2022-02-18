using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlanetEngine {
	[ExecuteInEditMode]
	public class UniverseManager : MonoBehaviour {
		public static UniverseManager universeManager = null;
		public float triggerRadius = 5f, drawRadius = 10f;
		public Vector3 offset { get { return universeOffset; } }
		Vector3 lastUpdatedPosition = Vector3.zero;
		Vector3 universeOffset = Vector3.zero;
		Transform target;
		List<UniverseTransform> universeTransforms;

		void Start() {
			universeManager = this;
			universeTransforms = new List<UniverseTransform>();
			UpdateTransformList();
		}

		void Update() {
			target = Camera.main.transform;
			if (triggerRadius > target.position.magnitude) return;
			Vector3 deltaOffset = target.position;
			universeOffset += target.position;
			target.position = Vector3.zero;
			foreach (UniverseTransform universeTransform in universeTransforms) {
				universeTransform.position -= deltaOffset;
			}
		}

		public void UpdateTransformList() {
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PlanetEngine");
			universeTransforms.Clear();
			foreach (GameObject planetEngineObjects in gameObjects) {
				if (planetEngineObjects.GetComponent<UniverseTransform>() != null)
					universeTransforms.Add(planetEngineObjects.GetComponent<UniverseTransform>());
			}
		}
	}
}