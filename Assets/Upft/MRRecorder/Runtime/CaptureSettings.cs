namespace Upft.MRRecorder.Runtime
{
    public record VideoEncodingOptions(
        Resolution Resolution,
        string OutputDir,
        string FileName
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
            ResolutionQuality.Low => new Resolution(640, 360),
            ResolutionQuality.Medium => new Resolution(1280, 720),
            ResolutionQuality.High => new Resolution(1920, 1080),
            _ => new Resolution(1280, 720)
        };
    }
}
