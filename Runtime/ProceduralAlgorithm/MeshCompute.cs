using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// This mesh compute interface exposes the functionality of the mesh shader.
    /// </summary>
    internal class MeshCompute : Compute
    {
        ComputeBuffer _inputMeshVertices, _inputMeshIndexes, _inputMeshUVs;
        ComputeBuffer _outputMeshVertices, _outputMeshIndexes, _outputMeshUVs;

        public new void SetKernel(string kernelName)
        {
            Shader = Resources.Load<ComputeShader>("MeshShaders/MeshShader");
            base.SetKernel(kernelName);
        }

        /// <summary>
        /// Adds array mesh to shader
        /// </summary>
        /// <param name="vertices">vertices of the mesh</param>
        /// <param name="indexes">indexes of the mesh</param>
        /// <param name="uvs">uvs of the mesh</param>
        public void SetInputMesh(Vector3[] vertices = null, int[] indexes = null, Vector2[] uvs = null)
        {
            // Create vertice buffer and assign to shader.
            if (vertices != null)
            {
                _inputMeshVertices = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
                _inputMeshVertices.SetData(vertices);
                Shader.SetBuffer(KernelId, "input_vertice_array", _inputMeshVertices);
            }
            // Create index buffer and assign to shader.
            if (indexes != null)
            {
                _inputMeshIndexes = new ComputeBuffer(indexes.Length, sizeof(int));
                _inputMeshIndexes.SetData(indexes);
                Shader.SetBuffer(KernelId, "input_index_array", _inputMeshIndexes);
            }
            // Create uvv buffer and assign to shader.
            if (uvs != null)
            {
                _inputMeshUVs = new ComputeBuffer(uvs.Length, sizeof(float) * 2);
                _inputMeshUVs.SetData(uvs);
                Shader.SetBuffer(KernelId, "input_uv_array", _inputMeshUVs);
            }
        }

        /// <summary>
        /// Adds mesh to shader by splitting it up in seperate arrays/buffers.
        /// </summary>
        /// <param name="mesh">the mesh to be added</param>
        public void SetInputMesh(Mesh mesh)
        {
            SetInputMesh(mesh.vertices, mesh.triangles, mesh.uv);
        }

        /// <summary>
        /// Takes the mesh from the output and inserts it as input.
        /// Only possible if shader runs multiple times.
        /// Allows for reusing buffers instead of recreating them.
        /// </summary>
        /// <param name="vertexSize">The size of the new output mesh</param>
        /// <param name="indexSize">The size of the new output indexes</param>
        public void SetOutputMeshAsInput(int vertexSize = 0, int indexSize = 0)
        {
            // TODO: make compatible with not all arrays active.
            // Clear old input mesh buffers.
            if (_inputMeshVertices != null) _inputMeshVertices.Dispose();
            if (_inputMeshIndexes != null) _inputMeshIndexes.Dispose();
            if (_inputMeshUVs != null) _inputMeshUVs.Dispose();

            // Set the output to the input.
            if (_outputMeshVertices != null) _inputMeshVertices = _outputMeshVertices;
            if (_outputMeshIndexes != null) _inputMeshIndexes = _outputMeshIndexes;
            if (_outputMeshUVs != null) _inputMeshUVs = _outputMeshUVs;

            // Add buffers to shader.
            if (_inputMeshVertices != null) Shader.SetBuffer(KernelId, "input_vertice_array", _inputMeshVertices);
            if (_inputMeshIndexes != null) Shader.SetBuffer(KernelId, "input_index_array", _inputMeshIndexes);
            if (_inputMeshUVs != null) Shader.SetBuffer(KernelId, "input_uv_array", _inputMeshUVs);

            SetOutputMeshProperties(vertexSize, indexSize);
        }

        /// <summary>
        /// Creates buffers to receive the computed meshdata.
        /// </summary>
        /// <param name="vertexSize">The size of the output vertices expected</param>
        /// <param name="indexSize">The size of the output indexes expected</param>
        public void SetOutputMeshProperties(int vertexSize = 0, int indexSize = 0)
        {
            // If values zero set to input mesh values.
            if (vertexSize == 0 || indexSize == 0)
            {
                if (_inputMeshVertices != null) vertexSize = _inputMeshVertices.count;
                if (_inputMeshIndexes != null) indexSize = _inputMeshIndexes.count;
            }

            // create output buffers if input buffer is given.
            if (_inputMeshVertices != null)
            {
                _outputMeshVertices = new ComputeBuffer(vertexSize, sizeof(float) * 3);
                Shader.SetBuffer(KernelId, "output_vertice_array", _outputMeshVertices);
            }
            if (_inputMeshIndexes != null)
            {
                _outputMeshIndexes = new ComputeBuffer(indexSize, sizeof(int));
                Shader.SetBuffer(KernelId, "output_index_array", _outputMeshIndexes);
            }
            if (_inputMeshUVs != null)
            {
                _outputMeshUVs = new ComputeBuffer(vertexSize, sizeof(float) * 2);
                Shader.SetBuffer(KernelId, "output_uv_array", _outputMeshUVs);
            }
        }

        /// <summary>
        /// This runs the shader.
        /// </summary>
        /// <param name="threads">The amount of parallel tasks.</param>
        public void Execute(int threads)
        {
            Shader.SetInt("maximum", threads);
            if (60000 < threads)
            {
                int factor = Mathf.CeilToInt(threads / 60000f);
                Shader.SetInt("batch", factor);
                threads = 60000;
            }
            else
            {
                Shader.SetInt("batch", 1);
            }
            Shader.Dispatch(KernelId, threads, 1, 1);
        }

        /// <summary>
        /// Executes the shader and returns the computed shader.
        /// </summary>
        /// <param name="threads">The amount of parallel tasks</param>
        /// <param name="mesh">The target for the mesh</param>
        /// <returns></returns>
        internal Mesh GetOutputMesh(int threads, Mesh mesh = null)
        {
            // Run shader.
            Execute(threads);
            // Create new mesh if none was given.
            if (mesh == null) mesh = new Mesh();
            // Set mesh to int32 indexing so more then 64k vertices are allowed.
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            // Replace array of mesh arrays that were given as input.
            if (_inputMeshVertices != null)
            {
                Vector3[] outputVerices = new Vector3[_outputMeshVertices.count];
                _outputMeshVertices.GetData(outputVerices);
                mesh.vertices = outputVerices;
            }
            if (_inputMeshIndexes != null)
            {
                int[] outputIndexes = new int[_outputMeshIndexes.count];
                _outputMeshIndexes.GetData(outputIndexes);
                mesh.triangles = outputIndexes;
            }
            if (_inputMeshUVs != null)
            {
                Vector2[] outputUV = new Vector2[_outputMeshUVs.count];
                _outputMeshUVs.GetData(outputUV);
                mesh.uv = outputUV;
            }
            ClearMemory();
            return mesh;
        }

        private new void ClearMemory()
        {
            base.ClearMemory();
            if (_inputMeshVertices != null) _inputMeshVertices.Dispose();
            if (_inputMeshIndexes != null) _inputMeshIndexes.Dispose();
            if (_inputMeshUVs != null) _inputMeshUVs.Dispose();

            if (_outputMeshVertices != null) _outputMeshVertices.Dispose();
            if (_outputMeshIndexes != null) _outputMeshIndexes.Dispose();
            if (_outputMeshUVs != null) _outputMeshUVs.Dispose();
        }
    }
}