public class Config
{
    public int SelectedSaveFileOption { get; set; } = 0;
    public string? SelectedDirectory { get; set; } = null;
    public bool CopyToClipboard { get; set; } = true;
    public string? LastTemporaryVideoFile { get; set; } = null;
}