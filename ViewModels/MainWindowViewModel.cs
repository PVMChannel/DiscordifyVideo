using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DiscordifyVideo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string? ConvertedVideo = null;
    
    [ObservableProperty]
    public ConvertionProgress _progress = new ConvertionProgress();

    [RelayCommand]
    public async Task Convert()
    {
        var clipboard = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null)?.Clipboard;
        var storageProvider = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop2 ? desktop2.MainWindow : null)!.StorageProvider;
        // TODO: ???? ^


        if (clipboard != null)
        {
            VideoConverter videoConverter = new VideoConverter();

            var progressHandler = new Progress<ConvertionProgress>(value => 
            {
                Progress = value;
            });

            try
            {
            // muze byt text!!
                string fileName = await videoConverter.RunConvert(progressHandler, await clipboard.GetTextAsync(), new h264());
                ConvertedVideo = fileName;

                IStorageFile file = await storageProvider.TryGetFileFromPathAsync(fileName);

                DataObject dataObject = new();
                dataObject.Set(DataFormats.Files, new List<IStorageFile> { file });
                dataObject.Set("text/uri-list", file.Path.AbsoluteUri);
                await clipboard.SetDataObjectAsync(dataObject);

            }catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.Data);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }

    [RelayCommand]
    public async Task DeleteConvertedVideo()
    {
        if(ConvertedVideo == null) return;
        
        File.Delete(ConvertedVideo);
        Directory.Delete(Path.GetDirectoryName(ConvertedVideo));

        ConvertedVideo = null;
    }
}
