using System;
using System.Threading.Tasks;
using FFMpegCore;

public interface IVideoFormat
{
    string AudioFileExtension { get; }
    string VideoFileExtension { get; }

    Task ConvertAudioToFile(IMediaAnalysis videoInfo, string originalVideoFileName, string newFileName, IProgress<int> progress);
    Task ConvertVideoToFile(IMediaAnalysis videoInfo, string originalVideoFileName, string newFileName, long bitrateInBytes, IProgress<int> progress);
    Task CombineAudioAndVideo(IMediaAnalysis videoInfo, string audioFileName, string videoFileName, string outputFileName, IProgress<int> progress);
}