using System;
using UnityEngine;

namespace Upft.MRRecorder.Runtime.Utils
{
    public static class PathUtils
    {
        public static string GetDefaultOutputPath() => Application.persistentDataPath;

        public static string GetDefaultVideoFileName() => $"{Application.productName}_Video_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
        
        
        #if MR_RECORDER_USE_NATIVE_GALLERY
        public static string GetDefaultAlbumName() => Application.productName;
        #endif
    }
}
