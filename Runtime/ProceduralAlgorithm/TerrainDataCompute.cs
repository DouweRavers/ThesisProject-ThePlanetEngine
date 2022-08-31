using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PlanetEngine
{
    internal class TerrainDataCompute : Compute
    {
        int width, height, depth;
        ComputeBuffer _outputArray;

        internal new void SetKernel(string kernelName)
        {
            Shader = Resources.Load<ComputeShader>("TerrainDataShaders/TerrainDataShader");
            base.SetKernel(kernelName);
        }

        internal void SetInputArray2D<T>(T[,] array2D)
        {
            T[] array1D = new T[array2D.GetLength(0) * array2D.GetLength(1)];
            for (int i = 0; i < array1D.Length; i++)
            {
                int x = i % array2D.GetLength(0);
                int y = i / array2D.GetLength(0);

                array1D[i] = array2D[x, y];
            }
            AddArray("base_array_in", array1D);
        }

        internal void SetOutputDataProperties<T>(T type, string name, int width, int height, int depth)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            _outputArray = new ComputeBuffer(height * width * depth, GetTypeSize(type));
            Shader.SetBuffer(KernelId, name, _outputArray);
        }

        internal void SetOutputDataProperties<T>(T type, string name, int width, int height)
        {
            this.width = width;
            this.height = height;
            Shader.SetInt("width", width);
            Shader.SetInt("height", height);
            _outputArray = new ComputeBuffer(height * width, Marshal.SizeOf(type));
            Shader.SetBuffer(KernelId, name, _outputArray);
        }

        internal void Execute()
        {
            int threads = _outputArray.count;
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


        internal T[] GetOutputArray<T>()
        {
            // Run shader.
            Execute();
            T[] output = new T[_outputArray.count];
            _outputArray.GetData(output);
            ClearMemory();
            return output;
        }

        internal T[,] GetOutputArray2D<T>()
        {
            T[] buffer = GetOutputArray<T>();
            // TODO: forloop over computed output. => better way?
            T[,] output = new T[width, height];
            for (int i = 0; i < buffer.Length; i++)
            {
                int x = i % width;
                int y = i / width;
                output[x, y] = buffer[i];
            }
            return output;
        }

        internal T[,,] GetOutputArray3D<T>()
        {
            T[] buffer = GetOutputArray<T>();
            // TODO: forloop over computed output. => better way?
            T[,,] output = new T[width, height, depth];
            for (int i = 0; i < buffer.Length; i++)
            {
                int x = i % width;
                int y = i / width;
                // TODO: make 3D instead of 2D
                int z = 0;
                output[x, y, z] = buffer[i];
            }
            return output;
        }

        new void ClearMemory()
        {
            base.ClearMemory();
            if (_outputArray != null) _outputArray.Dispose();
        }

        int GetTypeSize<T>(T type)
        {
            int size = 0;
            switch (type)
            {
                case float f:
                    size = sizeof(float);
                    break;
                case Vector3 vec:
                    size = sizeof(float) * 3;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return size;
        }
    }
}