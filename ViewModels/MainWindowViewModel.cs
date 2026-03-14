using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FFMpegCore;
using FFMpegCore.Arguments;

namespace DiscordifyVideo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCopyToClipboardAllowedToChange))]
    [NotifyPropertyChangedFor(nameof(CopyToClipboard))]
    [NotifyPropertyChangedFor(nameof(IsSelectDirectoryEnabled))]
    public int _selectedSaveFileOption = 0;

    public bool IsCopyToClipboardAllowedToChange => SelectedSaveFileOption != 0;

    private bool _copyToClipboard = true;
    public bool CopyToClipboard { 
        get => IsCopyToClipboardAllowedToChange ? _copyToClipboard : true; 
        set
        {
            if(!IsCopyToClipboardAllowedToChange) return;

            _copyToClipboard = value;
            OnPropertyChanged(nameof(CopyToClipboard));
        }
    }

    public bool IsSelectDirectoryEnabled => SelectedSaveFileOption == 1;
    
    [ObservableProperty]
    public ConvertionProgress _progress = new ConvertionProgress();

    private string? _selectedDirectory = null;
    /// <summary>
    /// RETURNS ONLY LAST PART OF THE PATH!! USE _selecctedDirectory FOR FULL PATH.
    /// if its null, returns "none"
    /// </summary>
    public string SelectedDirectory { 
        get => _selectedDirectory == null ? "none" : Path.GetFileName(_selectedDirectory); // should return the last part of the path
        set {
            _selectedDirectory = value;
            OnPropertyChanged(nameof(SelectedDirectory));
        }
    }

    
    public string? ConvertedVideo = null;

    [RelayCommand]
    public async Task ConvertFromClipboard()
    {
        // TODO: make a static object for the clipboard, which gets reused
        var clipboard = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null)?.Clipboard;
        if (clipboard == null) return;

        string filePath = await clipboard.GetTextAsync();

        await Convert(filePath);
    }

    private static Dictionary<string, string> extraFormats = new Dictionary<string, string>()
    {
        { "matroska", "*.mkv" },
    };

    private static List<string> formats = FFMpeg.GetContainerFormats().Select(
        value => extraFormats.TryGetValue(value.Name, out string customFileExtension) ? customFileExtension : "*" + value.Extension
    ).ToList();

    [RelayCommand]
    public async Task ConvertFromFile()
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Supported Formats") { Patterns = formats.ToList() },
            }
        });

        if (files.Count < 1) return;

        await Convert(files[0].TryGetLocalPath());
    }

    public async Task<string?> OpenSaveToDialog()
    {
         // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);

        // Start async operation to open the dialog.
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new[]
            {
                new FilePickerFileType("MP4") { Patterns = new[] { "*.mp4" } },
            }
        });

        if (file is null) return null;
        
        return file.TryGetLocalPath();
    }

    private async Task Convert(string sourceFilePath)
    {
        var storageProvider = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop2 ? desktop2.MainWindow : null)!.StorageProvider;
        // TODO: ???? ^ dekstop2?
    
        VideoConverter videoConverter = new VideoConverter();

        var progressHandler = new Progress<ConvertionProgress>(value => 
        {
            Progress = value;
        });

        string? outputFileName = null;
        string? outputDirectory = null;

        if(SelectedSaveFileOption == 1)
        {
            outputDirectory = _selectedDirectory;
        }
        if(SelectedSaveFileOption == 2)
        {
            outputFileName = await OpenSaveToDialog();
            if(outputFileName == null) return;
        }

        // muze byt text!!
        string fileName = await videoConverter.RunConvert(progressHandler, sourceFilePath, new h264(), outputFileName, outputDirectory);
        ConvertedVideo = fileName;

        if (CopyToClipboard)
        {
            var clipboard = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null)?.Clipboard;
            if (clipboard == null) return;

            IStorageFile file = await storageProvider.TryGetFileFromPathAsync(fileName);

            DataObject dataObject = new();
            dataObject.Set(DataFormats.Files, new List<IStorageFile> { file });
            dataObject.Set("text/uri-list", file.Path.AbsoluteUri);
            await clipboard.SetDataObjectAsync(dataObject);
        }

    }
    /// <summary>
    /// does nothing for now
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DeleteConvertedVideo()
    {
        return; // TODO: if output to directory is enabled, it would delete the directory if its empty
        if(ConvertedVideo == null) return;
        
        File.Delete(ConvertedVideo);
        Directory.Delete(Path.GetDirectoryName(ConvertedVideo));

        ConvertedVideo = null;
    }

    [RelayCommand]
    public async Task SelectDirectory()
    {
        var storageProvider = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null)!.StorageProvider;

        FolderPickerOpenOptions options = new()
        {
            AllowMultiple = false
        };

        var PickedFolders = await storageProvider.OpenFolderPickerAsync(options);
        if(PickedFolders.Count == 0) return;

        var PickedFolder = PickedFolders[0];

        SelectedDirectory = PickedFolder.TryGetLocalPath();
    }
}
