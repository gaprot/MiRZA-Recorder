using UnityEngine;

namespace Upft.MRRecorder.Runtime.MediaCodec
{
    public static class AndroidByteBufferExtensions
    {
        public static AndroidJavaObject Clear(this AndroidJavaObject byteBuffer) =>
            byteBuffer.Call<AndroidJavaObject>("clear");

        public static AndroidJavaObject Rewind(this AndroidJavaObject byteBuffer) =>
            byteBuffer.Call<AndroidJavaObject>("rewind");

        public static AndroidJavaObject Put(this AndroidJavaObject byteBuffer, byte[] src) =>
            byteBuffer.Call<AndroidJavaObject>("put", src);

        public static AndroidJavaObject Put(this AndroidJavaObject byteBuffer, byte[] src, int offset, int length) =>
            byteBuffer.Call<AndroidJavaObject>("put", src, offset, length);

        public static AndroidJavaObject Flip(this AndroidJavaObject byteBuffer) =>
            byteBuffer.Call<AndroidJavaObject>("flip");

        public static AndroidJavaObject Position(this AndroidJavaObject byteBuffer, int newPosition) =>
            byteBuffer.Call<AndroidJavaObject>("position", newPosition);

        public static AndroidJavaObject Limit(this AndroidJavaObject byteBuffer, int newLimit) =>
            byteBuffer.Call<AndroidJavaObject>("limit", newLimit);

        public static int GetCapacity(this AndroidJavaObject byteBuffer) =>
            byteBuffer.Call<int>("capacity");
    }

    public static class BufferInfoExtensions
    {
        public static int GetOffset(this AndroidJavaObject bufferInfo) => bufferInfo.Get<int>("offset");
        public static int GetSize(this AndroidJavaObject bufferInfo) => bufferInfo.Get<int>("size");

        public static long GetPresentationTimeUs(this AndroidJavaObject bufferInfo) =>
            bufferInfo.Get<long>("presentationTimeUs");

        public static MediaCodec.BufferFlag GetFlags(this AndroidJavaObject bufferInfo) =>
            (MediaCodec.BufferFlag)bufferInfo.Get<int>("flags");

        public static void Set(this AndroidJavaObject bufferInfo, int offset, int size, long presentationTimeUs,
            int flags) =>
            bufferInfo.Call("set", offset, size, presentationTimeUs, flags);
    }
}
