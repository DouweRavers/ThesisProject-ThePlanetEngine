using System;
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

[Serializable]
public sealed class PlanetEngine : ScriptableObject {
    
    // Singleton requirements
    private static PlanetEngine instance = null;
    private static readonly object padlock = new object();

    // helper classes
    private PEAssetManager assetManager;
    private PlanetObjectManager planetObjectManager;

    // Object access
    public static PlanetEngine Instance {
        get {
            lock (padlock) {
                if (instance == null) {
                    instance = CreateInstance<PlanetEngine>();
                }
                return instance;
            }
        }
    }

    public static void CreatePlanet(string planetName) {
        GameObject planet = Instance.planetObjectManager.createNewPlanetObject(planetName);
        Instance.assetManager.SavePlanetObjectToFile(planet.GetComponent<PlanetRoot>());
    }

    public static GameObject GetActivePlanet() {
        return Instance.planetObjectManager.activePlanet;
    }

    public static string[] GetAllPlanetNames() {
        return Instance.assetManager.ScanSavedPlanets();
    }

    public static void SelectPlanet(string planetName) {
        string planet = Instance.assetManager.LoadPlanet(planetName);
        GameObject gameObject = new GameObject();
        gameObject.name = planetName;
        gameObject.AddComponent<PlanetRoot>();
        Instance.planetObjectManager.ChangeActivePlanet(gameObject);
    }
    
    public static void DeletePlanet(string planetName) {
        Instance.planetObjectManager.ChangeActivePlanet(null);
        Instance.assetManager.DeletePlanet(planetName);
    }

    void Awake() {
        assetManager = CreateInstance<PEAssetManager>();
        planetObjectManager = CreateInstance<PlanetObjectManager>();
    }

}