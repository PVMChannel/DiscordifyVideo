using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordifyVideo.Models;
using FFMpegCore.Extensions.Downloader;

namespace DiscordifyVideo.ViewModels;

public partial class FFMpegDownloadProgressDialogViewModel : ObservableObject
{
    private readonly Window _dialog;

    public FFMpegDownloadProgressDialogViewModel(Window dialog)
    {
        _dialog = dialog;
    }

    public async Task StartDownload()
    {
        await FFMpegDownloaderModel.StartDownload();
        
        _dialog.Close(true);
    }
}