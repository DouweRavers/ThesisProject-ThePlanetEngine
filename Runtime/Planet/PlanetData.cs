using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    public class PlanetData : ScriptableObject
    {
        #region Procedural Properties
        public int Seed;
        #endregion

        #region Celestial Properties
        public float Radius;
        public float ContinentScale;
        public bool HasOcean;
        #endregion

        #region Rendering Properties
        public int MaxDepth;
        public int LODSphereCount;
        public Gradient2D OceanGradient;
        #endregion


        #region Default Values
        const float defaultRadius_c = 10f;
        const float defaultContinentScale_c = 0.5f;
        const bool defaultOcean_c = true;
        const int defaultMaxDepth_c = 12;
        const int defaultLODlevels_c = 3;
        #endregion

        public void Init(
            int seed,
            float radius = defaultRadius_c,
            float continentscale = defaultContinentScale_c,
            bool ocean = defaultOcean_c,
            int maxDepth = defaultMaxDepth_c,
            int lodSphereCount = defaultLODlevels_c
            )
        {
            Seed = seed;
            Radius = radius;
            ContinentScale = continentscale;
            HasOcean = ocean;
            MaxDepth = maxDepth;
            LODSphereCount = lodSphereCount;
            OceanGradient = CreateInstance<Gradient2D>();
        }

        public PlanetData Copy(bool deep = false)
        {
            PlanetData copy = CreateInstance<PlanetData>();
            copy.Seed = Seed;
            copy.Radius = Radius;
            copy.ContinentScale = ContinentScale;
            copy.HasOcean = HasOcean;
            copy.MaxDepth = MaxDepth;
            copy.LODSphereCount = LODSphereCount;
            copy.OceanGradient = OceanGradient;
            return copy;
        }
    }
}