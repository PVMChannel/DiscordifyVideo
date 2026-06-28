using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using DiscordifyVideo.Models;
using FFMpegCore;
using FFMpegCore.Enums;

public class VideoConverter
{
    public VideoConverter()
    {
        
    }

    public async Task<string> RunConvert(IProgress<ConvertionProgress> progress, string source, IVideoFormat videoFormat, VideoSpecificConversionSettings videoSpecificConversionSettings, string? outputFileName = null, string? outputDirectory = null)
    {
        ConvertionProgress convertionProgress = new ConvertionProgress();

        var videoInfo = await FFProbe.AnalyseAsync(source);

        using MemoryStream audioStream = new();
        // TODO: add overhead for metadata
        using MemoryStream videoStream = new();

        string finalFileName;

        if(outputFileName != null) finalFileName = outputFileName;
        else if (outputDirectory != null)
        {
            finalFileName = Path.Combine(outputDirectory, FileManager.ChangeFileExtension(Path.GetFileName(source), videoFormat.VideoFileExtension));

            if(Path.Exists(finalFileName)) finalFileName = FileManager.FindValidFilePath(finalFileName);
        }
        else
        {
            string finalDirectoryFileName = FileManager.CreateTemporaryFilePath();
            Directory.CreateDirectory(finalDirectoryFileName);

            finalFileName = Path.Combine(finalDirectoryFileName, FileManager.ChangeFileExtension(Path.GetFileName(source), videoFormat.VideoFileExtension));
        }

        await videoFormat.ConvertAudio(videoInfo, source, audioStream, videoSpecificConversionSettings, new Progress<int>(value =>
            {
                convertionProgress.AudioConvertionProgress = value;
                progress.Report(convertionProgress);
            }));

        long audioAndMetadataSizeBytes = audioStream.Length;
        long videoSizeAvailable = videoSpecificConversionSettings.TargetSize - audioAndMetadataSizeBytes;

        long targetBitrateInBytes = (long) (videoSizeAvailable / videoSpecificConversionSettings.FinalVideoDuration.TotalSeconds);

        await videoFormat.ConvertVideo(videoInfo, source, videoStream, videoSpecificConversionSettings, targetBitrateInBytes, new Progress<int>(value =>
            {
                convertionProgress.VideoConvertionProgress = value;
                progress.Report(convertionProgress);
            }));
            
        audioStream.Position = 0;
        videoStream.Position = 0;
        await videoFormat.CombineAudioAndVideo(videoInfo, audioStream, videoStream, finalFileName);

        return finalFileName;
    }
}