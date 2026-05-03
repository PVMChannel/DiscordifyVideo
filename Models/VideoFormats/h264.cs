using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using DiscordifyVideo.Models;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

public class h264 : IVideoFormat
{
    public static string NULL_FILE = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "NUL" : "/dev/null";
    public static Speed SPEED_PRESET = Speed.Slow;
    public string AudioFileExtension { get => "mp4"; }
    public string VideoFileExtension { get => "mp4"; }

    public async Task ConvertAudio(IMediaAnalysis videoInfo, string originalVideoFileName, Stream outputStream, IProgress<int> progress)
    {
        await FFMpegArguments.FromFileInput(originalVideoFileName, true)
            .OutputToPipe(new StreamPipeSink(outputStream), options => options
                .ForceFormat("mp4")
                .WithCustomArgument("-movflags frag_keyframe")
                .SelectStream(0, 0, Channel.Audio)
                .WithAudioCodec("libopus")
            )
            .NotifyOnProgress((progressDouble) =>
            {
                progress.Report((int) progressDouble);
            }, videoInfo.Duration)
            .ProcessAsynchronously();
    }

    public async Task ConvertVideo(IMediaAnalysis videoInfo, string originalVideoFileName, Stream outputStream, long bitrateInBytes, IProgress<int> progress)
    {
        // 125, because it is kilobits
        int bitrateInKiloBits = (int) (bitrateInBytes / 125);

        string passlogfile = FileManager.CreateTemporaryFilePath();

        await FFMpegArguments.FromFileInput(originalVideoFileName, true)
            .OutputToFile(NULL_FILE, true, options => options
                .DisableChannel(Channel.Audio)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(SPEED_PRESET)
                .WithVideoBitrate(bitrateInKiloBits)
                .WithCustomArgument("-bufsize "+bitrateInKiloBits+"k")
                .WithCustomArgument("-pass 1")
                .WithCustomArgument("-passlogfile "+passlogfile)
                .ForceFormat("null")
            )
            .NotifyOnProgress(progressDouble =>
            {
                progress.Report((int) progressDouble / 2);
            }, videoInfo.Duration)
            .ProcessAsynchronously();

        await FFMpegArguments.FromFileInput(originalVideoFileName, true)
            .OutputToPipe(new StreamPipeSink(outputStream), options => options
                .ForceFormat("mp4")
                .WithCustomArgument("-movflags frag_keyframe")
                .DisableChannel(Channel.Audio)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(SPEED_PRESET)
                .WithVideoBitrate(bitrateInKiloBits)
                .WithCustomArgument("-bufsize "+bitrateInKiloBits+"k")
                .WithCustomArgument("-pass 2")
                .WithCustomArgument("-passlogfile "+passlogfile)
            )
            .NotifyOnProgress(progressDouble =>
            {
                progress.Report(50 + (int) progressDouble / 2);
            }, videoInfo.Duration)
            .ProcessAsynchronously();

        // cleanup after two-pass, for multi-track video support there will be more files
        File.Delete(passlogfile + "-0.log");
        File.Delete(passlogfile + "-0.log.mbtree");
    }

    public async Task CombineAudioAndVideo(IMediaAnalysis videoInfo, Stream audioStream, Stream videoStream, string outputFileName, IProgress<int> progress)
    {
        Debug.WriteLine(outputFileName);

        await FFMpegArguments.FromPipeInput(new StreamPipeSource(audioStream))
            .AddPipeInput(new StreamPipeSource(videoStream))
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