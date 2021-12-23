using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {
	/*internal*/
	public class TextureGenerator : ScriptableObject {
		public static ComputeShader textureShader;

		static TextureGenerator() { }

		// Generates a base texture on which all textures base. This method will take a mesh with UV coordinates and generate a
		// Texture that maps all (normalized) vector values as RGB values by interpolating between two given vertices.
		// The A value is the magnitude of the vector
		public static Texture2D GenerateBaseTexture(Mesh mesh, Rect size) {
			Texture2D baseTexture = new Texture2D(Mathf.RoundToInt(size.width), Mathf.RoundToInt(size.height), TextureFormat.RGBAFloat, false);
			baseTexture.filterMode = FilterMode.Point;
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

				for (int y = Mathf.RoundToInt(origin.y); y < Mathf.RoundToInt(end.y); y++) {
					Vector3 projectedVerticeY = verticeY * (y - origin.y) / (baseTexture.height / 3);
					for (int x = Mathf.RoundToInt(origin.x); x < Mathf.RoundToInt(end.x); x++) {
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

		public static Texture2D GenerateBiomeTexture(Texture2D baseTexture) {
			System.Random random = new System.Random();
			Vector3[] points = new Vector3[baseTexture.width];
			Color[] biomes = new Color[points.Length];
			for (int i = 0; i < points.Length; i++) {
				int randomX = random.Next() % baseTexture.width;
				int randomY = random.Next() % baseTexture.height;
				Color pointColor = baseTexture.GetPixel(randomX, randomY);
				if (pointColor.r < -100) {
					i--;
					continue;
				}
				points[i] = new Vector3(pointColor.r, pointColor.g, pointColor.b);
				if (points[i].y > 0.9f || points[i].y < -0.9f) biomes[i] = Color.white;
				else {
					if (random.Next() % 2 == 0) biomes[i] = Color.blue;
					else {
						if (points[i].y < 0.3f && points[i].y > -0.3f) biomes[i] = Color.yellow;
						else biomes[i] = Color.green;
					}
				}
			}
			Texture2D vonoroiTexture = new Texture2D(baseTexture.width, baseTexture.height);
			for (int x = 0; x < baseTexture.width; x++) {
				for (int y = 0; y < baseTexture.height; y++) {
					float minimumDistance = baseTexture.width + baseTexture.height;
					Color color = Color.black;
					for (int i = 0; i < points.Length; i++) {
						Color localColor = baseTexture.GetPixel(x, y);
						Vector3 localVector = new Vector3(localColor.r, localColor.g, localColor.b);
						float dist = Vector3.Distance(localVector.normalized, points[i].normalized);
						if (dist < minimumDistance) {
							minimumDistance = dist;
							color = biomes[i];
						}
					}
					vonoroiTexture.SetPixel(x, y, color);
				}
			}
			vonoroiTexture.Apply();
			return vonoroiTexture;
		}

		public static Texture2D GenerateHeightTexture(Texture2D baseTexture, Texture2D biomeTexture) {
			Texture2D heigtTexture = new Texture2D(baseTexture.width, baseTexture.height);
			for (int x = 0; x < baseTexture.width; x++) {
				for (int y = 0; y < baseTexture.height; y++) {
					Color vertexColor = baseTexture.GetPixel(x, y);
					Color biomeColor = biomeTexture.GetPixel(x, y);
					Vector3 vertex = new Vector3(vertexColor.r, vertexColor.g, vertexColor.b);
					float noise = Perlin.PerlinNoise(vertex * 5f);
					float amp = 0;
					if (biomeColor == Color.blue) amp = 0;
					if (biomeColor == Color.green) amp = 0.5f;
					if (biomeColor == Color.yellow) amp = 0.2f;
					if (biomeColor == Color.white) amp = 0.8f;
					Color color = Color.white * noise * amp;
					heigtTexture.SetPixel(x, y, color);
				}
			}
			heigtTexture.Apply();
			return heigtTexture;
		}
	}

	public class Perlin {
		public static float PerlinNoise(Vector3 vector) {
			return PerlinNoise(vector.x, vector.y, vector.z);
		}
		public static float PerlinNoise(float x, float y, float z) {
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
}