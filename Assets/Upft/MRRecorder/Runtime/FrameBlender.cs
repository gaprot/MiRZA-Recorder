using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Upft.MRRecorder.Runtime.Utils;

namespace Upft.MRRecorder.Runtime
{
    public class FrameBlender : IDisposable
    {
        private readonly BlendMaterial _blendMaterial = new();

        private Texture2D _captureTex;

        public FrameBlender(Resolution resolution)
        {
            _captureTex = new Texture2D(resolution.Width, resolution.Height, TextureFormat.ARGB32, false);
        }

        public unsafe NativeArray<byte> Compose(
            Texture2D cameraTexture,
            Camera sceneCamera,
            float cropRatio = 1f)
        {
            try
            {
                var sceneRT = RenderTexture.GetTemporary(_captureTex.width, _captureTex.height, 0,
                    RenderTextureFormat.ARGB32);
                var physicalRT = RenderTexture.GetTemporary(_captureTex.width, _captureTex.height, 0,
                    RenderTextureFormat.ARGB32);
                var compositeRT = RenderTexture.GetTemporary(_captureTex.width, _captureTex.height, 0,
                    RenderTextureFormat.ARGB32);

                try
                {
                    // フレーム合成処理
                    const float baseScaleFactor = 78f;
                    var sceneFOV = sceneCamera.fieldOfView;
                    var fovRatio = sceneFOV / (baseScaleFactor * cropRatio);

                    var prevTarget = sceneCamera.targetTexture;
                    sceneCamera.targetTexture = sceneRT;
                    sceneCamera.Render();
                    sceneCamera.targetTexture = prevTarget;

                    Graphics.Blit(cameraTexture, physicalRT);
                    _blendMaterial.Blend(sceneRT, physicalRT, fovRatio);
                    Graphics.Blit(sceneRT, compositeRT, _blendMaterial.Material);

                    // ピクセルデータの取得
                    var length = _captureTex.width * _captureTex.height * 4;
                    var nativeData = new NativeArray<byte>(length, Allocator.Persistent);

                    var prevActive = RenderTexture.active;
                    RenderTexture.active = compositeRT;

                    _captureTex.ReadPixels(new Rect(0, 0, _captureTex.width, _captureTex.height), 0, 0);
                    _captureTex.Apply();

                    fixed (void* texPtr = _captureTex.GetRawTextureData())
                    {
                        UnsafeUtility.MemCpy(nativeData.GetUnsafePtr(), texPtr, length);
                    }

                    RenderTexture.active = prevActive;
                    return nativeData;
                }
                finally
                {
                    RenderTexture.ReleaseTemporary(sceneRT);
                    RenderTexture.ReleaseTemporary(physicalRT);
                    RenderTexture.ReleaseTemporary(compositeRT);
                }
            }
            catch (Exception ex)
            {
                RecordLogger.Error("Error capturing composed frame", ex);
                throw;
            }
        }


        public void Dispose()
        {
            if (_captureTex != null)
            {
                UnityEngine.Object.Destroy(_captureTex);
                _captureTex = null;
            }

            _blendMaterial?.Dispose();
        }
    }
}
