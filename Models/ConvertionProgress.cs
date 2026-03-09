using CommunityToolkit.Mvvm.ComponentModel;

public partial class ConvertionProgress : ObservableObject
{
    [ObservableProperty]
    public int _audioConvertionProgress = 0;
    [ObservableProperty]
    public int _videoConvertionProgress = 0;
    [ObservableProperty]
    public int _combiningProgress = 0;
}