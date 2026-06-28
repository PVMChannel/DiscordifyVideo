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

    public async Task ConvertAudio(IMediaAnalysis videoInfo, string originalVideoFileName, Stream outputStream, VideoSpecificConversionSettings settings, IProgress<int> progress)
    {
        await FFMpegArguments.FromFileInput(originalVideoFileName, true, options => options.ApplyAudioSpecificInputSettings(settings))
            .OutputToPipe(new StreamPipeSink(outputStream), options => options
                .ApplyAudioSpecificOutputSettings(settings)
                .ForceFormat("nut")
                .SelectStream(0, 0, Channel.Audio)
                .WithAudioCodec("libopus")
            )
            .NotifyOnProgress((progressDouble) =>
            {
                progress.Report((int) progressDouble);
            }, settings.FinalVideoDuration)
            .ProcessAsynchronously();
    }

    public async Task ConvertVideo(IMediaAnalysis videoInfo, string originalVideoFileName, Stream outputStream, VideoSpecificConversionSettings settings, long bitrateInBytes, IProgress<int> progress)
    {
        // 125, because it is kilobits
        int bitrateInKiloBits = (int) (bitrateInBytes / 125);

        string passlogfile = FileManager.CreateTemporaryFilePath();

        await FFMpegArguments.FromFileInput(originalVideoFileName, true, options => options.ApplyVideoSpecificInputSettings(settings))
            .OutputToFile(NULL_FILE, true, options => options
                .ApplyVideoSpecificOutputSettings(settings)
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
            }, settings.FinalVideoDuration)
            .ProcessAsynchronously();

        await FFMpegArguments.FromFileInput(originalVideoFileName, true, options => options.ApplyVideoSpecificInputSettings(settings))
            .OutputToPipe(new StreamPipeSink(outputStream), options => options
                .ApplyVideoSpecificOutputSettings(settings)
                .ForceFormat("nut")
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
            }, settings.FinalVideoDuration)
            .ProcessAsynchronously();

        // cleanup after two-pass, for multi-track video support there will be more files
        File.Delete(passlogfile + "-0.log");
        File.Delete(passlogfile + "-0.log.mbtree");
    }

    public async Task CombineAudioAndVideo(IMediaAnalysis videoInfo, Stream audioStream, Stream videoStream, string outputFileName)
    {
        Debug.WriteLine(outputFileName);

        await FFMpegArguments.FromPipeInput(new StreamPipeSource(audioStream))
            .AddPipeInput(new StreamPipeSource(videoStream))
            .OutputToFile(outputFileName, true, options => options
                .CopyChannel()
            )
            .ProcessAsynchronously();

    }
}