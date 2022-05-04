using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PlanetEngine
{
    internal enum ShaderType { BASE, DATA, COLOR, EFFECT, GRADIENT, NONE }
    internal class TextureCompute : Compute
    {
        RenderTexture _outputTexture;

        internal void SetKernel(string kernelName, ShaderType shaderType)
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
            base.SetKernel(kernelName);
        }


        internal void OutputTextureProperties(string name, int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGB32)
        {
            _outputTexture = new RenderTexture(width, height, 0, format);
            _outputTexture.enableRandomWrite = true;
            _outputTexture.Create();
            _shader.SetTexture(_kernelId, name, _outputTexture);
            _shader.SetInt("width", width-1);
            _shader.SetInt("height", height-1);
        }

        internal Texture2D GetOutputTexture(TextureFormat format = TextureFormat.RGBA32)
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

        new void ClearMemory()
        {
            base.ClearMemory();
            RenderTexture.active = null;
            _outputTexture.Release();
        }
    }
}