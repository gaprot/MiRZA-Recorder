using System;
using UnityEngine;
using Upft.MRRecorder.Runtime.Utils;

namespace Upft.MRRecorder.Runtime.MediaCodec
{
    public partial class MediaCodec : IDisposable
    {
        public const int COLOR_FORMAT_YUV420_FLEXIBLE = 0x7F420888;

        private AndroidJavaObject _codec;
        private AndroidJavaObject _bufferInfo;

        private MediaCodec(AndroidJavaObject codec)
        {
            _codec = codec;
        }

        public AndroidJavaObject BufferInfo => _bufferInfo;

        public static MediaCodec CreateEncoderByType(string mime)
        {
            using var mediaCodecClass = new AndroidJavaClass("android.media.MediaCodec");
            var codec = mediaCodecClass.CallStatic<AndroidJavaObject>("createEncoderByType", mime);
            return new MediaCodec(codec);
        }

        public void Configure(
            MediaFormat format,
            AndroidJavaObject surface,
            AndroidJavaObject mediaCrypto,
            ConfigureFlag flags)
        {
            _codec?.Call("configure", format.Instance, surface, mediaCrypto, (int)flags);
        }

        public void Start()
        {
            try
            {
                _bufferInfo = new AndroidJavaObject("android.media.MediaCodec$BufferInfo");
                _codec?.Call("start");
                RecordLogger.Debug("MediaCodec started");
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"MediaCodec start failed: {ex.Message}", ex);
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                _codec?.Call("stop");
                RecordLogger.Debug("MediaCodec stopped");
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"MediaCodec stop failed: {ex.Message}", ex);
                throw;
            }
        }

        public void Release()
        {
            try
            {
                _codec?.Call("release");
                RecordLogger.Debug("MediaCodec released");
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"MediaCodec release failed: {ex.Message}", ex);
                throw;
            }
        }

        public int DequeueInputBuffer(long timeoutUs)
        {
            try
            {
                return _codec.Call<int>("dequeueInputBuffer", timeoutUs);
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"DequeueInputBuffer failed: {ex.Message}", ex);
                throw;
            }
        }

        public AndroidJavaObject GetInputBuffer(int index)
        {
            try
            {
                return _codec.Call<AndroidJavaObject>("getInputBuffer", index);
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"GetInputBuffer failed: {ex.Message}", ex);
                throw;
            }
        }

        public void QueueInputBuffer(int index, int offset, int size, long presentationTimeUs, int flags)
        {
            try
            {
                _codec.Call("queueInputBuffer", index, offset, size, presentationTimeUs, flags);
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"QueueInputBuffer failed: {ex.Message}", ex);
                throw;
            }
        }

        public int DequeueOutputBuffer(AndroidJavaObject info, int timeoutUs)
        {
            try
            {
                return _codec.Call<int>("dequeueOutputBuffer", info, (long)timeoutUs);
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"DequeueOutputBuffer failed: {ex.Message}", ex);
                throw;
            }
        }

        public AndroidJavaObject GetOutputBuffer(int index)
        {
            try
            {
                var buffer = _codec.Call<AndroidJavaObject>("getOutputBuffer", index);
                if (buffer == null)
                {
                    throw new InvalidOperationException($"Failed to get output buffer at index {index}");
                }

                return buffer;
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"GetOutputBuffer failed: {ex.Message}", ex);
                throw;
            }
        }

        public AndroidJavaObject GetOutputFormat()
        {
            try
            {
                return _codec.Call<AndroidJavaObject>("getOutputFormat");
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"GetOutputFormat failed: {ex.Message}", ex);
                throw;
            }
        }

        public void ReleaseOutputBuffer(int index, bool render)
        {
            try
            {
                _codec.Call("releaseOutputBuffer", index, render);
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"ReleaseOutputBuffer failed: {ex.Message}", ex);
                throw;
            }
        }

        private void ReleaseUnmanagedResources()
        {
            {
                if (_codec != null)
                {
                    try
                    {
                        _codec.Call("release");
                    }
                    catch (Exception ex)
                    {
                        RecordLogger.Warning($"Error releasing codec: {ex.Message}");
                    }
                    finally
                    {
                        _codec.Dispose();
                        _codec = null;
                    }
                }

                if (_bufferInfo != null)
                {
                    _bufferInfo.Dispose();
                    _bufferInfo = null;
                }
            }
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~MediaCodec()
        {
            ReleaseUnmanagedResources();
        }
    }
}
