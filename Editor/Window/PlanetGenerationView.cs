using UnityEngine;

internal class PlanetGenerationView : View
{

    public override Views viewType { get { return Views.GENERATOR; } }

    bool autogenerating = false;
    bool settingsChanged = true;
    float size = 5f;
    public override void ShowGUI()
    {
        /*
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Generate Planet") || (autogenerating && settingsChanged)) {
            PlanetEngineEditor.getSelectedPlanet().GetComponent<PlanetRoot>().UpdateMesh();
        }
        GUILayout.Space(50);
        autogenerating = GUILayout.Toggle(autogenerating, "Auto Generate");
        GUILayout.EndHorizontal();
        */
        GUILayout.Label("Mesh primitives");
        if (GUILayout.Button("Generate Quad"))
        {
            PlanetEngine.MakeQuad(PlanetEngineEditor.getSelectedPlanet());
        }
        if (GUILayout.Button("Generate Cube"))
        {
            PlanetEngine.MakeCube(PlanetEngineEditor.getSelectedPlanet());
        }
        if (GUILayout.Button("Generate Sphere"))
        {
            PlanetEngine.MakeSphere(PlanetEngineEditor.getSelectedPlanet());
        }

        GUILayout.Label("Modify mesh");
        if (GUILayout.Button("Divide using CPU"))
        {
            PlanetEngine.Divide(PlanetEngineEditor.getSelectedPlanet(), true);
        }
        if (GUILayout.Button("Divide using GPU"))
        {
            PlanetEngine.Divide(PlanetEngineEditor.getSelectedPlanet());
        }
        if (GUILayout.Button("Normalize and Amplify"))
        {
            PlanetEngine.NormalizeAndAmplify(PlanetEngineEditor.getSelectedPlanet(), size);
        }
        size = GUILayout.HorizontalSlider(size, 0.1f, 10f, GUILayout.Height(25));
    }


}