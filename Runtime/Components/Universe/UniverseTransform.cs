using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {
	[ExecuteInEditMode]
	public class UniverseTransform : MonoBehaviour {

		public Vector3 position {
			get { return transform.position + UniverseManager.universeManager.offset; }
			set { transform.position = value - UniverseManager.universeManager.offset; }
		}

		void Start() {
			UniverseManager.universeManager.AddUniverseTransform(this);
		}
		void OnDestroy() {
			UniverseManager.universeManager.RemoveUniverseTransform(this);
		}

	}






}

