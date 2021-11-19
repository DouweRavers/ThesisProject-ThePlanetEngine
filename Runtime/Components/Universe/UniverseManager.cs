using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlanetEngine {
	[ExecuteInEditMode]
	public class UniverseManager : MonoBehaviour {
		public static UniverseManager universeManager;
		public float triggerRadius = 5f, drawRadius = 10f;
		public Vector3 offset { get { return universeOffset; } }
		Vector3 lastUpdatedPosition = Vector3.zero;
		Vector3 universeOffset = Vector3.zero;
		Transform target;
		List<UniverseTransform> universeTransforms;
		void Awake() {
			universeManager = this;
			universeTransforms = new List<UniverseTransform>();
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

		public void AddUniverseTransform(UniverseTransform universeTransform) {
			universeTransforms.Add(universeTransform);
		}

		public void RemoveUniverseTransform(UniverseTransform universeTransform) {
			universeTransforms.Remove(universeTransform);
		}


	}
}