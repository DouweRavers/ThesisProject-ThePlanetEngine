using UnityEngine;

namespace PlanetEngine
{
   
    /// <summary>
    /// This abstract class defines the basics of a planet. Planets with different rendering settings can
    /// be created from this base planet. Where only the planet data is stored.
    /// </summary>

    public abstract class BasePlanet : MonoBehaviour
    {
        /// <summary>
        /// Holds all data conserning the planet generation process.
        /// </summary>
        public PlanetData Data
        {
            get
            {
                if (_data == null)
                {
                    _data = ScriptableObject.CreateInstance<PlanetData>();
                    try
                    {
                        _data.LoadData(name);
                    }
                    catch (System.Exception)
                    {
                        Debug.LogWarning("No data file exists for this planet, new one is created.");
                    }
                }
                return _data;
            }
            set
            {
                _data = value;
                _data.SaveData(name);
            }

        }
        PlanetData _data;

    }
}