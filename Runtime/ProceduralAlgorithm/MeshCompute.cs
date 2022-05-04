using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PlanetEngine
{
    internal class MeshCompute : Compute
    {
        ComputeBuffer _inputMeshVertices, _inputMeshIndexes, _inputMeshUVs;
        ComputeBuffer _outputMeshVertices, _outputMeshIndexes, _outputMeshUVs;
        internal new void SetKernel(string kernelName) {
            _shader = Resources.Load<ComputeShader>("MeshShaders/MeshShader");
            base.SetKernel(kernelName);
        }

        internal void SetInputMesh(Vector3[] vertices = null, int[] indexes = null, Vector2[] uvs = null)
        {
            // Create vertice buffer and assign to shader.
            if (vertices != null)
            {
                _inputMeshVertices = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
                _inputMeshVertices.SetData(vertices);
                _shader.SetBuffer(_kernelId, "input_vertice_array", _inputMeshVertices);
            }
            // Create index buffer and assign to shader.
            if (indexes != null)
            {
                _inputMeshIndexes = new ComputeBuffer(indexes.Length, sizeof(int));
                _inputMeshIndexes.SetData(indexes);
                _shader.SetBuffer(_kernelId, "input_index_array", _inputMeshIndexes);
            }
            // Create uvv buffer and assign to shader.
            if (uvs != null)
            {
                _inputMeshUVs = new ComputeBuffer(uvs.Length, sizeof(float) * 2);
                _inputMeshUVs.SetData(uvs);
                _shader.SetBuffer(_kernelId, "input_uv_array", _inputMeshUVs);
            }
        }

        internal void SetInputMesh(Mesh mesh)
        {
            SetInputMesh(mesh.vertices, mesh.triangles, mesh.uv);
        }

        // TODO: make compatible with not all arrays active.
        internal void SetOutputMeshAsInput(int vertexSize = 0, int indexSize = 0)
        {
            // Clear old input mesh buffers.
            _inputMeshVertices.Dispose();
            _inputMeshIndexes.Dispose();
            _inputMeshUVs.Dispose();

            // Set the output to the input.
            _inputMeshVertices = _outputMeshVertices;
            _inputMeshIndexes = _outputMeshIndexes;
            _inputMeshUVs = _outputMeshUVs;

            // Add buffers to shader.
            _shader.SetBuffer(_kernelId, "input_vertice_array", _inputMeshVertices);
            _shader.SetBuffer(_kernelId, "input_index_array", _inputMeshIndexes);
            _shader.SetBuffer(_kernelId, "input_uv_array", _inputMeshUVs);

            SetOutputMeshProperties(vertexSize, indexSize);
        }

        internal void SetOutputMeshProperties(int vertexSize = 0, int indexSize = 0)
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
                _shader.SetBuffer(_kernelId, "output_vertice_array", _outputMeshVertices);
            }
            if (_inputMeshIndexes != null)
            {
                _outputMeshIndexes = new ComputeBuffer(indexSize, sizeof(int));
                _shader.SetBuffer(_kernelId, "output_index_array", _outputMeshIndexes);
            }
            if (_inputMeshUVs != null)
            {
                _outputMeshUVs = new ComputeBuffer(vertexSize, sizeof(float) * 2);
                _shader.SetBuffer(_kernelId, "output_uv_array", _outputMeshUVs);
            }
        }

        internal void Execute(int threads)
        {
            _shader.SetInt("maximum", threads);
            if (60000 < threads)
            {
                int factor = Mathf.CeilToInt(threads / 60000f);
                _shader.SetInt("batch", factor);
                threads = 60000;
            }
            else
            {
                _shader.SetInt("batch", 1);
            }
            _shader.Dispatch(_kernelId, threads, 1, 1);
        }

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

        new void ClearMemory()
        {
            base.ClearMemory();
            if(_inputMeshVertices != null) _inputMeshVertices.Dispose();
            if (_inputMeshIndexes != null) _inputMeshIndexes.Dispose();
            if (_inputMeshUVs != null) _inputMeshUVs.Dispose();

            if (_outputMeshVertices != null) _outputMeshVertices.Dispose();
            if (_outputMeshIndexes != null) _outputMeshIndexes.Dispose();
            if (_outputMeshUVs != null) _outputMeshUVs.Dispose();
        }
    }
}