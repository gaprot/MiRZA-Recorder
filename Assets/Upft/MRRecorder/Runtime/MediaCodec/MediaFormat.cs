using System;
using UnityEngine;

namespace Upft.MRRecorder.Runtime.MediaCodec
{
    public class MediaFormat : IDisposable
    {
        public const string MIMETYPE_VIDEO_AVC = "video/avc"; // H.264/AVC
        public const int DEFAULT_BIT_RATE = 8_000_000; // 8Mbps

        public AndroidJavaObject Instance { get; } = new("android.media.MediaFormat");

        public void SetString(string key, string value) => Instance.Call("setString", key, value);
        public void SetInteger(string key, int value) => Instance.Call("setInteger", key, value);

        public void Dispose()
        {
            Instance?.Dispose();
        }
    }
}
