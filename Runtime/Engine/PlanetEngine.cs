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

    public Material defaultMaterial;

    // Singleton requirements
    private static PlanetEngine instance = null;
    private static readonly object padlock = new object();

    // Helper classes
    ObjectManager objectManager;
    MeshGenerator meshGenerator;
    TextureGenerator textureGenerator;

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
        objectManager = CreateInstance<ObjectManager>();
        meshGenerator = CreateInstance<MeshGenerator>();
        textureGenerator = CreateInstance<TextureGenerator>();
    }

    public static GameObject CreatePlanet(string planetName) {
        return Instance.objectManager.createNewPlanetObject(planetName);
    }

    public static void MakeQuad(GameObject planetObject) {
        planetObject.GetComponent<MeshFilter>().mesh = Instance.meshGenerator.GenerateUnitQuadMesh();
        Material material = new Material(Instance.defaultMaterial);
        Texture2D baseTexture = Instance.textureGenerator.GenerateBaseTexture(planetObject.GetComponent<MeshFilter>().sharedMesh, new Rect(0, 0, 1024, 1024));
        Texture2D heightMapTexture = Instance.textureGenerator.CreateHeightmap(baseTexture);
        material.mainTexture = heightMapTexture;

        planetObject.GetComponent<MeshRenderer>().material = material;
    }

    public static void MakeCube(GameObject planetObject) {
        planetObject.GetComponent<MeshFilter>().mesh = Instance.meshGenerator.GenerateUnitCubeMesh();
        Material material = new Material(Instance.defaultMaterial);
        material.mainTexture = Instance.textureGenerator.GenerateBaseTexture(planetObject.GetComponent<MeshFilter>().sharedMesh, new Rect(0, 0, 1024, 1024));
        planetObject.GetComponent<MeshRenderer>().material = material;
    }

    public static void Divide(GameObject planetObject) {
        Mesh mesh = planetObject.GetComponent<MeshFilter>().sharedMesh;
        mesh = Instance.meshGenerator.SubdivideQuadMesh(mesh, 1);
        planetObject.GetComponent<MeshFilter>().mesh = mesh;
    }

    public static void BlowUp(GameObject planetObject) {
        Mesh mesh = planetObject.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < mesh.vertexCount; i++) {
            vertices[i] = mesh.vertices[i].normalized;
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        planetObject.GetComponent<MeshFilter>().mesh = mesh;
    }
}