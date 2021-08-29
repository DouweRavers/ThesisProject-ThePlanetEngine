using UnityEngine;

internal class PlanetGenerationView : View {

    public override Views viewType { get { return Views.GENERATOR; } }

    bool autogenerating = false;
    bool settingsChanged = true;

    public override void ShowGUI() {
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
        if (GUILayout.Button("Generate Quad")) {
            PlanetEngine.MakeQuad(PlanetEngineEditor.getSelectedPlanet());
        }
        if (GUILayout.Button("Generate Cube")) {
            PlanetEngine.MakeCube(PlanetEngineEditor.getSelectedPlanet());
        }

        GUILayout.Label("Modify mesh");
        if (GUILayout.Button("Divide")) {
            PlanetEngine.Divide(PlanetEngineEditor.getSelectedPlanet());
        }
        if (GUILayout.Button("Blow up (like Balloon)")) {
            PlanetEngine.BlowUp(PlanetEngineEditor.getSelectedPlanet());
        }
    }


}