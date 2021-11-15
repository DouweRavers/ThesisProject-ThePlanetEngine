using UnityEngine;
namespace PlanetEngine {

	internal class TextureGenerator : ScriptableObject {
		public static ComputeShader textureShader;

		static TextureGenerator() { }

		// Generates a base texture on which all textures base. This method will take a mesh with UV coordinates and generate a
		// Texture that maps all (normalized) vector values as RGB values by interpolating between two given vertices.
		// The A value is the magnitude of the vector
		public static Texture2D GenerateBaseTexture(Mesh mesh, Rect size) {
			Texture2D baseTexture = new Texture2D(Mathf.RoundToInt(size.height), Mathf.RoundToInt(size.width), TextureFormat.RGBAFloat, false);
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

				for (int y = Mathf.RoundToInt(origin.y); y < end.y; y++) {
					Vector3 projectedVerticeY = verticeY * (y - origin.y) / (baseTexture.height / 3);
					for (int x = Mathf.RoundToInt(origin.x); x < end.x; x++) {
						Vector3 projectedVerticeX = verticeX * (x - origin.x) / (baseTexture.width / 3);
						Vector3 projectedVertex = originVertex + projectedVerticeX + projectedVerticeY;
						baseTexture.SetPixel(x, y,
							new Color(projectedVertex.normalized.x, projectedVertex.normalized.y, projectedVertex.normalized.z, projectedVertex.magnitude));
					}
				}
			}
			baseTexture.Apply();
			return baseTexture;
		}

		public static Texture2D CreateHeightmap(Texture2D baseTexture) {
			RenderTexture renderHeightMapTexture = new RenderTexture(baseTexture.width, baseTexture.height, 24);
			renderHeightMapTexture.enableRandomWrite = true;
			renderHeightMapTexture.Create();

			RenderTexture renderBaseTexture = new RenderTexture(baseTexture.width, baseTexture.height, 24);
			renderBaseTexture.enableRandomWrite = true;
			RenderTexture.active = renderBaseTexture;
			Graphics.Blit(baseTexture, renderBaseTexture);

			textureShader.SetTexture(0, "base_texture", renderBaseTexture);
			textureShader.SetTexture(0, "height_map_texture", renderHeightMapTexture);
			textureShader.Dispatch(0, renderHeightMapTexture.width / 8, renderHeightMapTexture.height / 8, 1);

			Texture2D outputTexture = new Texture2D(renderHeightMapTexture.width, renderHeightMapTexture.height, TextureFormat.RGB24, true);
			RenderTexture.active = renderHeightMapTexture;
			outputTexture.ReadPixels(new Rect(0, 0, renderHeightMapTexture.width, renderHeightMapTexture.height), 0, 0);
			outputTexture.Apply();

			return outputTexture;
		}
	}
}