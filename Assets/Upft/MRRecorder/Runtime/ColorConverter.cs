using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Upft.MRRecorder.Runtime
{
    public class ColorConverter
    {
        public static unsafe NativeArray<byte> ConvertArgb32ToYuv420(NativeArray<byte> argbData,
            Resolution resolution)
        {
            try
            {
                int yuvSize = resolution.Width * resolution.Height * 3 / 2;
                var yuvData = new NativeArray<byte>(yuvSize, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);

                byte* yuvPtr = (byte*)yuvData.GetUnsafePtr();
                byte* argbPtr = (byte*)argbData.GetUnsafePtr();

                int yIndex = 0;
                int uvIndex = resolution.Width * resolution.Height;

                for (int j = resolution.Height - 1; j >= 0; j--)
                {
                    for (int i = 0; i < resolution.Width; i++)
                    {
                        int argbIndex = (j * resolution.Width + i) * 4;
                        byte a = argbPtr[argbIndex + 0];
                        byte r = argbPtr[argbIndex + 1];
                        byte g = argbPtr[argbIndex + 2];
                        byte b = argbPtr[argbIndex + 3];

                        yuvPtr[yIndex++] = (byte)(
                            (0.299 * r + 0.587 * g + 0.114 * b));

                        if (i % 2 == 0 && j % 2 == 1)
                        {
                            yuvPtr[uvIndex++] = (byte)(
                                (-0.169 * r - 0.331 * g + 0.5 * b) + 128);
                            yuvPtr[uvIndex++] = (byte)(
                                (0.5 * r - 0.419 * g - 0.081 * b) + 128);
                        }
                    }
                }

                return yuvData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert ARGB to YUV420: {ex.Message}");
                throw;
            }
        }
    }
}
