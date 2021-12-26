using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {
	/*internal*/
	public static class TextureTool {
		public static ComputeShader textureShader;
		static bool initialized = false;
		static void Initialize() {
			if (initialized) return;
			initialized = true;
		}

		// Generates a base texture on which all textures base. This method will take a mesh with UV coordinates and generate a
		// Texture that maps all (normalized) vector values as RGB values by interpolating between two given vertices.
		// The A value is the magnitude of the vector
		public static Texture2D GenerateBaseTexture(Mesh mesh, Rect size) {
			Initialize();
			Texture2D baseTexture = new Texture2D(Mathf.RoundToInt(size.width), Mathf.RoundToInt(size.height), TextureFormat.RGBAFloat, false);
			baseTexture.filterMode = FilterMode.Point;
			for (int x = 0; x < baseTexture.width; x++) {
				for (int y = 0; y < baseTexture.height; y++) {
					baseTexture.SetPixel(x, y, new Color(1, 1, 1, -1));
				}
			}
			for (int i = 0; i < mesh.triangles.Length; i += 6) {
				int A = mesh.triangles[i]; // x0y0
				int B = mesh.triangles[i + 1]; // x0y1
				int C = mesh.triangles[i + 2]; // x1y1
				int D = mesh.triangles[i + 5]; // x1y0

				Vector2 origin = mesh.uv[A];
				origin.x *= baseTexture.width;
				origin.y *= baseTexture.height;

				Vector2 end = mesh.uv[C];
				end.x *= baseTexture.width;
				end.y *= baseTexture.height;

				Vector3 originVertex = mesh.vertices[A];
				Vector3 verticeX = mesh.vertices[D] - mesh.vertices[A];
				Vector3 verticeY = mesh.vertices[B] - mesh.vertices[A];

				for (int y = Mathf.FloorToInt(origin.y); y < Mathf.CeilToInt(end.y); y++) {
					Vector3 projectedVerticeY = verticeY * (y - origin.y) / (baseTexture.height / 3);
					for (int x = Mathf.FloorToInt(origin.x); x < Mathf.CeilToInt(end.x); x++) {
						Vector3 projectedVerticeX = verticeX * (x - origin.x) / (baseTexture.width / 4);
						Vector3 projectedVertex = originVertex + projectedVerticeX + projectedVerticeY;
						baseTexture.SetPixel(x, y,
						new Color(projectedVertex.normalized.x, projectedVertex.normalized.y, projectedVertex.normalized.z, projectedVertex.magnitude));
					}
				}
			}
			baseTexture.Apply();
			return baseTexture;
		}

		public static Texture2D GenerateBiomeTexture(Texture2D baseTexture, Texture2D heigtMap) {
			Initialize();
			System.Random random = new System.Random();
			Vector3[] points = new Vector3[baseTexture.width];
			Biomes[] biomes = new Biomes[points.Length];
			for (int i = 0; i < points.Length; i++) {
				int randomX = random.Next() % baseTexture.width;
				int randomY = random.Next() % baseTexture.height;
				Color pointColor = baseTexture.GetPixel(randomX, randomY);
				if (pointColor.a < 0) continue;
				points[i] = new Vector3(pointColor.r, pointColor.g, pointColor.b);
				float heigt = heigtMap.GetPixel(randomX, randomY).r;
				if (points[i].y > 0.9f || points[i].y < -0.9f) biomes[i] = Biomes.SNOW;
				else {
					if (points[i].y < 0.3f && points[i].y > -0.3f) biomes[i] = Biomes.DESERT;
					else biomes[i] = Biomes.GRASS;
				}
			}
			// r closest, g second closest, b proportion = (SCdist - Cdist) / SCdist, a empty 
			Texture2D vonoroiTexture = new Texture2D(baseTexture.width, baseTexture.height);
			vonoroiTexture.filterMode = FilterMode.Point;
			for (int x = 0; x < baseTexture.width; x++) {
				for (int y = 0; y < baseTexture.height; y++) {
					Color positionColor = baseTexture.GetPixel(x, y);
					if (positionColor.a < 0) continue;
					float minimumDistance = baseTexture.width + baseTexture.height;
					float secondMinimumDistance = baseTexture.width + baseTexture.height;
					Biomes closest = Biomes.OCEAN, secondClosest = Biomes.OCEAN;
					if (heigtMap.GetPixel(x, y).r > 0.5f) {
						for (int i = 0; i < points.Length; i++) {
							Vector3 vertex = new Vector3(positionColor.r, positionColor.g, positionColor.b);
							float dist = Vector3.Distance(vertex, points[i].normalized);
							if (dist < minimumDistance) {
								secondMinimumDistance = minimumDistance;
								secondClosest = closest;
								minimumDistance = dist;
								closest = biomes[i];
							} else if (dist < secondMinimumDistance) {
								secondMinimumDistance = dist;
								secondClosest = biomes[i];
							}
						}
					}
					vonoroiTexture.SetPixel(x, y, new Color((float)closest / 4f, (float)secondClosest / 4f, (secondMinimumDistance - minimumDistance) / secondMinimumDistance, 0));
				}
			}
			vonoroiTexture.Apply();
			return vonoroiTexture;
		}

		public static Texture2D GenerateHeightTexture(Texture2D baseTexture) {
			Initialize();
			Texture2D heigtTexture = new Texture2D(baseTexture.width, baseTexture.height);
			heigtTexture.filterMode = FilterMode.Point;
			Perlin.Seed = Mathf.RoundToInt(Time.timeSinceLevelLoad);
			Perlin.Scale = 2.5f;
			for (int x = 0; x < baseTexture.width; x++) {
				for (int y = 0; y < baseTexture.height; y++) {
					Color vertexColor = baseTexture.GetPixel(x, y);
					if (vertexColor.a < 0) heigtTexture.SetPixel(x, y, vertexColor);
					Vector3 vertex = new Vector3(vertexColor.r, vertexColor.g, vertexColor.b);
					float noise = Perlin.PerlinNoise(vertex);
					Color color = Color.white * noise;
					heigtTexture.SetPixel(x, y, color);
				}
			}
			heigtTexture.Apply();
			return heigtTexture;
		}
		public static Texture2D GenerateTerrainColorTexture(Texture2D biomeTexture, Texture2D heigtMap) {
			Initialize();
			Texture2D terrainTexture = new Texture2D(biomeTexture.width, biomeTexture.height);
			terrainTexture.filterMode = FilterMode.Point;
			for (int x = 0; x < biomeTexture.width; x++) {
				for (int y = 0; y < biomeTexture.height; y++) {
					float height = heigtMap.GetPixel(x, y).r;
					Color biomeData = biomeTexture.GetPixel(x, y);
					Biomes closestBiome = (Biomes)Mathf.RoundToInt(biomeData.r * 4);
					Biomes secondClosestBiome = (Biomes)Mathf.RoundToInt(biomeData.g * 4);
					float proportion = biomeData.b;
					Color closestBiomeColor = Color.black, secondClosestBiomeColor = Color.black;
					Gradient gradient = new Gradient();
					GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
					alphaKey[0].alpha = 1.0f;
					alphaKey[0].time = 0.0f;
					alphaKey[1].alpha = 1.0f;
					alphaKey[1].time = 1.0f;
					GradientColorKey[] colorKey = new GradientColorKey[2];
					colorKey[0].color = Color.white;
					colorKey[0].time = 0.0f;
					colorKey[1].color = Color.white;
					colorKey[1].time = 1.0f;
					switch (closestBiome) {
						case Biomes.SNOW:
							colorKey[0].color = Color.white;
							colorKey[1].color = Color.white * 0.3f;
							gradient.SetKeys(colorKey, alphaKey);
							closestBiomeColor = gradient.Evaluate(height);
							break;
						case Biomes.GRASS:
							colorKey[0].color = Color.green;
							colorKey[1].color = Color.green * 0.3f;
							gradient.SetKeys(colorKey, alphaKey);
							closestBiomeColor = gradient.Evaluate(height);
							break;
						case Biomes.DESERT:
							colorKey[0].color = Color.yellow;
							colorKey[1].color = Color.yellow * 0.3f;
							gradient.SetKeys(colorKey, alphaKey);
							closestBiomeColor = gradient.Evaluate(height);
							break;
						case Biomes.OCEAN:
							colorKey[0].color = Color.blue;
							colorKey[1].color = Color.blue * 0.3f;
							gradient.SetKeys(colorKey, alphaKey);
							closestBiomeColor = gradient.Evaluate(height);
							break;

					}
					switch (secondClosestBiome) {
						case Biomes.SNOW:
							colorKey[0].color = Color.white;
							colorKey[1].color = Color.white * 0.3f;
							gradient.SetKeys(colorKey, alphaKey);
							secondClosestBiomeColor = gradient.Evaluate(height);
							break;
						case Biomes.GRASS:
							colorKey[0].color = Color.green;
							colorKey[1].color = Color.green * 0.3f;
							gradient.SetKeys(colorKey, alphaKey);
							secondClosestBiomeColor = gradient.Evaluate(height);
							break;
						case Biomes.DESERT:
							colorKey[0].color = Color.yellow;
							colorKey[1].color = Color.yellow * 0.3f;
							gradient.SetKeys(colorKey, alphaKey);
							secondClosestBiomeColor = gradient.Evaluate(height);
							break;
						case Biomes.OCEAN:
							colorKey[0].color = Color.blue;
							colorKey[1].color = Color.blue * 0.3f;
							gradient.SetKeys(colorKey, alphaKey);
							secondClosestBiomeColor = gradient.Evaluate(height);
							break;
					}
					colorKey[0].color = secondClosestBiomeColor;
					colorKey[1].color = closestBiomeColor;
					gradient.SetKeys(colorKey, alphaKey);
					Color color = gradient.Evaluate(proportion);
					terrainTexture.SetPixel(x, y, color);
				}
			}
			terrainTexture.Apply();
			return terrainTexture;
		}
	}


	public static class Perlin {
		public static int Seed {
			get {
				return seed;
			}
			set {
				seed = value;
				System.Random random = new System.Random(value);
				offset = Vector3.up * UnityEngine.Random.Range(-100f, 100);
				offset += Vector3.left * UnityEngine.Random.Range(-100f, 100);
				offset += Vector3.forward * UnityEngine.Random.Range(-100f, 100);
			}
		}
		public static float Scale = 1f;

		static int seed = 0;
		static Vector3 offset = Vector3.zero;

		public static float PerlinNoise(Vector3 vector) {
			vector *= Scale;
			vector += offset;
			return PerlinNoise(vector.x, vector.y, vector.z);
		}
		static float PerlinNoise(float x, float y, float z) {
			float noise = 0;
			noise += Mathf.PerlinNoise(x, y);
			noise += Mathf.PerlinNoise(y, z);
			noise += Mathf.PerlinNoise(z, x);
			noise += Mathf.PerlinNoise(y, x);
			noise += Mathf.PerlinNoise(x, z);
			noise += Mathf.PerlinNoise(z, y);
			return noise / 6f;
		}
	}

	public enum Biomes {
		SNOW, GRASS, DESERT, OCEAN
	}
}