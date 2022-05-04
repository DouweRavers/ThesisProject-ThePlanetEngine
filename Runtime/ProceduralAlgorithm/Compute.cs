using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PlanetEngine
{
    internal abstract class Compute : ScriptableObject
    {
        protected int _kernelId = -1;
        protected ComputeShader _shader;

        // A list containing all buffers that will have to be terminated after the shader has run.
        protected List<ComputeBuffer> _buffers = new List<ComputeBuffer>();

        internal void SetKernel(string kernelName)
        {
            _kernelId = _shader.FindKernel(kernelName);
        }

        internal void AddValue<T>(string name, T value)
        {
            switch (value)
            {
                case int i:
                    _shader.SetInt(name, i);
                    break;
                case float f:
                    _shader.SetFloat(name, f);
                    break;
                case bool b:
                    _shader.SetBool(name, b);
                    break;
                case int[] ari:
                    _shader.SetInts(name, ari);
                    break;
                case float[] arf:
                    _shader.SetFloats(name, arf);
                    break;
                case Vector2 vector2:
                    float[] float2 = new float[] { vector2.x, vector2.y };
                    _shader.SetFloats(name, float2);
                    break;
                case Vector3 vector3:
                    float[] float3 = new float[] { vector3.x, vector3.y, vector3.z };
                    _shader.SetFloats(name, float3);
                    break;
                case Matrix4x4 matrix:
                    _shader.SetMatrix(name, matrix);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        internal void AddArray<T>(string name, T[] array)
        {
            ComputeBuffer buffer = new ComputeBuffer(array.Length, Marshal.SizeOf(array[0]));
            buffer.SetData(array);
            _shader.SetBuffer(_kernelId, name, buffer);
            _buffers.Add(buffer);
        }

        internal void AddTexture(string name, Texture2D texture)
        {
            _shader.SetTexture(_kernelId, name, texture);
        }

        void OnDestroy() { ClearMemory(); }

        protected void ClearMemory()
        {
            foreach (ComputeBuffer buffer in _buffers)
            {
                buffer.Release();
            }
        }
    }
}