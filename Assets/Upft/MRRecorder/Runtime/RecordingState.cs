using System;

namespace Upft.MRRecorder.Runtime
{
    public struct RecordingState
    {
        public bool IsRecording { get; set; }
        public int FrameCount { get; private set; }
        public DateTime StartTime { get; private set; }

        public TimeSpan Duration => IsRecording
            ? DateTime.Now - StartTime
            : TimeSpan.Zero;
        

        public void IncrementFrameCount()
        {
            FrameCount++;
        }
        
        public void Start()
        {
            IsRecording = true;
            FrameCount = 0;
            StartTime = DateTime.Now;
        }

        public void Reset()
        {
            IsRecording = false;
            FrameCount = 0;
            StartTime = default;
        }

    }
}
