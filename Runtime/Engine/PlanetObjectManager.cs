using UnityEngine;
using UnityEngine.SceneManagement;



internal class PlanetObjectManager : ScriptableObject {

    public GameObject activePlanet;

    public GameObject createNewPlanetObject(string planetName) {
        GameObject planet = new GameObject();
        planet.name = planetName;
        planet.AddComponent<PlanetRoot>();
        return planet;
    }
}
