using UnityEditor;
using UnityEngine;

/**********************************************************************
 * 
 *                      The planet engine tool
 *      This class is the body of all other subtools. It will toggle between
 *      different subtools while providing a uniform style and layout.
 * 
 **********************************************************************/
namespace PlanetEngine {

	public class InterfaceManager : ScriptableObject {

		#region Singleton
		// This class only exist once in the editor.
		private static InterfaceManager instance = null;
		private static readonly object padlock = new object();
		public static InterfaceManager singleton {
			get {
				lock (padlock) {
					if (instance == null) {
						instance = CreateInstance<InterfaceManager>();
					}
					return instance;
				}
			}
		}
		#endregion

		#region Loaded textures
		// Textures get loaded once here so no unnesseary duplicates are created.
		public static Texture2D PlanetEnginelogo {
			get {
				if (PElogo == null) {
					PElogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.douwco.theplanetengine/Editor/Images/WhiteLogo.png", typeof(Texture2D));
				}
				return PElogo;
			}
		}
		public static Texture2D Douwcologo {
			get {
				if (Dlogo == null) {
					Dlogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.douwco.theplanetengine/Editor/Images/DouwcoLogo.png", typeof(Texture2D));
				}
				return Dlogo;
			}
		}
		static Texture2D PElogo, Dlogo;
		#endregion

		#region Menu Access
		// All windows and functions are accessed by these static functions
		[MenuItem("GameObject/3D Object/Planet", false, 40)]
		public static void createPlanet() {
			PlanetEngineEditor.CreatePlanet("Planet");
		}

		[MenuItem("Window/Planet Engine/Universe Manager")]
		public static void ShowWindow() {
			PlanetManagerWindow wnd = EditorWindow.GetWindow<PlanetManagerWindow>();
			wnd.titleContent = new GUIContent("Planet manager");
		}
		#endregion
	}
}