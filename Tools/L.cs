using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

public readonly struct L
{
    private static readonly object _lock = new object();
    private static readonly string _logPath;

    static L()
    {
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        _logPath = Path.Combine(exeDir, "debug.log");
    }

    public int mLine { get; }
    public string mPath { get; }

    public L(
        [CallerLineNumber] int aLine = 0,
        [CallerFilePath] string aPath = ""
    )
    {
        mPath = aPath;
        mLine = aLine;
    }

    public override string ToString()
    {
        return $"{mPath}:({mLine})";
    }

    public static void WriteLine(
        string text,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string path = ""
    )
    {
        var loc = new L(line, path);
        var message = $"{DateTime.Now:HH:mm:ss.fff} {loc} : {text}";
        Trace.WriteLine(message);
        WriteToFile(message);
    }

    private static void WriteToFile(string message)
    {
        try
        {
            lock (_lock)
            {
                File.AppendAllText(_logPath, message + Environment.NewLine);
            }
        }
        catch
        {
            // ログ出力失敗は無視
        }
    }

    public static void ClearLog()
    {
        try
        {
            lock (_lock)
            {
                File.WriteAllText(_logPath, string.Empty);
            }
        }
        catch
        {
            // ignore
        }
    }
}


