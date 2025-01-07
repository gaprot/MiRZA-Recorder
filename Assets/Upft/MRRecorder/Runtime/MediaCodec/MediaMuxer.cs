using System;
using UnityEngine;
using Upft.MRRecorder.Runtime.Utils;

namespace Upft.MRRecorder.Runtime.MediaCodec
{
    public class MediaMuxer : IDisposable
    {
        public bool IsStarted => _isStarted;
        private AndroidJavaObject _instance;
        private bool _isStarted;

        public MediaMuxer(string outputPath)
        {
            _instance = CreateMediaMuxer(outputPath);
        }

        private static AndroidJavaObject CreateMediaMuxer(string outputPath)
        {
            try
            {
                return new AndroidJavaObject("android.media.MediaMuxer",
                    outputPath,
                    OutputFormat.MUXER_OUTPUT_MPEG_4);
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"Failed to create MediaMuxer: {ex.Message}", ex);
                throw;
            }
        }


        public void Start()
        {
            try
            {
                _instance.Call("start");
                _isStarted = true;
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"StartMuxer failed: {ex.Message}", ex);
                throw;
            }
        }

        public void Stop()
        {
            if (!_isStarted) return;

            try
            {
                _instance.Call("stop");
                _isStarted = false;
            }
            catch (Exception ex)
            {
                RecordLogger.Warning($"Error stopping muxer: {ex.Message}");
            }
        }

        public void Release()
        {
            try
            {
                _instance?.Call("release");
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"Error releasing muxer: {ex.Message}");
            }
        }

        public int AddTrack(AndroidJavaObject format)
        {
            try
            {
                return _instance.Call<int>("addTrack", format);
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"AddTrack failed: {ex.Message}", ex);
                throw;
            }
        }


        public void WriteSampleData(int trackIndex, AndroidJavaObject buffer, AndroidJavaObject bufferInfo)
        {
            if (!_isStarted)
            {
                throw new InvalidOperationException("Muxer is not started");
            }

            try
            {
                _instance.Call("writeSampleData", trackIndex, buffer, bufferInfo);
            }
            catch (Exception ex)
            {
                RecordLogger.Error($"WriteSampleData failed: {ex.Message}", ex);
                throw;
            }
        }

        public void Dispose()
        {
            _instance?.Dispose();
            _instance = null;
        }

        /// <summary>
        /// Output format constants for MediaMuxer
        /// </summary>
        public static class OutputFormat
        {
            /// <summary>
            /// MPEG4 media file format
            /// Added in API level 18
            /// </summary>
            public const int MUXER_OUTPUT_MPEG_4 = 0;

            /// <summary>
            /// WEBM media file format
            /// Added in API level 21
            /// </summary>
            public const int MUXER_OUTPUT_WEBM = 1;

            /// <summary>
            /// 3GPP media file format
            /// Added in API level 26
            /// </summary>
            public const int MUXER_OUTPUT_3GPP = 2;

            /// <summary>
            /// HEIF media file format
            /// Added in API level 28
            /// </summary>
            public const int MUXER_OUTPUT_HEIF = 3;

            /// <summary>
            /// Ogg media file format
            /// Added in API level 29
            /// </summary>
            public const int MUXER_OUTPUT_OGG = 4;
        }
    }
}
