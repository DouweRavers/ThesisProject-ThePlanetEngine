using UnityEngine;
using UnityEditor;

internal enum Views { MANAGER, GENERATOR, EDITOR };

internal abstract class View : ScriptableObject {
    public abstract Views viewType{ get; }
    public abstract void ShowGUI();
}
