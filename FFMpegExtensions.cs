using FFMpegCore;

public static class FFMpegExtensions
{
    public static FFMpegArgumentOptions ApplyAudioSpecificInputSettings(this FFMpegArgumentOptions argumentOptions, VideoSpecificConversionSettings videoSpecificConversionSettings)
    {
        return argumentOptions.Seek(videoSpecificConversionSettings.CutVideoStart)
                              .WithDuration(videoSpecificConversionSettings.FinalVideoDuration);
    }

    public static FFMpegArgumentOptions ApplyAudioSpecificOutputSettings(this FFMpegArgumentOptions argumentOptions, VideoSpecificConversionSettings videoSpecificConversionSettings)
    {
        return argumentOptions;
    }

    public static FFMpegArgumentOptions ApplyVideoSpecificInputSettings(this FFMpegArgumentOptions argumentOptions, VideoSpecificConversionSettings videoSpecificConversionSettings)
    {
        return argumentOptions.Seek(videoSpecificConversionSettings.CutVideoStart)
                              .WithDuration(videoSpecificConversionSettings.FinalVideoDuration);
    }

    public static FFMpegArgumentOptions ApplyVideoSpecificOutputSettings(this FFMpegArgumentOptions argumentOptions, VideoSpecificConversionSettings videoSpecificConversionSettings)
    {
        return argumentOptions;
    }
}