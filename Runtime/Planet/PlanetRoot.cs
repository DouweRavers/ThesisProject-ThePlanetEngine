using System;
using UnityEngine;

/**********************************************************************
 * 
 *                      Planet root
 *      This script is the base component of planet object.
 *      
 * 
 **********************************************************************/

[Serializable]
public class PlanetRoot : MonoBehaviour {
    public string planetName;

    public void Init(string planetName) {
        this.planetName = planetName;
    }

    void Start() {

    }

    void Update() {

    }
}
