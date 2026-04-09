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
    public const long TARGET_SIZE = 10 * 1024 * 1024;
    public VideoConverter()
    {
        
    }

    public async Task<string> RunConvert(IProgress<ConvertionProgress> progress, string source, IVideoFormat videoFormat, string? outputFileName = null, string? outputDirectory = null)
    {
        ConvertionProgress convertionProgress = new ConvertionProgress();

        var videoInfo = await FFProbe.AnalyseAsync(source);

        string audioFileName = FileManager.CreateTemporaryFilePath(videoFormat.AudioFileExtension);
        // ^ save as mp4, so that the metadata will be added to the calculation
        string videoFileName = FileManager.CreateTemporaryFilePath(videoFormat.VideoFileExtension);

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

        await videoFormat.ConvertAudioToFile(videoInfo, source, audioFileName, new Progress<int>(value =>
            {
                convertionProgress.AudioConvertionProgress = value;
                progress.Report(convertionProgress);
            }));

        long audioAndMetadataSizeBytes = new FileInfo(audioFileName).Length;
        long videoSizeAvailable = TARGET_SIZE - audioAndMetadataSizeBytes;

        long targetBitrateInBytes = (long) (videoSizeAvailable / videoInfo.Duration.TotalSeconds);

        await videoFormat.ConvertVideoToFile(videoInfo, source, videoFileName, targetBitrateInBytes, new Progress<int>(value =>
            {
                convertionProgress.VideoConvertionProgress = value;
                progress.Report(convertionProgress);
            }));
            

        await videoFormat.CombineAudioAndVideo(videoInfo, audioFileName, videoFileName, finalFileName, new Progress<int>(value =>
            {
                convertionProgress.CombiningProgress = value;
                progress.Report(convertionProgress);
            }));

        File.Delete(audioFileName);
        File.Delete(videoFileName);

        return finalFileName;
    }
}