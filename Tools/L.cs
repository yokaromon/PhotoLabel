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

    /// <summary>
    /// ログをローテーションする。最大 maxGenerations 世代保持。
    /// debug.log → debug.log.1 → debug.log.2 → ... → 削除
    /// </summary>
    public static void RotateLog(int maxGenerations = 3)
    {
        try
        {
            lock (_lock)
            {
                // 最古世代を削除
                var oldest = $"{_logPath}.{maxGenerations}";
                if (File.Exists(oldest)) File.Delete(oldest);

                // 古い世代を1つずつ繰り上げ
                for (int i = maxGenerations - 1; i >= 1; i--)
                {
                    var src = $"{_logPath}.{i}";
                    var dst = $"{_logPath}.{i + 1}";
                    if (File.Exists(src)) File.Move(src, dst);
                }

                // 現行ログを .1 へ
                if (File.Exists(_logPath)) File.Move(_logPath, $"{_logPath}.1");
            }
        }
        catch
        {
            // ignore
        }
    }
}


