using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Upft.MRRecorder.Runtime.Utils;

namespace Upft.MRRecorder.Runtime.MediaCodec
{
    public class VideoEncoder : IDisposable
    {
        private const int TIMEOUT_US = 10_000; // 10ms

        public bool IsEncoding => _isEncoding;

        private MediaCodec _codec;
        private MediaMuxer _muxer;
        private Resolution _resolution;
        private bool _isDisposed;
        private bool _isEncoding;

        public void StartEncoding(VideoEncodingOptions options)
        {
            RecordLogger.Debug($"Start Encoding");
            if (_isEncoding)
            {
                RecordLogger.Warning($"Already encoding");
                return;
            }

            try
            {
                var path = Path.Combine(options.OutputDir, options.FileName);
                _resolution = options.Resolution;

                using var format = new MediaFormat();
                format.SetString("mime", MediaFormat.MIMETYPE_VIDEO_AVC);
                format.SetInteger("width", options.Resolution.Width);
                format.SetInteger("height", options.Resolution.Height);
                format.SetInteger("frame-rate", options.Resolution.FrameRate);
                format.SetInteger("bitrate", options.Bitrate ?? options.CalculateDefaultBitrate());
                format.SetInteger("i-frame-interval", options.KeyFrameInterval);
                format.SetInteger("color-format", MediaCodec.COLOR_FORMAT_YUV420_FLEXIBLE);
                format.SetInteger("priority", options.Priority);
                format.SetInteger("low-latency", options.LowLatencyMode ? 1 : 0);

                _codec = MediaCodec.CreateEncoderByType(MediaFormat.MIMETYPE_VIDEO_AVC);
                _codec.Configure(format, null, null, MediaCodec.ConfigureFlag.Encode);
                _codec.Start();

                _muxer = new MediaMuxer(path);
                _isEncoding = true;
                RecordLogger.Debug($"Started encoding to: {path}");
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"Failed to start encoding: {ex.Message}", ex);
                throw;
            }
        }

        public void Complete()
        {
            if (!_isEncoding)
            {
                RecordLogger.Debug("Not encoding");
                return;
            }

            try
            {
                SendEndOfStream();

                RecordLogger.Debug("Video encoding completed successfully");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"Failed to finish encoding: {ex.Message}", ex);
                throw;
            }
            finally
            {
                CleanupResources();
                _isEncoding = false;
            }
        }


        public void ProcessFrame(NativeArray<byte> frameData, long timestamp)
        {
            try
            {
                if (!_isEncoding || _codec == null) return;
                using var tempYuvData = ColorConverter.ConvertArgb32ToYuv420(frameData, _resolution);
                var yuvData = new NativeArray<byte>(tempYuvData.Length, Allocator.Persistent);
                yuvData.CopyFrom(tempYuvData);

                try
                {
                    if (_codec == null)
                    {
                        return;
                    }

                    // 入力バッファを取得
                    var inputBufferId = _codec.DequeueInputBuffer(TIMEOUT_US);
                    if (inputBufferId < 0) return;

                    var inputBuffer = _codec.GetInputBuffer(inputBufferId);
                    if (inputBuffer == null) return;
                    inputBuffer.Clear();

                    unsafe
                    {
                        var rawBuffer = inputBuffer.GetRawObject();
                        var dstPtr = AndroidJNI.GetDirectBufferAddress(rawBuffer);
                        var srcPtr = (sbyte*)yuvData.GetUnsafePtr();

                        if (dstPtr != null)
                        {
                            // バッファサイズを確認
                            var capacity = AndroidJNI.GetDirectBufferCapacity(rawBuffer);
                            if (capacity >= yuvData.Length)
                            {
                                Buffer.MemoryCopy(
                                    srcPtr,
                                    dstPtr,
                                    capacity,
                                    yuvData.Length);
                            }
                            else
                            {
                                RecordLogger.Error($"Buffer too small: {capacity} < {yuvData.Length}");
                            }
                        }
                    }

                    inputBuffer.Limit(yuvData.Length);
                    inputBuffer.Rewind();

                    // フレームを送信
                    _codec.QueueInputBuffer(
                        index: inputBufferId,
                        offset: 0,
                        size: yuvData.Length,
                        presentationTimeUs: timestamp,
                        flags: 0);


                    DrainEncoder();
                }
                catch (NullReferenceException ex)
                {
                    RecordLogger.Error($"Failed to encode frame. _codec is null?: {_codec == null}: {ex.Message}", ex);
                }
                finally
                {
                    if (yuvData.IsCreated)
                    {
                        yuvData.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"Failed to encode frame: {ex.Message}", ex);
                throw;
            }
            finally
            {
                if (frameData.IsCreated)
                {
                    frameData.Dispose();
                }
            }
        }

        private void DrainEncoder()
        {
            if (!_isEncoding || _codec == null)
            {
                RecordLogger.Debug($"DrainEncoder: Not encoding");
                return;
            }


            try
            {
                // 出力バッファを取得
                var outputBufferId = _codec.DequeueOutputBuffer(_codec.BufferInfo, TIMEOUT_US);

                switch (outputBufferId)
                {
                    case (int)MediaCodec.InfoCode.OutputFormatChanged:
                        // フォーマット変更
                        if (!_muxer.IsStarted)
                        {
                            var format = _codec.GetOutputFormat();
                            var trackIndex = _muxer.AddTrack(format);
                            _muxer.Start();
                        }

                        break;

                    case >= 0:
                        var outputBuffer = _codec.GetOutputBuffer(outputBufferId);
                        var size = _codec.BufferInfo.GetSize();

                        if (outputBuffer != null &&
                            size != 0 &&
                            _muxer.IsStarted)
                        {
                            var offset = _codec.BufferInfo.GetOffset();
                            outputBuffer.Position(offset);
                            outputBuffer.Limit(offset + size);

                            _muxer.WriteSampleData(0, outputBuffer, _codec.BufferInfo);
                        }

                        _codec.ReleaseOutputBuffer(outputBufferId, false);

                        var flags = _codec.BufferInfo.GetFlags();

                        if (!flags.HasFlag(MediaCodec.BufferFlag.EndOfStream))
                        {
                            return;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"Error during encoder drain: {ex.Message}", ex);
                throw;
            }
        }


        private void SendEndOfStream()
        {
            var inputBufferId = _codec.DequeueInputBuffer(TIMEOUT_US);
            if (inputBufferId >= 0)
            {
                _codec.QueueInputBuffer(inputBufferId, 0, 0, 0, (int)MediaCodec.BufferFlag.EndOfStream);
            }

            while (true)
            {
                var outputBufferId = _codec.DequeueOutputBuffer(_codec.BufferInfo, TIMEOUT_US);
                if (outputBufferId >= 0)
                {
                    var outputBuffer = _codec.GetOutputBuffer(outputBufferId);

                    var flags = _codec.BufferInfo.GetFlags();
                    if (!flags.HasFlag(MediaCodec.BufferFlag.EndOfStream))
                    {
                        break;
                    }

                    _codec.ReleaseOutputBuffer(outputBufferId, false);
                }
                else if (outputBufferId == (int)MediaCodec.InfoCode.OutputFormatChanged)
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
        }

        private void CleanupResources()
        {
            try
            {
                if (_muxer != null)
                {
                    _muxer.Stop();
                    _muxer.Release();
                    _muxer.Dispose();
                    _muxer = null;
                }

                if (_codec != null)
                {
                    _codec.Stop();
                    _codec.Release();
                    _codec.Dispose();
                    _codec = null;
                }

                _isEncoding = false;
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"Error during cleanup: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            CleanupResources();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
