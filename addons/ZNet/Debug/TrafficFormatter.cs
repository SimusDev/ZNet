namespace ZNet.Debug;

public static class TrafficFormatter
{
    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        else if (bytes < 1024 * 1024)
            return $"{bytes / 1024f:F1} KB";
        else if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024f * 1024f):F1} MB";
        else
            return $"{bytes / (1024f * 1024f * 1024f):F2} GB";
    }

    public static string FormatBytesPerSecond(long bytesPerSecond)
    {
        return $"{FormatBytes(bytesPerSecond)}/s";
    }
}