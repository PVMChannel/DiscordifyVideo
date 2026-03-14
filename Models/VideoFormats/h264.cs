using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

public class h264 : IVideoFormat
{
    public static string NULL_FILE = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "NUL" : "/dev/null";
    public static Speed SPEED_PRESET = Speed.Slow;
    public string AudioFileExtension { get => "mp4"; }
    public string VideoFileExtension { get => "mp4"; }

    public async Task ConvertAudioToFile(IMediaAnalysis videoInfo, string originalVideoFileName, string newFileName, IProgress<int> progress)
    {
        await FFMpegArguments.FromFileInput(originalVideoFileName, true)
            .OutputToFile(newFileName, false, options => options
                .SelectStream(0, 0, Channel.Audio)
                .WithAudioCodec("libopus")
            )
            .NotifyOnProgress((progressDouble) =>
            {
                progress.Report((int) progressDouble);
            }, videoInfo.Duration)
            .ProcessAsynchronously();
    }

    public async Task ConvertVideoToFile(IMediaAnalysis videoInfo, string originalVideoFileName, string newFileName, long bitrateInBytes, IProgress<int> progress)
    {
        // 125, because it is kilobits
        int bitrateInKiloBits = (int) (bitrateInBytes / 125);

        await FFMpegArguments.FromFileInput(originalVideoFileName, true)
            .OutputToFile(NULL_FILE, true, options => options
                .DisableChannel(Channel.Audio)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(SPEED_PRESET)
                .WithVideoBitrate(bitrateInKiloBits)
                .WithCustomArgument("-bufsize "+bitrateInKiloBits+"k")
                .WithCustomArgument("-pass 1") //* maybe change passlogfile
                .ForceFormat("null")
            )
            .NotifyOnProgress(progressDouble =>
            {
                progress.Report((int) progressDouble / 2);
            }, videoInfo.Duration)
            .ProcessAsynchronously();

        await FFMpegArguments.FromFileInput(originalVideoFileName, true)
            .OutputToFile(newFileName, false, options => options
                .DisableChannel(Channel.Audio)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(SPEED_PRESET)
                .WithVideoBitrate(bitrateInKiloBits)
                .WithCustomArgument("-bufsize "+bitrateInKiloBits+"k")
                .WithCustomArgument("-pass 2") //* maybe change passlogfile
            )
            .NotifyOnProgress(progressDouble =>
            {
                progress.Report(50 + (int) progressDouble / 2);
            }, videoInfo.Duration)
            .ProcessAsynchronously();
    }

    public async Task CombineAudioAndVideo(IMediaAnalysis videoInfo, string audioFileName, string videoFileName, string outputFileName, IProgress<int> progress)
    {
        Debug.WriteLine(outputFileName);

        await FFMpegArguments.FromFileInput(audioFileName, true)
            .AddFileInput(videoFileName)
            .OutputToFile(outputFileName, true, options => options
                .CopyChannel()
            )
            .NotifyOnProgress(progressDouble =>
            {
                progress.Report((int) progressDouble);
            }, videoInfo.Duration)
            .ProcessAsynchronously();

    }
}