using UnityEngine;

internal class NoPlanetView : View {

    public override Views viewType { get { return Views.NOPLANET; } }


    public override void ShowGUI() {
        GUILayout.Label("No planet selected", Stylesheet.subtitleStyle);
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.ExpandWidth(true)); // spacer
        if (GUILayout.Button("Create new planet", GUILayout.Width(150))) {
            NewPlanetPopup.Popup();
        }
        GUILayout.Label("", GUILayout.ExpandWidth(true)); // spacer
        GUILayout.EndHorizontal();
    }
}
