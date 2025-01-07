using System;
using UnityEngine;

namespace Upft.MRRecorder.Runtime.Utils
{
    public static class PathUtils
    {
        public static string GetDefaultOutputPath() => Application.persistentDataPath;

        public static string GetDefaultVideoFileName() => $"{Application.productName}_Video_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
    }
}
