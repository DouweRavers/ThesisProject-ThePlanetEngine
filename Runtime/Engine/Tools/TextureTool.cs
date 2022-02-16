using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using System;

namespace PlanetEngine {

	public static class TextureTool {

		// Generates a base texture on which all textures base. This method will take a mesh with UV coordinates and generate a
		// Texture that maps all (normalized) vector values as RGB values by interpolating between two given vertices.
		// The A value is the magnitude of the vector
		[Obsolete("A more performand GPU based method is available. Maybe for mobile applications this is a option.", false)]
		public static Texture2D GenerateBaseTextureCPU(Mesh mesh, Rect size) {
			Texture2D baseTexture = new Texture2D(Mathf.RoundToInt(size.width), Mathf.RoundToInt(size.height), TextureFormat.RGBAFloat, false);
			baseTexture.filterMode = FilterMode.Point;
			for (int x = 0; x < baseTexture.width; x++) {
				for (int y = 0; y < baseTexture.height; y++) {
					baseTexture.SetPixel(x, y, new Color(0, 0, 0, -1));
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

		public static Texture2D GenerateBaseTextureGPU(int width, int height) {
			ComputeShader textureShader = Resources.Load<ComputeShader>("ComputeShaders/TextureShader");
			if (textureShader == null) Debug.LogWarning("No shader loaded");
			int kernelIndex = textureShader.FindKernel("GenerateBaseTexture");

			RenderTexture baseTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
			baseTexture.enableRandomWrite = true;
			baseTexture.Create();
			textureShader.SetTexture(kernelIndex, "base_texture_out", baseTexture);
			textureShader.SetInt("width", width - 1);
			textureShader.SetInt("height", height - 1);
			textureShader.Dispatch(kernelIndex, width, height, 1);
			Texture2D outputTexture = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
			outputTexture.filterMode = FilterMode.Point;
			RenderTexture.active = baseTexture;
			outputTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			outputTexture.Apply();
			return outputTexture;
		}

		[Obsolete("A more performand GPU based method is available. Maybe for mobile applications this is a option.", false)]
		public static Texture2D GenerateBiomeTextureCPU(Texture2D baseTexture, Texture2D heigtMap) {
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
					if (points[i].y < 0.1f && points[i].y > -0.1f) biomes[i] = Biomes.DESERT;
					else biomes[i] = Biomes.GRASS;
				}
			}
			// r closest, g second closest, b proportion = (SCdist - Cdist) / SCdist, a empty 
			Texture2D vonoroiTexture = new Texture2D(baseTexture.width, baseTexture.height);
			vonoroiTexture.filterMode = FilterMode.Point;
			for (int x = 0; x < baseTexture.width; x++) {
				for (int y = 0; y < baseTexture.height; y++) {
					vonoroiTexture.SetPixel(x, y, Color.black);
					Color positionColor = baseTexture.GetPixel(x, y);
					if (positionColor.a == 0) continue;
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
					vonoroiTexture.SetPixel(x, y, new Color((float)closest / 4f, (float)secondClosest / 4f, (secondMinimumDistance - minimumDistance) / secondMinimumDistance, 1));
				}
			}
			vonoroiTexture.Apply();
			return vonoroiTexture;
		}

		public static Texture2D GenerateBiomeTextureGPU(Texture2D baseTexture, int points) {
			// Select random points for vonoroi algorithm

			// use multithreading (unity jobs - DOTS) to fastly generate many points
			NativeArray<Color> baseTextureBuffer = baseTexture.GetRawTextureData<Color>();
			NativeArray<float3> vonoroiPoints = new NativeArray<float3>(points, Allocator.TempJob);
			GenerateVonoroiPointsJob vonoroiPointsJob = new GenerateVonoroiPointsJob() {
				baseSeed = Time.frameCount * 10,
				height = baseTexture.height,
				width = baseTexture.width,
				baseTexture = baseTextureBuffer,
				vonoroiPoints = vonoroiPoints,
			};
			vonoroiPointsJob.Schedule(points, 1).Complete();
			// Once points are chosen create texture for the different biomes using GPU 

			ComputeShader textureShader = Resources.Load<ComputeShader>("ComputeShaders/TextureShader");
			if (textureShader == null) Debug.LogWarning("No shader loaded");
			int kernelIndex = textureShader.FindKernel("GenerateBiomeTexture");
			// Create new texture with R8 type (int of 8bytes = 255 biomes types)
			RenderTexture biomeRenderTexture = new RenderTexture(baseTexture.width, baseTexture.height, 24);
			biomeRenderTexture.enableRandomWrite = true;
			biomeRenderTexture.filterMode = FilterMode.Point;
			biomeRenderTexture.Create();
			textureShader.SetTexture(kernelIndex, "biome_texture_out", biomeRenderTexture);
			// input base texture for vertex data
			textureShader.SetTexture(kernelIndex, "base_texture", baseTexture);
			textureShader.SetInt("width", baseTexture.width - 1);
			textureShader.SetInt("height", baseTexture.height - 1);
			// input vonoroi points 
			ComputeBuffer vonoroiPointsBuffer = new ComputeBuffer(vonoroiPoints.Length, 3 * sizeof(float));
			vonoroiPointsBuffer.SetData(vonoroiPoints);
			textureShader.SetBuffer(kernelIndex, "vonoroi_points", vonoroiPointsBuffer);
			textureShader.SetInt("vonoroi_point_count", vonoroiPoints.Length);
			// Perform calculation on GPU
			textureShader.Dispatch(kernelIndex, baseTexture.width, baseTexture.height, 1);
			// Receive output
			Texture2D biomeTexture = new Texture2D(biomeRenderTexture.width, biomeRenderTexture.height);
			biomeTexture.filterMode = FilterMode.Point;
			RenderTexture.active = biomeRenderTexture;
			biomeTexture.ReadPixels(new Rect(0, 0, biomeRenderTexture.width, biomeRenderTexture.height), 0, 0, false);
			RenderTexture.active = null;
			biomeTexture.Apply();
			// Free up unmanaged memory
			baseTextureBuffer.Dispose();
			vonoroiPoints.Dispose();
			vonoroiPointsBuffer.Dispose();
			return biomeTexture;
		}

		[BurstCompile]
		struct GenerateVonoroiPointsJob : IJobParallelFor {
			[ReadOnly]
			public int baseSeed;
			[ReadOnly]
			public int height, width;
			[ReadOnly]
			public NativeArray<Color> baseTexture;
			// Output
			public NativeArray<float3> vonoroiPoints;
			public void Execute(int index) {
				Unity.Mathematics.Random random = new Unity.Mathematics.Random();
				random.InitState((uint)((baseSeed + 1) * (index + 1)));
				int x = random.NextInt(width);
				int y = 0;
				if (width / 4 < x && x < width / 2)
					y = random.NextInt(height);
				else
					y = random.NextInt(height / 3, 2 * height / 3);
				Color color = baseTexture[x + y * width];
				vonoroiPoints[index] = new float3(color.r, color.g, color.b);
			}
		}

		[Obsolete("A more performand multi-threaded based method is available.", false)]
		public static Texture2D GenerateHeightTexture(Texture2D baseTexture) {
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

		public static Texture2D GenerateHeightTextureThreaded(Texture2D baseTexture, float scaler = 2, bool binary = false) {
			Texture2D heightTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RFloat, false);
			heightTexture.filterMode = FilterMode.Point;
			Perlin.Seed = Time.frameCount;
			Perlin.Scale = 2.5f;
			NativeArray<Color> baseTextureBuffer = baseTexture.GetRawTextureData<Color>();
			NativeArray<float> heightTextureBuffer = new NativeArray<float>(baseTextureBuffer.Length, Allocator.TempJob);
			GenerateHeightTextureJob heightTextureJob = new GenerateHeightTextureJob() {
				scaler = scaler,
				binary = binary,
				height = baseTexture.height,
				width = baseTexture.width,
				baseTexture = baseTextureBuffer,
				heightTexture = heightTextureBuffer
			};
			heightTextureJob.Schedule(baseTextureBuffer.Length, 1).Complete();
			heightTexture.SetPixelData(heightTextureBuffer.ToArray(), 0);
			heightTexture.Apply();
			heightTextureBuffer.Dispose();
			return heightTexture;
		}

		[BurstCompile]
		struct GenerateHeightTextureJob : IJobParallelFor {

			[ReadOnly]
			public float scaler;
			[ReadOnly]
			public bool binary;
			[ReadOnly]
			public int height, width;
			[ReadOnly]
			public NativeArray<Color> baseTexture;
			// Output
			public NativeArray<float> heightTexture;
			public void Execute(int index) {
				Color color = baseTexture[index];
				if (color.a == 0) return;
				Vector3 vertex = new Vector3(color.r, color.g, color.b);
				float height = noise.cnoise(vertex * scaler);
				if (binary) {
					height = height < 0 ? 0 : 1;
				} else {
					height = 0.5f * height + 0.5f;
				}
				heightTexture[index] = height;
			}
		}

		public static Texture2D GenerateTerrainColorTexture(Texture2D biomeTexture, Texture2D heigtMap) {
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

		public static Texture2D RegenerateBaseTextureForSubSurface(Texture2D baseTexture, Rect zone, RectInt size) {
			ComputeShader textureShader = Resources.Load<ComputeShader>("ComputeShaders/TextureShader");
			if (textureShader == null) Debug.LogWarning("No shader loaded");
			int kernelIndex = textureShader.FindKernel("RegenerateBaseTexture");

			// Output texture
			RenderTexture renderTexture = new RenderTexture(size.width, size.height, 0, RenderTextureFormat.ARGBFloat);
			renderTexture.enableRandomWrite = true;
			renderTexture.Create();
			textureShader.SetTexture(kernelIndex, "base_texture_out", renderTexture);
			textureShader.SetInt("width", size.width - 1);
			textureShader.SetInt("height", size.height - 1);

			// Input vertices
			Color[] vertexColors = new Color[]{
				baseTexture.GetPixel(
					Mathf.RoundToInt(baseTexture.width * zone.x),
					Mathf.RoundToInt(baseTexture.height * zone.y)),
				baseTexture.GetPixel(
					Mathf.RoundToInt(baseTexture.width * (zone.x + zone.width))- 2,
					Mathf.RoundToInt(baseTexture.height * zone.y)+ 1),
				baseTexture.GetPixel(
					Mathf.RoundToInt(baseTexture.width * zone.x)+ 1,
					Mathf.RoundToInt(baseTexture.height * (zone.y + zone.height))- 2),
				baseTexture.GetPixel(
					Mathf.RoundToInt(baseTexture.width * (zone.x  + zone.width))- 2,
					Mathf.RoundToInt(baseTexture.height * (zone.y + zone.height))-2)
			};
			textureShader.SetFloats("left_bottom_corner", new float[] { vertexColors[0].r, vertexColors[0].g, vertexColors[0].b, vertexColors[0].a });
			textureShader.SetFloats("right_bottom_corner", new float[] { vertexColors[1].r, vertexColors[1].g, vertexColors[1].b, vertexColors[1].a });
			textureShader.SetFloats("left_top_corner", new float[] { vertexColors[2].r, vertexColors[2].g, vertexColors[2].b, vertexColors[2].a });
			textureShader.SetFloats("right_top_corner", new float[] { vertexColors[3].r, vertexColors[3].g, vertexColors[3].b, vertexColors[3].a });

			textureShader.Dispatch(kernelIndex, size.width, size.height, 1);
			Texture2D outputTexture = new Texture2D(size.width, size.height, TextureFormat.RGBAFloat, false);
			outputTexture.filterMode = FilterMode.Point;
			RenderTexture.active = renderTexture;
			outputTexture.ReadPixels(new Rect(0, 0, size.width, size.height), 0, 0);
			outputTexture.Apply();
			return outputTexture;
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