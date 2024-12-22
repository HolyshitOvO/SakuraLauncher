using System;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
//namespace RunnerPlugin
//{
public static class Tick
{
    // Store the start times and tags
    private static readonly List<long> tickTimerTime = new List<long>();
    private static readonly List<string> tickTimerTag = new List<string>();
    private static readonly Dictionary<string, long> singleTickTag = new Dictionary<string, long>();

    // Store the app start time
    private static long appStartTime;

    // Method to mark the start time for a given tag
    public static void TickMethod(string tag)
    {
        tickTimerTime.Add(GetCurrentTime());
        tickTimerTag.Add(tag);
    }

    // Overloaded method without a tag (empty tag)
    public static void TickMethod()
    {
        TickMethod(string.Empty);
    }

    // Get the formatted string of all timing intervals
    public static string GetAllTime()
    {
        int sizeTemp = tickTimerTime.Count;
        var str = new StringBuilder();

        for (int i = 1; i < sizeTemp; i++)
        {
            if (string.IsNullOrEmpty(tickTimerTag[i])) continue;
            str.Append("█ ").Append(tickTimerTag[i]).Append(":");
            int sizeTemp2 = 22 - tickTimerTag[i].Length;
            for (int ii = 1; ii < sizeTemp2; ii++) str.Append(" ");
            str.Append(tickTimerTime[i] - tickTimerTime[i - 1]).Append("ms").Append("\r\n");
        }

        str.Append("█ AllTime:        ").Append(tickTimerTime[tickTimerTime.Count - 1] - tickTimerTime[0]).Append("ms");

        return str.ToString();
    }

    // Log all timing information and clear the data
    public static void LogAllThenClear()
    {
        LogAllTime("█TickTimer");
        Clear();
    }

    // Log all timing information with the default tag
    public static void LogAllTime()
    {
        LogAllTime("█TickTimer");
    }

    // Log all timing information with a custom tag
    public static void LogAllTime(string tag)
    {
        Debug.WriteLine(GetAllTime());
    }

    // Clear all the collected timing data
    public static void Clear()
    {
        tickTimerTag.Clear();
        tickTimerTime.Clear();
    }

    // █████ Single method timing tracking █████
    // Mark the start time for a single operation
    public static void TickSingle(String tag)
    {
        singleTickTag[tag]= GetCurrentTime();
    }



    // Log the time elapsed for a single operation with a custom tag
    public static void LogSingle(string tag)
    {
        long end = GetCurrentTime();
        if (!singleTickTag.ContainsKey(tag))
        {
            Debug.WriteLine("█TickSingle:  没有这个KEY值");
        }
        else
        {
            Debug.WriteLine($"█TickSingle:  {tag}:  {end - singleTickTag[tag]}ms");
        }
    }

    // Log the time elapsed since the app start
    public static void StartTimeLog()
    {
        long end = GetCurrentTime();
        Debug.WriteLine($"█StartTime:  {(end - appStartTime)}ms");
    }

    // Private constructor to prevent instantiation
    //private Tick() { }

    // Set the app start time (can be called once at app startup)
    public static void SetAppStartTime(long appStartTime)
    {
        Tick.appStartTime = appStartTime;
    }

    // Helper method to get the current time in milliseconds
    private static long GetCurrentTime()
    {
        return Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000);
    }
}

//}