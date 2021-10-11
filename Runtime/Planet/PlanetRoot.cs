using UnityEngine;

/**********************************************************************
 * 
 *                      Planet root
 *      This script is the base component of planet object.
 *      
 * 
 **********************************************************************/

public enum LODmodes {
    SINGLEMESH, // a single mesh presenting the whole planet for full view of planet (front and back)
    QUADTREE, // a quad tree based mesh system for large chunks of the planet (high detail, complex system)
    TERRAIN // a small part of the planet is generated on top of the unity terrain system for best performance.
};

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PlanetRoot : MonoBehaviour {

    LODmodes lodMode = LODmodes.SINGLEMESH;

    void Start() {

    }

    void Update() {

    }

    public void SetLodMode(LODmodes mode) { lodMode = mode; }
}
