using UnityEngine;
using UnityEditor;

namespace PlanetEngine
{

	[CustomEditor(typeof(Planet))]
	[CanEditMultipleObjects]
	public class PlanetEditor : Editor
	{
		Planet planet;
		public override void OnInspectorGUI()
		{
			GUILayout.Label("Interface comes here...");
		}

		void OnEnable()
		{
			planet = (target as Planet);
			planet.GetComponent<LODGroup>().hideFlags = HideFlags.HideInInspector;
			// Hide Components used by the Planet editor
			for (int i = 0; i < planet.transform.childCount; i++)
			{
				Transform child = planet.transform.GetChild(i);
				if (child.gameObject.tag.Equals("PlanetEngine"))
				{
					SceneVisibilityManager.instance.DisablePicking(child.gameObject, true);
					child.gameObject.hideFlags = HideFlags.HideInHierarchy;
				}
			}
			// Setup planet in editor
			if (planet.transform.childCount == 0) planet.CreatePlanet();
		}
	}
}