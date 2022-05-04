using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    [RequireComponent(typeof(Terrain))]
    [RequireComponent(typeof(TerrainCollider))]
    internal class PlanetTerrain : MonoBehaviour
    {
        // TEMP: this should be part of the creator menu;
        [SerializeField]
        GameObject tree;
        [SerializeField]
        Texture2D grass;
        internal TerrainData Data
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

        // closest point on planet sphere in local space
        Vector3 _closestSurfacePoint;
        // Reference to the planet.
        Planet _planet;
        private void Start()
        {
            enabled = false;
        }

        private void Update()
        {
            _planet = GetComponentInParent<Planet>();
            float terrainHeight = Data.size.y;
            float specificTerrainHeight = GetComponent<Terrain>().SampleHeight(_planet.Target.position);
            float heightAboveTerrain = (_planet.Target.position.y - transform.position.y) - specificTerrainHeight;

            if (terrainHeight * 3 < heightAboveTerrain)
            {
                _planet.SwitchToTree();
            }
        }

        internal void CreateTerrain()
        {
            _planet = GetComponentInParent<Planet>();
            _closestSurfacePoint = _planet.transform.InverseTransformPoint(_planet.GetClosestPointToSurface(_planet.Target.position));
            GenerateTerrainData();
            PositionTerrain();
            GetComponent<Terrain>().materialTemplate = new Material(Shader.Find("Nature/Terrain/Standard"));
            enabled = true;
        }

        internal void DestroyTerrain()
        {
            enabled = false;
            Data = null;
        }


        void PositionTerrain()
        {
            transform.position = _planet.transform.TransformPoint(_closestSurfacePoint - Data.size / 2);
            // TODO: Offer an way to rotate the player so the change in terrain isn't noticed.
        }

        void GenerateTerrainData()
        {
            int resolution = 513;
            Vector3[] cornerPoints = GetCornerPoints(true);
            Texture2D baseValues = ProceduralTexture.GetBaseTexture(resolution, resolution, cornerPoints);
            float[,] heightValues = ProceduralTerrainData.GenerateHeightValues(baseValues, _planet.Data);
            TerrainLayer[] layers = ProceduralTerrainData.GenerateTerrainLayers(_planet.Data);
            float[,,] alphaValues = ProceduralTerrainData.GenerateAlphaValues(baseValues, _planet.Data);
            TreePrototype[] treePrototypes = ProceduralTerrainData.GenerateTreePrototypes();
            DetailPrototype[] detailPrototypes = ProceduralTerrainData.GenerateDetailPrototypes();
            TreeInstance[] treeInstances = ProceduralTerrainData.GenerateTreeInstances(resolution/10);
            int[,] detailInstances = ProceduralTerrainData.GenerateDetailInstances(resolution);

            Data = new TerrainData();
            Data.heightmapResolution = resolution;
            Data.alphamapResolution = resolution;
            Data.SetHeights(0, 0, heightValues);
            Data.terrainLayers = layers;
            Data.SetAlphamaps(0, 0, alphaValues);
            Data.size = GetSizeFromCorners(cornerPoints);
            Data.treePrototypes = treePrototypes;
            Data.detailPrototypes = detailPrototypes;
            Data.RefreshPrototypes();
            Data.SetTreeInstances(treeInstances, true);
            Data.SetDetailResolution(resolution, 1);
            Data.SetDetailLayer(0, 0, 0, detailInstances);
        }

        Vector3[] GetCornerPoints(bool debugLines = false)
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

        Vector3 GetSizeFromCorners(Vector3[] cornerPoints)
        {
            Vector3 size = Vector3.zero;
            size.x = _planet.Data.Radius * (cornerPoints[1] - cornerPoints[0]).magnitude;
            size.z = _planet.Data.Radius * (cornerPoints[3] - cornerPoints[0]).magnitude;
            size.y = size.x / 3f;
            return size;
        }
    }
}