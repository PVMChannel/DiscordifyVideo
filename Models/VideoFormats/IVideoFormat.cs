using System;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;

public interface IVideoFormat
{
    string AudioFileExtension { get; }
    string VideoFileExtension { get; }

    Task ConvertAudio(IMediaAnalysis videoInfo, string originalVideoFileName, Stream outputStream, VideoSpecificConversionSettings settings,IProgress<int> progress);
    Task ConvertVideo(IMediaAnalysis videoInfo, string originalVideoFileName, Stream outputStream, VideoSpecificConversionSettings settings, long bitrateInBytes, IProgress<int> progress);
    Task CombineAudioAndVideo(IMediaAnalysis videoInfo, Stream audioStream, Stream videoStream, string outputFileName);
}