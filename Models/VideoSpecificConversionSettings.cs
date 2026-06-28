using System;
using System.Dynamic;

public class VideoSpecificConversionSettings {
    public long TargetSize;
    public TimeSpan CutVideoStart;
    public TimeSpan CutVideoEnd;
    public TimeSpan FinalVideoDuration { get => CutVideoEnd - CutVideoStart; }
}