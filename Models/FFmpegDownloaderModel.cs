using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Exceptions;
using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;

namespace DiscordifyVideo.Models;

public class FFMpegDownloaderModel
{
    public static bool CheckAndFindFFMpeg()
    {
        try{
            FFMpeg.GetCodecs();
            return true;
        }
        catch (FFMpegException)
        {
            string previousBinaryFolder = GlobalFFOptions.Current.BinaryFolder;

            try
            {
                GlobalFFOptions.Current.BinaryFolder = ConfigManager.BinaryPath;
                FFMpeg.GetCodecs();
                return true;
            }
            catch
            {
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