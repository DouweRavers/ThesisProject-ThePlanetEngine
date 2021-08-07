using UnityEngine;
using UnityEditor;

internal class PlanetManagerView : View {

    public override Views viewType { get { return Views.MANAGER; } }

    public override void ShowGUI() {
        GUILayout.Label("hello");
    }
}
