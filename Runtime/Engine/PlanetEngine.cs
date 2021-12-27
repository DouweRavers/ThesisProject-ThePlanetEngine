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

namespace PlanetEngine
{

	public static class PlanetEngine
	{
		public static GameObject CreatePlanet(string planetName)
		{
			GameObject planet = new GameObject();
			planet.name = planetName;
			planet.tag = "PlanetEngine";
			planet.AddComponent<Planet>();
			return planet;
		}
	}
}