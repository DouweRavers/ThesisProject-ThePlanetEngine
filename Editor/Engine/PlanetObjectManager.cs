using System;
using UnityEngine;
using UnityEngine.SceneManagement;

internal class PlanetObjectManager : ScriptableObject {

    public GameObject activePlanet;

    public GameObject createNewPlanetObject(string planetName) {
        GameObject planet = new GameObject();
        planet.name = planetName;
        PlanetRoot planetRoot = planet.AddComponent<PlanetRoot>();
        planetRoot.Init(planetName);
        ChangeActivePlanet(planet);
        return planet;
    }

    internal void ChangeActivePlanet(GameObject planet) {
        if(activePlanet != null) DestroyImmediate(activePlanet);
        if (planet != null) {
            activePlanet = planet;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.MoveGameObjectToScene(planet, activeScene);
        }
        activePlanet = planet;
    }
}
