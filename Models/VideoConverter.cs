using System;
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

    public string createTemporaryFilePath()
    {
        return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    }
    public string createTemporaryFilePath(string extension)
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

    public async Task<string> RunConvert(IProgress<ConvertionProgress> progress, string source, IVideoFormat videoFormat)
    {
        ConvertionProgress convertionProgress = new ConvertionProgress();

        var videoInfo = await FFProbe.AnalyseAsync(source);

        string audioFileName = createTemporaryFilePath(videoFormat.AudioFileExtension);
        // ^ ulozit jako mp4, aby do toho byly pripocitany i ty metadata
        string videoFileName = createTemporaryFilePath(videoFormat.VideoFileExtension);

        string finalDirectoryFileName = createTemporaryFilePath();
        Directory.CreateDirectory(finalDirectoryFileName);
        string finalFileName = Path.Combine(finalDirectoryFileName, changeFileExtension(Path.GetFileName(source), videoFormat.VideoFileExtension));

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

        
        File.Delete(videoFileName);

        return finalFileName;


        Debug.WriteLine("hotovo?");
    }
}