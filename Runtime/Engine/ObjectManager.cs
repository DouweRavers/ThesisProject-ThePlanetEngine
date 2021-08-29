using UnityEngine;



internal class ObjectManager : ScriptableObject {

    public GameObject activePlanet;

    public GameObject createNewPlanetObject(string planetName) {
        GameObject planet = new GameObject();
        planet.name = planetName;
        planet.AddComponent<PlanetRoot>();
        return planet;
    }
}
