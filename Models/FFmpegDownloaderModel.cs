using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Exceptions;
using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;

namespace DiscordifyVideo.Models;

public class FFMpegDownloaderModel
{
    private static bool IsFFmpegNotFoundException(Exception exception)
    {
        return (
                (exception is FFMpegException) || // linux probably
                (exception is Win32Exception winexception && winexception.NativeErrorCode == 2) // windows
            );
    }
    public static bool CheckAndFindFFMpeg()
    {
        try{
            FFMpeg.GetCodecs();
            return true;
        }
        catch (Exception exception)
        {
            if(!IsFFmpegNotFoundException(exception)) throw;

            string previousBinaryFolder = GlobalFFOptions.Current.BinaryFolder;

            try
            {
                GlobalFFOptions.Current.BinaryFolder = ConfigManager.BinaryPath;
                FFMpeg.GetCodecs();
                return true;
            }
            catch(Exception exception2)
            {
                if(!IsFFmpegNotFoundException(exception2)) throw;

                GlobalFFOptions.Current.BinaryFolder = previousBinaryFolder;
                return false;
            }
        }
    }
    public static async Task StartDownload()
    {
        Directory.CreateDirectory(ConfigManager.BinaryPath);

        await FFMpegDownloader.DownloadBinaries(
            FFMpegVersions.LatestAvailable, 
            FFMpegBinaries.FFMpeg | FFMpegBinaries.FFProbe, 
            new(){
                BinaryFolder = ConfigManager.BinaryPath
            }
        ); 
        
        GlobalFFOptions.Current.BinaryFolder = ConfigManager.BinaryPath;
    }
}