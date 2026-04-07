using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using DiscordifyVideo.ViewModels;
using FFMpegCore;
using FFMpegCore.Extensions.Downloader;

namespace DiscordifyVideo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Loaded += WhenLoaded;

        InitializeComponent();
    }

    private async void WhenLoaded(object? sender, RoutedEventArgs _)
    {
        await InitiateFFMpegCheck();
    }

    public async Task InitiateFFMpegCheck()
    {
        var vm = (MainWindowViewModel)DataContext!; 
        bool hasFFMpeg = vm.CheckForFFMpeg();

        if (hasFFMpeg) return;

        try
        {
            bool success = await TryDownloadFFMpeg();

            if(!success) {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
                {
                    desktopApp.Shutdown();
                }else Environment.Exit(0);

                return;
            }
        }catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
            Debug.WriteLine(e.StackTrace);
        }

        if (!vm.CheckForFFMpeg())
        {
            var dialog = new ConfirmDialog();
            dialog.DataContext = new ConfirmDialogViewModel(dialog, "Something went wrong while downloading FFmpeg, do you want to try again?");

            // ShowDialog returns a result when the dialog closes
            bool? result = await dialog.ShowDialog<bool?>(this);

            if (result.GetValueOrDefault(false))
            {
                await InitiateFFMpegCheck();
                return;
            }

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }else Environment.Exit(0);

            return;
        }
    }

    public async Task<bool> TryDownloadFFMpeg()
    {
        var dialog = new ConfirmDialog();
        dialog.DataContext = new ConfirmDialogViewModel(dialog, "FFmpeg was not detected on your system. Do you want to download it automatically?");

        // ShowDialog returns a result when the dialog closes
        bool? result = await dialog.ShowDialog<bool?>(this);

        if(result.GetValueOrDefault(false))
        {
            
            var downloadDialog = new FFMpegDownloadProgressDialog();
            var downloadDialogDataContext = new FFMpegDownloadProgressDialogViewModel(downloadDialog);

            downloadDialog.DataContext = downloadDialogDataContext;

            var dialogClosed = downloadDialog.ShowDialog<bool?>(this);

            await downloadDialogDataContext.StartDownload();

            return (await dialogClosed).GetValueOrDefault(false); // may not cancel correctly when the user closes the dialog
        }else return false;
    }
}