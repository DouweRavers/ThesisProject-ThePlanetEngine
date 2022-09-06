using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// This component represents a small piece of planet displayed as a terrain.
    /// </summary>
    [RequireComponent(typeof(Terrain))]
    [RequireComponent(typeof(TerrainCollider))]
    internal class PlanetTerrain : MonoBehaviour
    {
        /// <summary>
        /// Data used by the unity terrain system.
        /// </summary>
        public TerrainData TerrainData
        {
            get
            {
                return GetComponent<Terrain>().terrainData;
            }
            set
            {
                GetComponent<Terrain>().terrainData = value;
                GetComponent<TerrainCollider>().terrainData = value;
            }
        }

        /// <summary>
        /// closest point on planet sphere in local space
        /// </summary>
        Vector3 _closestSurfacePoint;

        /// <summary>
        /// Reference to the planet.
        /// </summary>
        Planet _planet;

        private void Start()
        {
            enabled = false;
        }

        private void Update()
        {
            _planet = GetComponentInParent<Planet>();
            float terrainHeight = TerrainData.size.y;
            float specificTerrainHeight = GetComponent<Terrain>().SampleHeight(_planet.Target.position);
            float heightAboveTerrain = (_planet.Target.position.y - transform.position.y) - specificTerrainHeight;

            if (terrainHeight * 1.5 < heightAboveTerrain)
            {
                _planet.SwitchToTree();
            }
        }

        /// <summary>
        /// Generates a terrain using the planet data.
        /// </summary>
        public void CreateTerrain()
        {
            _planet = GetComponentInParent<Planet>();
            _closestSurfacePoint = _planet.transform.InverseTransformPoint(_planet.GetClosestPointToSurface(_planet.Target.position));
            GenerateTerrainData();
            PositionTerrain();
            GetComponent<Terrain>().materialTemplate = new Material(Shader.Find("Nature/Terrain/Standard"));
            GetComponent<Terrain>().treeCrossFadeLength = 50;
            enabled = true;
        }

        /// <summary>
        /// Clears the planet data.
        /// </summary>
        public void DestroyTerrain()
        {
            enabled = false;
            TerrainData = null;
        }

        /// <summary>
        /// Places the terrain at the right position.
        /// </summary>
        private void PositionTerrain()
        {
            transform.position = _planet.transform.TransformPoint(_closestSurfacePoint - TerrainData.size / 2);
            // TODO: Offer an way to rotate the player so the change in terrain isn't noticed.
        }

        /// <summary>
        /// Generates the terrain data from the planet data.
        /// </summary>
        private void GenerateTerrainData()
        {
            int resolution = 513;
            Vector3[] cornerPoints = GetCornerPoints(true);
            Texture2D baseValues = ProceduralTexture.GetBaseTexture(resolution, resolution, cornerPoints);
            float[,] heightValues = ProceduralTerrainData.GenerateHeightValues(baseValues, _planet.Data);
            TerrainLayer[] layers = ProceduralTerrainData.GenerateTerrainLayersRGB();
            float[,,] alphaValues = ProceduralTerrainData.GenerateAlphaValuesRGB(baseValues, _planet.Data);
            TreePrototype[] treePrototypes = ProceduralTerrainData.GenerateTreePrototypes(_planet.Data);
            DetailPrototype[] detailPrototypes = ProceduralTerrainData.GenerateDetailPrototypes(_planet.Data);
            TreeInstance[] treeInstances = ProceduralTerrainData.GenerateTreeInstances(resolution / 10, _planet.Data);
            int[,] detailInstances = ProceduralTerrainData.GenerateDetailInstances(resolution, _planet.Data);

            TerrainData = new TerrainData();
            TerrainData.heightmapResolution = resolution;
            TerrainData.alphamapResolution = resolution;
            TerrainData.SetHeights(0, 0, heightValues);
            TerrainData.terrainLayers = layers;
            TerrainData.SetAlphamaps(0, 0, alphaValues);
            TerrainData.size = GetSizeFromCorners(cornerPoints);
            if (treePrototypes != null) TerrainData.treePrototypes = treePrototypes;
            if (detailPrototypes != null) TerrainData.detailPrototypes = detailPrototypes;
            TerrainData.RefreshPrototypes();
            if (treeInstances != null) TerrainData.SetTreeInstances(treeInstances, true);
            if (detailInstances != null)
            {
                TerrainData.SetDetailResolution(resolution, 1);
                for (int i = 0; i < _planet.Data.FoliageTypes.Length; i++)
                {
                    TerrainData.SetDetailLayer(0, 0, i, detailInstances);
                }
            }
        }

        /// <summary>
        /// Generates 4 points that form a square on the surface on the surface (not heightened). 
        /// </summary>
        /// <param name="debugLines">for debugging a the points can be splayed in respect to the center of the planet.</param>
        private Vector3[] GetCornerPoints(bool debugLines = false)
        {
            Vector3 shiftedVector = _closestSurfacePoint + Vector3.one;
            Vector3[] cornerPoints = new Vector3[4];
            // Top left
            cornerPoints[0] = Vector3.Cross(_closestSurfacePoint, shiftedVector);
            // Top right
            cornerPoints[1] = Vector3.Cross(_closestSurfacePoint, cornerPoints[0]);
            // Bottom right
            cornerPoints[2] = Vector3.Cross(shiftedVector, _closestSurfacePoint);
            // Bottom left
            cornerPoints[3] = Vector3.Cross(_closestSurfacePoint, cornerPoints[2]);
            // Use orthogonal vectors to get slight angled ones as corners.
            for (int i = 0; i < cornerPoints.Length; i++)
            {
                cornerPoints[i] = Vector3.Slerp(_closestSurfacePoint.normalized, cornerPoints[i].normalized, 0.1f);
            }
            if (debugLines)
            {
                Debug.DrawLine(_planet.transform.position, _planet.transform.TransformPoint(cornerPoints[0] * _planet.Data.Radius), Color.red, Mathf.Infinity);
                Debug.DrawLine(_planet.transform.position, _planet.transform.TransformPoint(cornerPoints[1] * _planet.Data.Radius), Color.red, Mathf.Infinity);
                Debug.DrawLine(_planet.transform.position, _planet.transform.TransformPoint(cornerPoints[2] * _planet.Data.Radius), Color.red, Mathf.Infinity);
                Debug.DrawLine(_planet.transform.position, _planet.transform.TransformPoint(cornerPoints[3] * _planet.Data.Radius), Color.red, Mathf.Infinity);
                Debug.DrawLine(_planet.transform.position, _planet.transform.TransformPoint(_closestSurfacePoint), Color.blue, Mathf.Infinity);
            }
            return cornerPoints;
        }

        /// <summary>
        /// Based on the corner points the size of the square on the surface is measured.
        /// </summary>
        /// <returns>A vector with the (width, height, width/3=depth)</returns>
        private Vector3 GetSizeFromCorners(Vector3[] cornerPoints)
        {
            Vector3 size = Vector3.zero;
            size.x = _planet.Data.Radius * (cornerPoints[1] - cornerPoints[0]).magnitude;
            size.z = _planet.Data.Radius * (cornerPoints[3] - cornerPoints[0]).magnitude;
            size.y = size.x / 3f;
            return size;
        }
    }
}