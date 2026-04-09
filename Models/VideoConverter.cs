using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using FFMpegCore;
using FFMpegCore.Enums;

public class VideoConverter
{
    public const long TARGET_SIZE = 10 * 1024 * 1024;
    public VideoConverter()
    {
        
    }

    public static string createTemporaryFilePath()
    {
        return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    }
    public static string createTemporaryFilePath(string extension)
    {
        return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "." + extension);
    }
    
    public string changeFileExtension(string originalFileName, string to)
    {
        string[] parts = originalFileName.Split(".");
        if(parts.Length == 1)
        {
            parts = (string[]) parts.Append(to);
        }else parts.SetValue(to, parts.Length - 1);

        return string.Join(".", parts);
    }

    /// <summary>
    /// tries to find an alternative file name
    /// for the input of "video.mp4" it will try:
    /// video.discordify.mp4
    /// video.discordify2.mp4
    /// video.discrodify3.mp4
    /// etc.
    /// </summary>
    /// <param name="filePath">the FULL file path (eg. /path/to/video.mp4)</param>
    /// <returns>the full file path of the new file name</returns>
    private string findValidFilePath(string filePath)
    {
        string directoryName = Path.GetDirectoryName(filePath);
        string fileName = Path.GetFileName(filePath);
        List<string> parts = fileName.Split(".").ToList();

        parts.Insert(parts.Count - 1, "discordify");
        string newFileName = Path.Combine(directoryName, string.Join(".", parts));

        int iterationNumber = 2;
        // this part will get skipped if video.discordify.mp4 works
        while (Path.Exists(newFileName))
        {
            parts[^2] = "discordify" + iterationNumber.ToString();
            newFileName = Path.Combine(directoryName, string.Join(".", parts));
        }

        return newFileName;
    }

    public async Task<string> RunConvert(IProgress<ConvertionProgress> progress, string source, IVideoFormat videoFormat, string? outputFileName = null, string? outputDirectory = null)
    {
        ConvertionProgress convertionProgress = new ConvertionProgress();

        var videoInfo = await FFProbe.AnalyseAsync(source);

        string audioFileName = createTemporaryFilePath(videoFormat.AudioFileExtension);
        // ^ ulozit jako mp4, aby do toho byly pripocitany i ty metadata
        string videoFileName = createTemporaryFilePath(videoFormat.VideoFileExtension);

        string finalFileName;

        if(outputFileName != null) finalFileName = outputFileName;
        else if (outputDirectory != null)
        {
            finalFileName = Path.Combine(outputDirectory, changeFileExtension(Path.GetFileName(source), videoFormat.VideoFileExtension));

            if(Path.Exists(finalFileName)) finalFileName = findValidFilePath(finalFileName);
        }
        else
        {
            string finalDirectoryFileName = createTemporaryFilePath();
            Directory.CreateDirectory(finalDirectoryFileName);

            finalFileName = Path.Combine(finalDirectoryFileName, changeFileExtension(Path.GetFileName(source), videoFormat.VideoFileExtension));
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


        Debug.WriteLine("hotovo?");
    }
}