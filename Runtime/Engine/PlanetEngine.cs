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

public sealed class PlanetEngine : ScriptableObject {

    // Singleton requirements
    private static PlanetEngine instance = null;
    private static readonly object padlock = new object();

    // Helper classes
    PlanetObjectManager planetObjectManager;

    // Object access
    private static PlanetEngine Instance {
        get {
            lock (padlock) {
                if (instance == null) {
                    instance = CreateInstance<PlanetEngine>();
                }
                return instance;
            }
        }
    }

    void Awake() {
        planetObjectManager = CreateInstance<PlanetObjectManager>();
    }

    public static GameObject CreatePlanet(string planetName) {
        return Instance.planetObjectManager.createNewPlanetObject(planetName);
    }
}