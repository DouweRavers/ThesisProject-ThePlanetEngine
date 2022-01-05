using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine {

	//internal
	public static class MeshTool {

		/*###########################################
		  #           Primitive Meshes              #
		  ###########################################*/

		// TODO: Just turn these functions in obj files. 
		public static Mesh GenerateUnitQuadMesh() { // basis for a plane

			List<Vector3> newVertices = new List<Vector3>() {
			new Vector3(-0.5f,0,-0.5f),
			new Vector3(-0.5f,0,0.5f),
			new Vector3(0.5f,0,0.5f),
			new Vector3(0.5f,0,-0.5f)
		};

			List<int> newIndexes = new List<int>(){
			0,1,2, 0,2,3
		};

			List<Vector2> newUV = new List<Vector2>() {
			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0)
		};

			Mesh mesh = new Mesh();
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.vertices = newVertices.ToArray();
			mesh.triangles = newIndexes.ToArray();
			mesh.uv = newUV.ToArray();
			return mesh;
		}
		public static Mesh GenerateUnitQuadMesh(Vector2[] uvSubSpace) {
			Mesh mesh = GenerateUnitQuadMesh();
			mesh.uv = uvSubSpace;
			return mesh;
		}
		public static Mesh GenerateUnitCubeMesh() {

			List<Vector3> newVertices = new List<Vector3>() {
            // Front Face
			new Vector3(0.5f,-0.5f,0.5f), // 0 x,-y,z
            new Vector3(0.5f,0.5f,0.5f), // 1 x,y,z
			new Vector3(-0.5f,0.5f,0.5f), // 2 -x,y,z
			new Vector3(-0.5f,-0.5f,0.5f), // 3 -x,-y,z

            // LeftFace
			new Vector3(0.5f,-0.5f,-0.5f), // 4 x,-y,-z
			new Vector3(0.5f,0.5f,-0.5f), // 5 x,y,-z
            new Vector3(0.5f,0.5f,0.5f), // 6 x,y,z
			new Vector3(0.5f,-0.5f,0.5f), // 7 x,-y,z

            // RightFace
            new Vector3(-0.5f,-0.5f,0.5f), // 8 -x,-y,z
            new Vector3(-0.5f,0.5f,0.5f), // 9 -x,y,z
            new Vector3(-0.5f,0.5f,-0.5f), // 10 -x,y,-z
			new Vector3(-0.5f,-0.5f,-0.5f), // 11 -x,-y,-z

            // TopFace
            new Vector3(0.5f,0.5f,0.5f), // 12 x,y,z
            new Vector3(0.5f,0.5f,-0.5f), // 13 x,y,-z
            new Vector3(-0.5f,0.5f,-0.5f), // 14 -x,y,-z
            new Vector3(-0.5f,0.5f,0.5f), // 15 -x,y,z

            // BottomFace
			new Vector3(0.5f,-0.5f,-0.5f), // 16 x, -y, -z
			new Vector3(0.5f,-0.5f,0.5f), // 17 x, -y, z
			new Vector3(-0.5f,-0.5f,0.5f), // 18 -x,-y,z
            new Vector3(-0.5f,-0.5f,-0.5f), // 19 -x,-y,-z

            // BackFace
            new Vector3(-0.5f,-0.5f,-0.5f), // 20 -x,-y,-z
            new Vector3(-0.5f,0.5f,-0.5f), // 21 -x,y,-z
            new Vector3(0.5f,0.5f,-0.5f), // 22 x,y,-z
            new Vector3(0.5f,-0.5f,-0.5f) // 23 x,-y,-z
        };

			List<int> newIndexes = new List<int>() {
			0,1,2, 0,2,3, // FrontFace
            4,5,6, 4,6,7, // LeftFace
            8,9,10, 8,10,11, // RightFace
            12,13,14, 12,14,15, // TopFace
            16,17,18, 16,18,19, // BottomFace
            20,21,22, 20,22,23 // BackFace
		};

			List<Vector2> newUV = new List<Vector2>() {
            // FrontFace
            new Vector2(1f/4, 1f/3), // 1 x0y0
            new Vector2(1f/4, 2f/3), // 2 x0y1
            new Vector2(2f/4, 2f/3), // 3 x1y1
            new Vector2(2f/4, 1f/3), // 4 x1y0

            // LeftFace
            new Vector2(0, 1f/3), // 1 x0y0
            new Vector2(0, 2f/3), // 2 x0y1
            new Vector2(1f/4, 2f/3), // 3 x1y1
            new Vector2(1f/4, 1f/3), // 4 x1y0

            // RightFace
            new Vector2(2f/4, 1f/3), // 1 x0y0
            new Vector2(2f/4, 2f/3), // 2 x0y1
            new Vector2(3f/4, 2f/3), // 3 x1y1
            new Vector2(3f/4, 1f/3), // 4 x1y0

            // TopFace
            new Vector2(1f/4, 2f/3), // 1 x0y0
            new Vector2(1f/4, 1), // 2 x0y1
            new Vector2(2f/4, 1), // 3 x1y1
            new Vector2(2f/4, 2f/3), // 4 x1y0

            // BottomFace
			
            new Vector2(1f/4, 0), // 1 x0y0
            new Vector2(1f/4, 1f/3), // 2 x0y1
            new Vector2(2f/4, 1f/3), // 3 x1y1
            new Vector2(2f/4, 0), // 4 x1y0
            
            // BackFace
            new Vector2(3f/4, 1f/3), // 1 x0y0
            new Vector2(3f/4, 2f/3), // 2 x0y1
            new Vector2(1, 2f/3), // 3 x1y1
            new Vector2(1, 1f/3), // 4 x1y0

        };

			Mesh mesh = new Mesh();
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.vertices = newVertices.ToArray();
			mesh.triangles = newIndexes.ToArray();
			mesh.uv = newUV.ToArray();
			return mesh;
		}

		/*###########################################
		  #                 Modifiers               #
		  ###########################################*/

		// CPU based. slow but minimal best mesh result
		public static Mesh SubdivideCPU(Mesh mesh, int depth = 1) {
			if (depth <= 0) {
				return mesh; // end recursive loop
			}
			Vector3[] vertices_old = mesh.vertices;
			Vector2[] uvs_old = mesh.uv;
			int[] triangles_old = mesh.triangles;

			List<Vector3> vertices_new = new List<Vector3>();
			List<Vector2> uvs_new = new List<Vector2>();
			List<int> triangles_new = new List<int>();

			Dictionary<string, int> newpoints = new Dictionary<string, int>();
			for (int i = 0; i < triangles_old.Length; i += 6) {
				//			A-N-B
				//	A-B     |\|\|     triangles always set in form :
				//	|\| =>  M-O-P =>    t1: A,C,D, 
				//	C-D     |\|\|       t2: A,D,B
				//			C-Q-D


				// get corner points old quad
				int A_old = triangles_old[i];
				int C_old = triangles_old[i + 1];
				int D_old = triangles_old[i + 2];
				int B_old = triangles_old[i + 5];

				// create edges as keys for locating created middle points
				// for debugging strings -> change to long with 32 bit shift once working
				string edgeAB = "" + A_old + " " + B_old;
				string edgeBA = "" + B_old + " " + A_old;

				string edgeBD = "" + B_old + " " + D_old;
				string edgeDB = "" + D_old + " " + B_old;

				string edgeDC = "" + D_old + " " + C_old;
				string edgeCD = "" + C_old + " " + D_old;

				string edgeCA = "" + C_old + " " + A_old;
				string edgeAC = "" + A_old + " " + C_old;

				string edgeAD = "" + A_old + " " + D_old;
				string edgeDA = "" + D_old + " " + A_old;

				// add old vertices to new mesh
				vertices_new.Add(vertices_old[A_old]);
				uvs_new.Add(uvs_old[A_old]);
				int A_new = vertices_new.Count - 1;
				vertices_new.Add(vertices_old[B_old]);
				uvs_new.Add(uvs_old[B_old]);
				int B_new = vertices_new.Count - 1;
				vertices_new.Add(vertices_old[C_old]);
				uvs_new.Add(uvs_old[C_old]);
				int C_new = vertices_new.Count - 1;
				vertices_new.Add(vertices_old[D_old]);
				uvs_new.Add(uvs_old[D_old]);
				int D_new = vertices_new.Count - 1;

				// create or search middlepoints

				int[] intermediate_points = new int[5];
				// N = midpoint between A and B
				// M = midpoint between A and C
				// O = midpoint between A and D
				// P = midpoint between B and D
				// Q = midpoint between C and D


				string[] edges = new string[]{
				edgeAB, edgeBA, // midpoint N
				edgeAC, edgeCA, // midpoint M
				edgeAD, edgeDA, // midpoint O
				edgeBD, edgeDB, // midpoint P
				edgeCD, edgeDC  // midpoint Q
			};

				int[] edge_points = new int[]{
				A_new, B_new, // edge AB/BA
				A_new, C_new, // edge AC/CA
				A_new, D_new, // edge AD/DA
				B_new, D_new, // edge BD/DB
				C_new, D_new, // edge CD/DC
			};

				for (int k = 0; k < 5; k++) {
					try {
						intermediate_points[k] = newpoints[edges[k * 2]]; // test edge
					} catch (KeyNotFoundException) {
						Vector3 new_vertice = Vector3.Lerp(vertices_new[edge_points[k * 2]], vertices_new[edge_points[k * 2 + 1]], 0.5f);
						Vector2 new_uv = Vector2.Lerp(uvs_new[edge_points[k * 2]], uvs_new[edge_points[k * 2 + 1]], 0.5f);
						vertices_new.Add(new_vertice);
						uvs_new.Add(new_uv);
						intermediate_points[k] = vertices_new.Count - 1;
						newpoints[edges[k * 2]] = intermediate_points[k]; // make entry in map
						newpoints[edges[k * 2 + 1]] = intermediate_points[k]; // make entry for reversed edge in map
					}
				}
				int N = intermediate_points[0], M = intermediate_points[1], O = intermediate_points[2],
					P = intermediate_points[3], Q = intermediate_points[4];

				//			A-N-B
				//	A-B     |\|\|     triangles always set in form :
				//	|\| =>  M-O-P =>    t1: A,C,D, 
				//	C-D     |\|\|       t2: A,D,B
				//			C-Q-D

				// Quat 1
				// triangle AMO
				triangles_new.Add(A_new);
				triangles_new.Add(M);
				triangles_new.Add(O);
				// triangle AON
				triangles_new.Add(A_new);
				triangles_new.Add(O);
				triangles_new.Add(N);


				// Quat 2
				// triangle NOP
				triangles_new.Add(N);
				triangles_new.Add(O);
				triangles_new.Add(P);
				// triangle NPB
				triangles_new.Add(N);
				triangles_new.Add(P);
				triangles_new.Add(B_new);

				// Quat 3
				// triangle MCQ
				triangles_new.Add(M);
				triangles_new.Add(C_new);
				triangles_new.Add(Q);
				// triangle MQO
				triangles_new.Add(M);
				triangles_new.Add(Q);
				triangles_new.Add(O);

				// Quat 4
				// triangle OQD
				triangles_new.Add(O);
				triangles_new.Add(Q);
				triangles_new.Add(D_new);
				// triangle ODP
				triangles_new.Add(O);
				triangles_new.Add(D_new);
				triangles_new.Add(P);
			}
			mesh.vertices = vertices_new.ToArray();
			mesh.triangles = triangles_new.ToArray();
			mesh.uv = uvs_new.ToArray();
			return SubdivideCPU(mesh, depth - 1); // recursive
		}

		// GPU based. fast but inefficient mesh
		public static Mesh SubdivideGPU(Mesh mesh, int depth = 1) {
			int[] indexes = mesh.triangles;
			Vector3[] vertices = mesh.vertices;
			Vector2[] uv = mesh.uv;
			ComputeBuffer oldIndexBuffer = new ComputeBuffer(indexes.Length, sizeof(int));
			oldIndexBuffer.SetData(indexes);
			ComputeBuffer oldVertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
			oldVertexBuffer.SetData(vertices);
			ComputeBuffer oldUvBuffer = new ComputeBuffer(uv.Length, sizeof(float) * 2);
			oldUvBuffer.SetData(uv);

			ComputeBuffer newIndexBuffer = new ComputeBuffer(indexes.Length * 4, sizeof(int));
			ComputeBuffer newVertexBuffer = new ComputeBuffer(indexes.Length * 2, sizeof(float) * 3);
			ComputeBuffer newUvBuffer = new ComputeBuffer(indexes.Length * 2, sizeof(float) * 2);

			ComputeShader meshShader = Resources.Load<ComputeShader>("ComputeShaders/MeshShader");
			if (meshShader == null) Debug.LogWarning("No shader loaded");
			int kernelIndex = meshShader.FindKernel("SubdivideMesh");

			meshShader.SetBuffer(kernelIndex, "old_index_array", oldIndexBuffer);
			meshShader.SetBuffer(kernelIndex, "old_vertice_array", oldVertexBuffer);
			meshShader.SetBuffer(kernelIndex, "old_uv_array", oldUvBuffer);

			meshShader.SetBuffer(kernelIndex, "new_index_array", newIndexBuffer);
			meshShader.SetBuffer(kernelIndex, "new_vertice_array", newVertexBuffer);
			meshShader.SetBuffer(kernelIndex, "new_uv_array", newUvBuffer);

			int threads = Mathf.RoundToInt(indexes.Length / 3f);
			meshShader.SetInt("maximum", threads);
			if (60000 < threads) {
				int factor = Mathf.CeilToInt(threads / 60000f);
				meshShader.SetInt("batch", factor);
				threads = 60000;
			} else {
				meshShader.SetInt("batch", 1);
			}

			meshShader.Dispatch(kernelIndex, threads, 1, 1);
			Vector3[] newVerices = new Vector3[indexes.Length * 2];
			newVertexBuffer.GetData(newVerices);
			Vector2[] newUV = new Vector2[indexes.Length * 2];
			newUvBuffer.GetData(newUV);
			int[] newIndexes = new int[indexes.Length * 4];
			newIndexBuffer.GetData(newIndexes);


			oldIndexBuffer.Dispose();
			oldVertexBuffer.Dispose();
			oldUvBuffer.Dispose();
			newIndexBuffer.Dispose();
			newVertexBuffer.Dispose();
			newUvBuffer.Dispose();

			mesh.vertices = newVerices;
			mesh.triangles = newIndexes;
			mesh.uv = newUV;
			return mesh;
		}

		public static Mesh NormalizeAndAmplify(Mesh mesh, float gain) {
			Vector3[] vertices = mesh.vertices;

			ComputeShader meshShader = Resources.Load<ComputeShader>("ComputeShaders/MeshShader");
			if (meshShader == null) Debug.LogWarning("No shader loaded");

			int kernelIndex = meshShader.FindKernel("NormalizeAndAmplify");
			// Pass vertices
			ComputeBuffer oldVertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
			oldVertexBuffer.SetData(vertices);
			meshShader.SetBuffer(kernelIndex, "old_vertice_array", oldVertexBuffer);
			ComputeBuffer newVertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
			meshShader.SetBuffer(kernelIndex, "new_vertice_array", newVertexBuffer);
			// Pass gain
			meshShader.SetFloat("amplifier", gain);
			// Pass batch size
			int threads = Mathf.RoundToInt(vertices.Length);
			meshShader.SetInt("maximum", threads);
			if (60000 < threads) {
				int factor = Mathf.CeilToInt(threads / 60000f);
				meshShader.SetInt("batch", factor);
				threads = 60000;
			} else {
				meshShader.SetInt("batch", 1);
			}
			meshShader.Dispatch(kernelIndex, threads, 1, 1);

			newVertexBuffer.GetData(vertices);
			oldVertexBuffer.Dispose();
			newVertexBuffer.Dispose();
			mesh.vertices = vertices;

			return mesh;
		}

		public static Mesh OffsetMesh(Mesh mesh, Vector3 offset) {
			Vector3[] vertices = mesh.vertices;

			ComputeShader meshShader = Resources.Load<ComputeShader>("ComputeShaders/MeshShader");
			if (meshShader == null) Debug.LogWarning("No shader loaded");

			int kernelIndex = meshShader.FindKernel("Offset");
			// Pass vertices to shader
			ComputeBuffer oldVertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
			oldVertexBuffer.SetData(vertices);
			meshShader.SetBuffer(kernelIndex, "old_vertice_array", oldVertexBuffer);
			ComputeBuffer newVertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
			meshShader.SetBuffer(kernelIndex, "new_vertice_array", newVertexBuffer);
			// Pass offset to shader
			meshShader.SetFloats("offset_vector", new float[] { offset.x, offset.y, offset.z });
			// Pass batch size to shader
			int threads = Mathf.RoundToInt(vertices.Length);
			meshShader.SetInt("maximum", threads);
			if (60000 < threads) {
				int factor = Mathf.CeilToInt(threads / 60000f);
				meshShader.SetInt("batch", factor);
				threads = 60000;
			} else {
				meshShader.SetInt("batch", 1);
			}
			meshShader.Dispatch(kernelIndex, threads, 1, 1);

			newVertexBuffer.GetData(vertices);
			oldVertexBuffer.Dispose();
			newVertexBuffer.Dispose();
			mesh.vertices = vertices;
			return mesh;
		}

		public static Mesh[] SplitPlaneMeshInFour(Mesh mesh, bool resetUV = true) {
			int[] indices = mesh.triangles;
			Vector3[] vertices = mesh.vertices;
			Vector2[] uvs = mesh.uv;
			// Calculate bounds to get centre point
			mesh.RecalculateBounds();
			Vector3 center = mesh.bounds.center;
			// Create 4 new meshes and corrisponding arrays
			Mesh[] meshes = new Mesh[] {
				new Mesh(), new Mesh(), new Mesh(), new Mesh()
				};
			List<int>[] indicesArrays = new List<int>[] {
				new List<int>(), new List<int>(), new List<int>(), new List<int>()
				};
			List<Vector3>[] verticesArrays = new List<Vector3>[] {
				new List<Vector3>(), new List<Vector3>(), new List<Vector3>(), new List<Vector3>()
				};
			List<Vector2>[] uvsArrays = new List<Vector2>[] {
				new List<Vector2>(), new List<Vector2>(), new List<Vector2>(), new List<Vector2>()
				};

			// Assign every triangle to one of the four new meshes
			for (int i = 0; i < indices.Length; i += 3) {
				Vector3 triangleCenter = (vertices[indices[i]] + vertices[indices[i + 1]] + vertices[indices[i + 2]]) / 3f;
				triangleCenter.y = 0;
				int alpha = (triangleCenter.x < center.x ? 0 : 2) + (triangleCenter.z < center.z ? 0 : 1);
				indicesArrays[alpha].Add(verticesArrays[alpha].Count);
				verticesArrays[alpha].Add(vertices[indices[i]]);
				uvsArrays[alpha].Add(uvs[indices[i]]);
				indicesArrays[alpha].Add(verticesArrays[alpha].Count);
				verticesArrays[alpha].Add(vertices[indices[i + 1]]);
				uvsArrays[alpha].Add(uvs[indices[i + 1]]);
				indicesArrays[alpha].Add(verticesArrays[alpha].Count);
				verticesArrays[alpha].Add(vertices[indices[i + 2]]);
				uvsArrays[alpha].Add(uvs[indices[i + 2]]);
			}

			for (int i = 0; i < 4; i++) {
				if (resetUV) {
					Vector2 start = uvsArrays[i][0];
					for (int j = 0; j < uvsArrays[i].Count; j++) {
						uvsArrays[i][j] = 2 * (uvsArrays[i][j] - start);
					}
				}
				meshes[i].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
				meshes[i].vertices = verticesArrays[i].ToArray();
				meshes[i].triangles = indicesArrays[i].ToArray();
				meshes[i].uv = uvsArrays[i].ToArray();
			}

			return meshes;
		}

		public static Mesh ApplyHeightmap(Mesh mesh, Texture2D heigtmap, float radius) {
			Vector3[] vertices = mesh.vertices;
			Vector2[] uvs = mesh.uv;

			ComputeShader meshShader = Resources.Load<ComputeShader>("ComputeShaders/MeshShader");
			if (meshShader == null) Debug.LogWarning("No shader loaded");

			int kernelIndex = meshShader.FindKernel("ApplyHeightmap");
			// Pass vertices to shader
			ComputeBuffer oldVertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
			oldVertexBuffer.SetData(vertices);
			meshShader.SetBuffer(kernelIndex, "old_vertice_array", oldVertexBuffer);
			ComputeBuffer oldUVBuffer = new ComputeBuffer(uvs.Length, sizeof(float) * 2);
			oldUVBuffer.SetData(uvs);
			meshShader.SetBuffer(kernelIndex, "old_uv_array", oldUVBuffer);


			ComputeBuffer newVertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
			meshShader.SetBuffer(kernelIndex, "new_vertice_array", newVertexBuffer);
			// Pass offset to shader
			meshShader.SetTexture(kernelIndex, "heightmap", heigtmap);
			meshShader.SetInt("width", heigtmap.width);
			meshShader.SetInt("height", heigtmap.height);
			meshShader.SetFloat("radius", radius);
			// Pass batch size to shader
			int threads = Mathf.RoundToInt(vertices.Length);
			meshShader.SetInt("maximum", threads);
			if (60000 < threads) {
				int factor = Mathf.CeilToInt(threads / 60000f);
				meshShader.SetInt("batch", factor);
				threads = 60000;
			} else {
				meshShader.SetInt("batch", 1);
			}
			meshShader.Dispatch(kernelIndex, threads, 1, 1);

			newVertexBuffer.GetData(vertices);
			oldVertexBuffer.Dispose();
			oldUVBuffer.Dispose();
			newVertexBuffer.Dispose();
			mesh.vertices = vertices;
			return mesh;
		}
	}



}


