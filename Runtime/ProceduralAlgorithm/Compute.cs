using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PlanetEngine
{
    internal abstract class Compute : ScriptableObject
    {
        /// <summary>
        /// The id of the kernel used by this compute interface.
        /// </summary>
        protected int KernelId { get; set; } = -1;

        /// <summary>
        /// A refernece to the shader used.
        /// </summary>
        protected ComputeShader Shader { get; set; }

        /// <summary>
        /// A list containing all buffers that will have to be terminated after the shader has run.
        /// </summary>
        protected List<ComputeBuffer> Buffers { get; set; } = new List<ComputeBuffer>();

        void OnDestroy() { ClearMemory(); }

        /// <summary>
        /// Searches for the kernel with given name.
        /// </summary>
        public void SetKernel(string kernelName)
        {
            KernelId = Shader.FindKernel(kernelName);
        }

        /// <summary>
        /// Adds a value to the shader.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="name">The name of the value in the shader</param>
        /// <param name="value">the value itself.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddValue<T>(string name, T value)
        {
            switch (value)
            {
                case int i:
                    Shader.SetInt(name, i);
                    break;
                case float f:
                    Shader.SetFloat(name, f);
                    break;
                case bool b:
                    Shader.SetBool(name, b);
                    break;
                case int[] ari:
                    Shader.SetInts(name, ari);
                    break;
                case float[] arf:
                    Shader.SetFloats(name, arf);
                    break;
                case Vector2 vector2:
                    float[] float2 = new float[] { vector2.x, vector2.y };
                    Shader.SetFloats(name, float2);
                    break;
                case Vector3 vector3:
                    float[] float3 = new float[] { vector3.x, vector3.y, vector3.z };
                    Shader.SetFloats(name, float3);
                    break;
                case Matrix4x4 matrix:
                    Shader.SetMatrix(name, matrix);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Adds an array to the shader.
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="name">The name of the array in the shader</param>
        /// <param name="array">the array itself.</param>
        public void AddArray<T>(string name, T[] array)
        {
            ComputeBuffer buffer = new ComputeBuffer(array.Length, Marshal.SizeOf(array[0]));
            buffer.SetData(array);
            Shader.SetBuffer(KernelId, name, buffer);
            Buffers.Add(buffer);
        }

        /// <summary>
        /// Adds an texture to the shader.
        /// </summary>
        /// <param name="name">The name of the texture in the shader</param>
        /// <param name="texture">the texture itself.</param>
        public void AddTexture(string name, Texture2D texture)
        {
            Shader.SetTexture(KernelId, name, texture);
        }

        /// <summary>
        /// Clears all memory in shader.
        /// </summary>
        protected void ClearMemory()
        {
            foreach (ComputeBuffer buffer in Buffers)
            {
                buffer.Release();
            }
        }
    }
}