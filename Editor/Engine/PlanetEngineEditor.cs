using System;
using UnityEditor;
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
public sealed class PlanetEngineEditor : ScriptableObject {

    // Singleton requirements
    private static PlanetEngineEditor instance = null;
    private static readonly object padlock = new object();

    // helper classes
    private PEAssetManager assetManager;

    // Object access
    public static PlanetEngineEditor Instance {
        get {
            lock (padlock) {
                if (instance == null) {
                    instance = CreateInstance<PlanetEngineEditor>();
                }
                return instance;
            }
        }
    }

    void Awake() {
        assetManager = CreateInstance<PEAssetManager>();
    }

    public static void CreatePlanet(string planetName) {
        GameObject planet = PlanetEngine.CreatePlanet(planetName);
        Selection.activeTransform = planet.transform;
        //Selection.SetActiveObjectWithContext(planet, Selection.activeContext);
    }

    public static bool isSelectedObjectPlanet() {
        if (Selection.activeGameObject != null)
            return Selection.activeTransform.gameObject.GetComponent<PlanetRoot>() != null;
        else
            return false;
    }

    public static GameObject getSelectedPlanet() {
        if (isSelectedObjectPlanet())
            return Selection.activeTransform.gameObject;
        else
            return null;
    }

}