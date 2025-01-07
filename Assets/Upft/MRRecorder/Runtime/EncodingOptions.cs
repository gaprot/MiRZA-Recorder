namespace Upft.MRRecorder.Runtime
{
    public record VideoEncodingOptions(
        Resolution Resolution,
        string OutputDir,
        string FileName,
        int? Bitrate = null,
        bool LowLatencyMode = false,
        int KeyFrameInterval = 1,
        int Priority = 0
    );

    public record Resolution(
        int Width,
        int Height,
        int FrameRate = 30
    );

    public enum ResolutionQuality
    {
        Low,
        Medium,
        High
    }

    public static class ResolutionQualityExtensions
    {
        public static Resolution ToResolution(this ResolutionQuality quality) => quality switch
        {
            ResolutionQuality.Low => new Resolution(720, 480),
            ResolutionQuality.Medium => new Resolution(1280, 720),
            ResolutionQuality.High => new Resolution(1920, 1080),
            _ => new Resolution(1280, 720)
        };
        public static int CalculateDefaultBitrate(this VideoEncodingOptions options) =>
            (int)(options.Resolution.Width * options.Resolution.Height * options.Resolution.FrameRate * .145f);
            // FullHD 9Mbps = 9,000,000 = 1920 * 1080 * 30 * .145f
    }
}
