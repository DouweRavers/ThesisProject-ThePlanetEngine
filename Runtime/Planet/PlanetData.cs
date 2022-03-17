using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{

    public struct PlanetData
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
        const Gradient2D defaultOceanGradient_c = null;
        #endregion

        public PlanetData(
            int seed,
            float radius = defaultRadius_c,
            float continentscale = defaultContinentScale_c,
            bool ocean = defaultOcean_c,
            int maxDepth = defaultMaxDepth_c,
            int lodSphereCount = defaultLODlevels_c,
            Gradient2D oceangradient = defaultOceanGradient_c
            )
        {
            Seed = seed;
            Radius = radius;
            ContinentScale = continentscale;
            HasOcean = ocean;
            MaxDepth = maxDepth;
            LODSphereCount = lodSphereCount;
            OceanGradient = oceangradient;
        }

        public PlanetData Copy(bool deep = false)
        {
            PlanetData copy = new PlanetData(0);
            copy.Seed = Seed;
            copy.Radius = Radius;
            copy.ContinentScale = ContinentScale;
            copy.HasOcean = HasOcean;
            copy.MaxDepth = MaxDepth;
            copy.LODSphereCount = LODSphereCount;
            copy.OceanGradient = OceanGradient;
            return copy;
        }

        public bool Equals(PlanetData obj)
        {
            return obj.Seed == Seed &&
            obj.Radius == Radius &&
            obj.ContinentScale == ContinentScale &&
            obj.HasOcean == HasOcean &&
            obj.MaxDepth == MaxDepth &&
            obj.LODSphereCount == LODSphereCount &&
            obj.OceanGradient == OceanGradient;
        }

        public static bool operator ==(PlanetData a, PlanetData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(PlanetData a, PlanetData b) { return !a.Equals(b); }
    }
}