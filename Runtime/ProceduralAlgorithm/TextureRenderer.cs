using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PlanetEngine
{
    internal enum ShaderType { BASE, DATA, COLOR, EFFECT, GRADIENT, NONE }
    internal class TextureRenderer : ScriptableObject
    {
        int _kernelId = -1;
        ComputeShader _shader;
        RenderTexture _outputTexture;
        List<ComputeBuffer> _buffers = new List<ComputeBuffer>();

        public void SetKernel(string kernelName, ShaderType shaderType)
        {
            switch (shaderType)
            {
                case ShaderType.BASE:
                    _shader = Resources.Load<ComputeShader>("TextureShaders/BaseTextures");
                    break;
                case ShaderType.DATA:
                    _shader = Resources.Load<ComputeShader>("TextureShaders/DataTextures");
                    break;
                case ShaderType.COLOR:
                    _shader = Resources.Load<ComputeShader>("TextureShaders/ColorTextures");
                    break;
                case ShaderType.EFFECT:
                    _shader = Resources.Load<ComputeShader>("TextureShaders/EffectTextures");
                    break;
                case ShaderType.GRADIENT:
                    _shader = Resources.Load<ComputeShader>("TextureShaders/Gradient2D");
                    break;
                case ShaderType.NONE:
                    _shader = null;
                    break;
            }
            _kernelId = _shader.FindKernel(kernelName);
        }

        public void AddArray<T>(string name, T[] array)
        {
            ComputeBuffer buffer = new ComputeBuffer(array.Length, Marshal.SizeOf(array[0]));
            buffer.SetData(array);
            _shader.SetBuffer(_kernelId, name, buffer);
            _buffers.Add(buffer);
        }

        public void AddValue<T>(string name, T value)
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
                default:
                    throw new NotImplementedException();
            }
        }

        public void AddTexture(string name, Texture2D texture)
        {
            _shader.SetTexture(_kernelId, name, texture);
        }

        public void OutputTextureProperties(string name, int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGB32)
        {
            _outputTexture = new RenderTexture(width, height, 0, format);
            _outputTexture.enableRandomWrite = true;
            _outputTexture.Create();
            _shader.SetTexture(_kernelId, name, _outputTexture);
            _shader.SetInt("width", width);
            _shader.SetInt("height", height);
        }

        public Texture2D GetOutputTexture(TextureFormat format = TextureFormat.RGBA32)
        {
            _shader.Dispatch(_kernelId, _outputTexture.width, _outputTexture.height, 1);
            Texture2D outputTexture = new Texture2D(_outputTexture.width, _outputTexture.height, format, false);
            outputTexture.filterMode = FilterMode.Bilinear;
            RenderTexture.active = _outputTexture;
            outputTexture.ReadPixels(new Rect(0, 0, _outputTexture.width, _outputTexture.height), 0, 0);
            outputTexture.Apply();
            ClearMemory();
            return outputTexture;
        }

        void OnDestroy() { ClearMemory(); }

        void ClearMemory()
        {
            foreach (ComputeBuffer buffer in _buffers)
            {
                buffer.Release();
            }
            RenderTexture.active = null;
            _outputTexture.Release();
        }
    }
}