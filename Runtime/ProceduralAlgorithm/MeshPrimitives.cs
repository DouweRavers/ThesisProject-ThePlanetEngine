using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// A class containing basic mesh shapes.
    /// </summary>
    internal static class MeshPrimitives
    {
        /// <summary>
        /// A quad (2D square in 3D space) with sides of 1m
        /// </summary>
        public static Mesh UnitQuad { get { return GenerateUnitQuadMesh(); } }

        /// <summary>
        /// A cube with sides of 1m
        /// </summary>
        public static Mesh UnitCube { get { return GenerateUnitCubeMesh(); } }

        /// <summary>
        /// A sphere with a radius of 1m
        /// </summary>
        public static Mesh UnitSphere { get { return GenerateUnitSphere(); } }

        static Mesh GenerateUnitQuadMesh()
        { // basis for a plane

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

        static Mesh GenerateUnitCubeMesh()
        {
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

        static Mesh GenerateUnitSphere()
        {
            Mesh mesh = MeshModifier.Subdivide(UnitCube, 5);
            mesh = MeshModifier.NormalizeAndAmplify(mesh, 1f);
            return mesh;
        }
    }
}


