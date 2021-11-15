using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlanetEngine
{

	public class UniverseManager : MonoBehaviour
	{
		public static UniverseManager universeManager;
		void Awake()
		{
			universeManager = this;
		}
	}
}