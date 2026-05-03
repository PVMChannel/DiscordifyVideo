using System;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;

public interface IVideoFormat
{
    string AudioFileExtension { get; }
    string VideoFileExtension { get; }

    Task ConvertAudio(IMediaAnalysis videoInfo, string originalVideoFileName, Stream outputStream, IProgress<int> progress);
    Task ConvertVideo(IMediaAnalysis videoInfo, string originalVideoFileName, Stream outputStream, long bitrateInBytes, IProgress<int> progress);
    Task CombineAudioAndVideo(IMediaAnalysis videoInfo, Stream audioStream, Stream videoStream, string outputFileName, IProgress<int> progress);
}