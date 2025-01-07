using System;
using UnityEngine;

namespace Upft.MRRecorder.Runtime
{
    public class BlendMaterial : IDisposable
    {
        private const string ShaderName = "Hidden/MRBlend";
        private readonly int _mainTexProperty = Shader.PropertyToID("_MainTex");
        private readonly int _cameraTexProperty = Shader.PropertyToID("_CameraTex");
        private readonly int _blendFactorProperty = Shader.PropertyToID("_BlendFactor");
        private readonly int _sceneScale = Shader.PropertyToID("_SceneScale");

        private Material _blendMaterial;
        private float _blendFactor;
        public Material Material => _blendMaterial;

        public BlendMaterial()
        {
            var shader = Resources.Load("MRBlend") as Shader;
            if (shader == null)
            {
                throw new InvalidOperationException($"Shader '{ShaderName}' not found");
            }

            _blendMaterial = new Material(shader);
        }

        public void Blend(Texture sceneTexture, Texture cameraTexture, float fovRatio)
        {
            _blendMaterial.SetTexture(_mainTexProperty, sceneTexture);
            _blendMaterial.SetTexture(_cameraTexProperty, cameraTexture);
            _blendMaterial.SetVector(_sceneScale, new Vector4(fovRatio, fovRatio, 0, 0));
        }

        public void Dispose()
        {
            if (_blendMaterial != null)
            {
                UnityEngine.Object.Destroy(_blendMaterial);
                _blendMaterial = null;
            }
        }
    }
}
