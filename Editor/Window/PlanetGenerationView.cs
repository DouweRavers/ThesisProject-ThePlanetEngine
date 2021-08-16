using System.Collections;
using System.Collections.Generic;
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
            PlanetEngineEditor.getSelectedPlanet().GetComponent<PlanetMeshGenerator>().MakeQuad();
        }
        if (GUILayout.Button("Generate Cube")) {
            PlanetEngineEditor.getSelectedPlanet().GetComponent<PlanetMeshGenerator>().MakeCube();
        }

        GUILayout.Label("Modify mesh");
        if (GUILayout.Button("Divide")) {
            PlanetEngineEditor.getSelectedPlanet().GetComponent<PlanetMeshGenerator>().Divide();
        }
        if (GUILayout.Button("Blow up (like Balloon)")) {
            PlanetEngineEditor.getSelectedPlanet().GetComponent<PlanetMeshGenerator>().BlowUp();
        }



    }


}