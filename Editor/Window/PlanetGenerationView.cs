using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class PlanetGenerationView : View {

    public override Views viewType { get { return Views.GENERATOR; } }

    bool autogenerating = false;
    bool settingsChanged = true;

    public override void ShowGUI() {
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Generate Planet") || (autogenerating && settingsChanged)) {
            PlanetEngineEditor.getSelectedPlanet();
        }
        GUILayout.Space(50);
        autogenerating = GUILayout.Toggle(autogenerating, "Auto Generate");
        GUILayout.EndHorizontal();
        
    }


}