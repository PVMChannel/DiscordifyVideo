using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DiscordifyVideo.ViewModels;
using DiscordifyVideo.Views;
using FFMpegCore;

public interface IDialogService
{
    Task<bool> ShowConfirmAsync(string message);
    Task<VideoSpecificConversionSettings?> OpenVideoSpecificConversionSettingsDialog(string sourceFilePath, IMediaAnalysis videoAnalysis);
}

public class DialogService : IDialogService
{
    public DialogService()
    {
        
    }

    public async Task<bool> ShowConfirmAsync(string message)
    {
        if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = new ConfirmDialog();
            dialog.DataContext = new ConfirmDialogViewModel(dialog, message);
            var result = await dialog.ShowDialog<bool?>(desktop.MainWindow!);
            return result == true;
        }else throw new NotSupportedException();
    }

    
    public async Task<VideoSpecificConversionSettings?> OpenVideoSpecificConversionSettingsDialog(string sourceFilePath, IMediaAnalysis videoAnalysis)
    {
        if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = new VideoSpecificConversionSettingsDialog();

            var vm = new VideoSpecificConversionSettingsDialogViewModel(dialog, sourceFilePath, videoAnalysis);

            dialog.DataContext = vm;

            // ShowDialog returns a result when the dialog closes
            VideoSpecificConversionSettings? result = await dialog.ShowDialog<VideoSpecificConversionSettings?>(desktop.MainWindow!);

            return result;
        }else throw new NotSupportedException();
    }
}