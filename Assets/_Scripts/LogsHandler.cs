using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

public static class LogsHandler
{
    private static readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
    private static readonly AutoResetEvent logEvent = new AutoResetEvent(false);
    private static readonly Thread logThread;

    private static readonly string sessionId =
        DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + "_" +
        Guid.NewGuid().ToString("N");

    private static readonly string logFilePath =
        Path.Combine(Application.persistentDataPath, $"log_{sessionId}.txt");

    static LogsHandler()
    {
        logThread = new Thread(ProcessQueue)
        {
            IsBackground = true
        };
        logThread.Start();
    }

    private static void ProcessQueue()
    {
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            while (true)
            {
                logEvent.WaitOne();

                while (logQueue.TryDequeue(out string log))
                {
                    writer.WriteLine(log);
                }

                writer.Flush();
            }
        }
    }

    private static string Prefix(string memberName, string filePath)
    {
        string className = Path.GetFileNameWithoutExtension(filePath);
        return $"[{className}/{memberName}]";
    }

    private static void Enqueue(string message)
    {
        logQueue.Enqueue(message);
        logEvent.Set();
    }

    public static void Log(
        object message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        string msg = $"{Prefix(memberName, filePath)} {message}";
        Debug.Log(msg);
        Enqueue(msg);
    }

    public static void LogWarning(
        object message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        string msg = $"[WARNING] {Prefix(memberName, filePath)} {message}";
        Debug.LogWarning(msg);
        Enqueue(msg);
    }

    public static void LogError(
        object message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        string msg = $"[ERROR] {Prefix(memberName, filePath)} {message}";
        Debug.LogError(msg);
        Enqueue(msg);
    }

    public static void LogException(
        Exception exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        string msg = $"[EXCEPTION] {Prefix(memberName, filePath)} {exception}";
        Debug.LogException(exception);
        Enqueue(msg);
    }

    public static string GetLogPath() => logFilePath;
}