using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiscordifyVideo.Models;

public class FileManager
{
    public static string CreateTemporaryFilePath()
    {
        return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    }
    public static string CreateTemporaryFilePath(string extension)
    {
        return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "." + extension);
    }

    public static string ChangeFileExtension(string originalFileName, string to)
    {
        string[] parts = originalFileName.Split(".");
        if(parts.Length == 1)
        {
            parts = (string[]) parts.Append(to);
        }else parts.SetValue(to, parts.Length - 1);

        return string.Join(".", parts);
    }

    /// <summary>
    /// tries to find an alternative file name
    /// for the input of "video.mp4" it will try:
    /// video.discordify.mp4
    /// video.discordify2.mp4
    /// video.discrodify3.mp4
    /// etc.
    /// </summary>
    /// <param name="filePath">the FULL file path (eg. /path/to/video.mp4)</param>
    /// <returns>the full file path of the new file name</returns>
    internal static string FindValidFilePath(string filePath)
    {
        string directoryName = Path.GetDirectoryName(filePath);
        string fileName = Path.GetFileName(filePath);
        List<string> parts = fileName.Split(".").ToList();

        parts.Insert(parts.Count - 1, "discordify");
        string newFileName = Path.Combine(directoryName, string.Join(".", parts));

        int iterationNumber = 2;
        // this part will get skipped if video.discordify.mp4 works
        while (Path.Exists(newFileName))
        {
            parts[^2] = "discordify" + iterationNumber.ToString();
            newFileName = Path.Combine(directoryName, string.Join(".", parts));
        }

        return newFileName;
    }
}