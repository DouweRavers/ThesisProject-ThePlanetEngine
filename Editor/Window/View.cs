using UnityEngine;

internal enum Views { NOPLANET, GENERATOR, EDITOR };

internal abstract class View : ScriptableObject {
    public abstract Views viewType { get; }
    public abstract void ShowGUI();

    public static View GetViewOfType(Views type) {
        View view;
        switch (type) {
            case Views.NOPLANET:
                view = CreateInstance<NoPlanetView>();
                break;
            case Views.GENERATOR:
                view = CreateInstance<PlanetGenerationView>();
                break;
            case Views.EDITOR:
                view = null;
                break;
            default:
                view = CreateInstance<NoPlanetView>();
                break;
        }
        return view;
    }
}
