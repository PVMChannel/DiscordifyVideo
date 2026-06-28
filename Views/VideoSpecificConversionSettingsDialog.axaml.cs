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

public partial class VideoSpecificConversionSettingsDialog : Window
{
    public VideoSpecificConversionSettingsDialog()
    {
        InitializeComponent();
    }
}