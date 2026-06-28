using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FFMpegCore;

namespace DiscordifyVideo.ViewModels;

public partial class VideoSpecificConversionSettingsDialogViewModel : ObservableObject
{
    private readonly Window _dialog;
    private readonly string _sourceFilePath;
    private readonly IMediaAnalysis _videoAnalysis;

    public double VideoDurationMilliseconds { get => _videoAnalysis.Duration.TotalMilliseconds; }

    [ObservableProperty]
    public long _targetSizeInMiB = 10;

    [ObservableProperty]
    public TimeSpan _cutVideoStart = TimeSpan.Zero;
    [ObservableProperty]
    public TimeSpan _cutVideoEnd; // is set in constructor


    public VideoSpecificConversionSettingsDialogViewModel(Window dialog, string sourceFilePath, IMediaAnalysis videoAnalysis)
    {
        _dialog = dialog;
        _sourceFilePath = sourceFilePath;
        _videoAnalysis = videoAnalysis;
        CutVideoEnd = _videoAnalysis.Duration;
    }

    [RelayCommand]
    private void Confirm()
    {
        _dialog.Close(new VideoSpecificConversionSettings
        {
            TargetSize = TargetSizeInMiB * 1024 * 1024,
            CutVideoStart = CutVideoStart,
            CutVideoEnd = CutVideoEnd
        });
    }

    [RelayCommand]
    private void Cancel() => _dialog.Close(null);
}