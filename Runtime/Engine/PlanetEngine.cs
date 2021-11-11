using UnityEngine;

/**********************************************************************
 * 
 *                      The planet engine
 *      This singleton is the root of the application. It will manage the entire
 *      PE system. From processing the editor window inputs, to handling the API calls.
 *      As a manager class it mostly just routes one part of the system to an other part.
 *      
 * 
 **********************************************************************/

namespace PlanetEngine {

	public class PlanetEngine : ScriptableObject {
		public static GameObject CreatePlanet(string planetName) {
			UniverseManager universeManager = null;
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("PlanetEngine");
			foreach (GameObject planetEngineObjects in gameObjects) {
				if (universeManager != null) break;
				universeManager = (UniverseManager)planetEngineObjects.GetComponent(typeof(UniverseManager));
			}
			if (universeManager == null) {
				GameObject universeObject = new GameObject();
				universeObject.name = "UniverseManager";
				universeObject.tag = "PlanetEngine";
				universeObject.AddComponent<UniverseManager>();
				universeObject.hideFlags = HideFlags.HideInHierarchy;
			}
			GameObject planet = new GameObject();
			planet.name = planetName;
			planet.tag = "PlanetEngine";
			planet.AddComponent<Planet>();
			return planet;
		}
	}
}