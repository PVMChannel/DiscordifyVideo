using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DiscordifyVideo.ViewModels;
using FFMpegCore;
using FFMpegCore.Extensions.Downloader;

namespace DiscordifyVideo.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }
}