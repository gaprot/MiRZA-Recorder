using System;

namespace Upft.MRRecorder.Runtime.MediaCodec
{
    public partial class MediaCodec
    {
        [Flags]
        public enum BufferFlag
        {
            /// <summary>
            /// Indicates that the (encoded) buffer contains the data for a key frame.
            /// Added in API level 21.
            /// </summary>
            KeyFrame = 0x1,

            /// <summary>
            /// Indicates that the buffer contains codec initialization / codec specific data 
            /// instead of media data.
            /// Added in API level 16.
            /// </summary>
            CodecConfig = 0x2,

            /// <summary>
            /// Signals the end of stream. No buffers will be available after this,
            /// unless flush() is called.
            /// Added in API level 16.
            /// </summary>
            EndOfStream = 0x4,

            /// <summary>
            /// Indicates that the buffer only contains part of a frame, and the decoder
            /// should batch the data until a buffer without this flag appears before
            /// decoding the frame.
            /// Added in API level 26.
            /// </summary>
            PartialFrame = 0x8,

            /// <summary>
            /// Indicates that the buffer is decoded and updates the internal state of the decoder,
            /// but does not produce any output buffer. Used for frames that are only needed for
            /// reference but should not be displayed.
            /// Added in API level 34.
            /// 
            /// Note: Cannot be used together with EndOfStream flag.
            /// </summary>
            DecodeOnly = 0x20,

            /// <summary>
            /// [Obsolete] Use KeyFrame instead.
            /// This was deprecated in API level 21.
            /// Added in API level 16.
            /// </summary>
            [Obsolete("Use KeyFrame instead.")]
            SyncFrame = KeyFrame
        }

        [Flags]
        public enum ConfigureFlag
        {
            /// <summary>
            /// No flags specified
            /// </summary>
            None = 0,

            /// <summary>
            /// If this codec is to be used as an encoder, pass this flag.
            /// Added in API level 16
            /// </summary>
            Encode = 0x1,

            /// <summary>
            /// If this codec is to be used with LinearBlock and/or HardwareBuffer, pass this flag.
            /// Added in API level 30
            /// </summary>
            UseBlockModel = 0x2,

            /// <summary>
            /// This flag should be used on a secure decoder only. 
            /// MediaCodec configured with this flag does decryption in a separate thread.
            /// Added in API level 34
            /// </summary>
            UseCryptoAsync = 0x4,

            /// <summary>
            /// Configure the codec with a detached output surface.
            /// Added in API level 35
            /// </summary>
            DetachedSurface = 0x8
        }

        public enum InfoCode
        {
            /// <summary>
            /// If a non-negative timeout had been specified in the call to dequeueOutputBuffer,
            /// indicates that the call timed out.
            /// Added in API level 16.
            /// </summary>
            TryAgainLater = -1,

            /// <summary>
            /// The output format has changed, subsequent data will follow the new format.
            /// Use getOutputFormat() to get the new format.
            /// Added in API level 16.
            /// </summary>
            OutputFormatChanged = -2,

            /// <summary>
            /// [Obsolete] The output buffers have changed.
            /// This return value can be ignored as getOutputBuffers() has been deprecated.
            /// Client should request a current buffer using one of the get-buffer or 
            /// get-image methods each time one has been dequeued.
            /// Added in API level 16, deprecated in API level 21.
            /// </summary>
            [Obsolete("Use get-buffer or get-image methods to obtain current buffer instead.")]
            OutputBuffersChanged = -3
        }
    }
}
