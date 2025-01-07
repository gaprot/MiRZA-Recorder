using System;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Upft.MRRecorder.Runtime.MediaCodec;
using Upft.MRRecorder.Runtime.Utils;

namespace Upft.MRRecorder.Runtime
{
    public sealed class VideoRecorder : IDisposable
    {
        public float CropRatio { get; set; } = 1f;

        private readonly ARCameraManager _cameraManager;
        private readonly Camera _sceneCamera;

        private VideoEncodingOptions _options;
        private FrameBlender _frameBlender;
        private VideoEncoder _encoder;
        private CancellationTokenSource _internalCts = new();
        private RecordingState _state = new();
        private bool _isDisposed;
        private const long MICROSECONDS_PER_SECOND = 1_000_000;

        public bool IsEncoding => _encoder is { IsEncoding: true };
        public bool IsRecording => _state.IsRecording;

        public VideoRecorder(
            ARCameraManager cameraManager,
            Camera sceneCamera)
        {
            _cameraManager = cameraManager;
            _sceneCamera = sceneCamera;
            _state = default;
        }

        public void StartRecording(VideoEncodingOptions options)
        {
            if (_state.IsRecording || IsEncoding)
            {
                return;
            }

            _options = options;
            _frameBlender = new(options.Resolution);

            StartEncoding();
            _cameraManager.frameReceived += OnFrameReceived;

            _state.Start();

            RecordLogger.Info($"Started recording.");
        }

        private void OnFrameReceived(ARCameraFrameEventArgs args)
        {
            if (!_state.IsRecording) return;

            if (!_cameraManager.TryAcquireLatestCpuImage(out var image))
            {
                RecordLogger.Error($"Failed to acquire latest CPU image");
                return;
            }

            var timestamp = (long)(image.timestamp * MICROSECONDS_PER_SECOND);
            _state.IncrementFrameCount();
            try
            {
                var targetDimensions = new Vector2Int(_options.Resolution.Width, _options.Resolution.Height);

                var cropWidth = Mathf.RoundToInt(image.width * CropRatio);
                var cropHeight = Mathf.RoundToInt(image.height * CropRatio);

                // 必要な出力サイズより小さくならないように制限
                var inputWidth = Mathf.Max(cropWidth, targetDimensions.x);
                var inputHeight = Mathf.Max(cropHeight, targetDimensions.y);

                var centerX = (image.width - inputWidth) / 2;
                var centerY = (image.height - inputHeight) / 2;

                var inputRect = new RectInt(
                    centerX,
                    centerY,
                    inputWidth,
                    inputHeight
                );

                var outputDimensions = new Vector2Int(
                    Mathf.Min(targetDimensions.x, inputWidth),
                    Mathf.Min(targetDimensions.y, inputHeight)
                );

                image.ConvertAsync(
                    conversionParams: new(image, TextureFormat.RGBA32)
                    {
                        inputRect = inputRect,
                        outputDimensions = outputDimensions,
                        transformation = XRCpuImage.Transformation.None
                    },
                    onComplete: (status, param, data) => OnConverted(status, outputDimensions, data, timestamp));
            }
            catch (Exception ex)
            {
                RecordLogger.Error("Error capturing frame", ex);
            }
            finally
            {
                image.Dispose();
            }
        }

        private void OnConverted(
            XRCpuImage.AsyncConversionStatus status,
            Vector2Int outputDimensions,
            NativeArray<byte> data,
            long timestamp)
        {
            Texture2D camTexture = null;
            try
            {
                if (status != XRCpuImage.AsyncConversionStatus.Ready) return;

                camTexture = new Texture2D(
                    width: outputDimensions.x,
                    height: outputDimensions.y,
                    textureFormat: TextureFormat.RGBA32,
                    mipChain: false);
                using var rawData = camTexture.GetRawTextureData<byte>();
                data.CopyTo(rawData);
                camTexture.Apply();

                var frameData = _frameBlender.Compose(camTexture, _sceneCamera, CropRatio);
                _encoder.ProcessFrame(frameData, timestamp);
            }
            finally
            {
                if (camTexture != null)
                {
                    UnityEngine.Object.Destroy(camTexture);
                }

                if (data.IsCreated)
                {
                    data.Dispose();
                }
            }
        }

        private void StartEncoding()
        {
            try
            {
                _encoder = new();
                _encoder.StartEncoding(_options);
            }
            catch (OperationCanceledException)
            {
                RecordLogger.Info("Encoding task cancelled");
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"Error in encoding task: {ex.Message}", ex);
                throw;
            }
        }

        public void StopRecording()
        {
            if (!_state.IsRecording) return;

            try
            {
                // フレームの取得を停止
                var duration = _state.Duration;
                _state.IsRecording = false;
                _cameraManager.frameReceived -= OnFrameReceived;

                _encoder.Complete();
                RecordLogger.Info($"Recording stopped. Duration: {duration.TotalSeconds:F2} sec");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"Error stopping recording: {ex.Message}", ex);
            }
            finally
            {
                _frameBlender?.Dispose();
                _frameBlender = null;
                _internalCts?.Dispose();
                _internalCts = null;
                _state.Reset();
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _internalCts?.Dispose();
            _frameBlender?.Dispose();

            _state.Reset();
            _isDisposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
