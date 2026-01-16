using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tools;

namespace Tools
{
    public partial class FrmLog : System.Windows.Forms.Form
    {
        FileStream fs;
        FileSystemWatcher fsw;
        long pos;

        public string LogFileName
        {
            get
            {
                return LogManager.sLogManager.LogFileName;
            }
        }

        public FrmLog( string aLogDirname)
        {
            LogManager.Init(aLogDirname);

            InitializeComponent();
        }

        public static int Log(
            string aFormat,
            params object[] aParams)
        {
            return LogManager.Log( $"{ aParams}");
        }

        public static int Log(
            L aL,
            string aFormat,
            params object[] aParams)
        {
            return LogManager.Log(aL.ToString(), aFormat, aParams);
        }


        public static int Err(
            L aL,
            Exception aEx
        )
        {
            return LogManager.Log("{0}{1}", aL.ToString(),  aEx.StackTrace);
        }


        public void WatchStart()
        {

            //TODO:ファイルが選ばれなかった場合などの処理が必要

            //ファイルを読込専用、他プロセスからの読書き可能として開き、読込んで読込み位置を取得する
            fs = new FileStream(LogFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ReadFile(fs, textBox1);
            pos = fs.Position;

            //指定のファイルのみ、更新された際に非同期にイベントを呼出す
            fsw = new FileSystemWatcher();
            fsw.Path = Path.GetDirectoryName(LogFileName);
            fsw.Filter = Path.GetFileName(LogFileName);
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.Changed += new FileSystemEventHandler(fsw_Changed);
            fsw.SynchronizingObject = this;
            fsw.EnableRaisingEvents = true;
        }

        static void ReadFile(FileStream fs, TextBox tb)
        {
            //ファイルを一時的に読み込むバイト型配列を作成する
            byte[] bs = new byte[0xFFFF];
            //ファイルをすべて読み込む
            for (; ; )
            {
                //ファイルの一部を読み込む
                int readSize = fs.Read(bs, 0, bs.Length);
                //ファイルをすべて読み込んだときは終了する
                if (readSize == 0)
                    break;
                //部分的に読み込んだデータを使用したコードをここに記述する
                tb.Text += System.Text.Encoding.GetEncoding(65001).GetString(bs);
            }
            //カーソルを行末に移動して、スクロールさせる
            tb.SelectionStart = tb.Text.Length;
            tb.ScrollToCaret();
        }

        void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if( !Visible )
            {
                return;
            }
            //ファイルの先頭から指定した位置までストリーム内の読込み位置を変更し、追加分のデータを読込んで、読込み位置を最後の位置にする
            fs.Seek(pos, SeekOrigin.Begin);
            ReadFile(fs, textBox1);
            pos = fs.Position;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            fs.Close();
            fsw.EnableRaisingEvents = false;
            fsw.Dispose();
        }
    }
//    public class L
//    {
//        public int mLine;
//        public string mPath;
//        public L(
//            [CallerLineNumber] int aLine = 0,
//            [CallerFilePath] string aPath = ""
//        )
//        {
//            mPath = aPath;
//            mLine = aLine;
//        }
//        public string ToString()
//        {
//            string info = "";
//            info = string.Format("{0}:({1})", mPath, mLine);
//            return info;
//        }
//    }

    public class ExFileWatcher : FileSystemWatcher
    {
        private long _SeekPoint = 0;
        public long SeekPoint
        {
            get
            {
                return _SeekPoint;
            }
            set
            {
                _SeekPoint = value;
            }
        }
        public void SetSeekPoint( string aFileName )
        {
            // ログファイルの更新部分を読み込み
            using (StreamReader reader = new StreamReader(aFileName))
            {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                }

                // 現在のファイルサイズを記録
                _SeekPoint = reader.BaseStream.Position;
            }
        }
    }
    public class LogManager : Object
    {
        public static LogManager sLogManager = null;   // LogManagerをスタティックにしておいて、ログを出力するときは1つのインスタンスから出力するような方法
        private string mLogDir = "";
        private int mMaxDate = 0;
        private String mLastText = "";   // Utility クラスから持ってきた。lock文の変数に使用している。
        public delegate void DelegateOnUpdateLog(string aFileName, string aText);
        public DelegateOnUpdateLog OnLogUpdate = null;
        public Dictionary<string,ExFileWatcher> mWatcher = new Dictionary<string, ExFileWatcher>();

        public void SetEventHandler( Form aOwner, DelegateOnUpdateLog aDelegate )
        {
            ExFileWatcher errWatcher = StartLogMonitoring(aOwner, aDelegate, ErrorLogFileName);
            mWatcher.Add(ErrorLogFileName, errWatcher);
            ExFileWatcher logWatcher = StartLogMonitoring(aOwner, aDelegate, LogFileName);
            mWatcher.Add(LogFileName, logWatcher);
        }

        public string ErrorLogFileName
        {
            get
            {
                return GetTodayFileName("Err");
            }
        }

        public string LogFileName
        {
            get
            {
                return GetTodayFileName("Log");
            }
        }

        public static void Init( string aLogDir = null, int aMaxDate = 10)
        {
            if( aLogDir == null )
            {
                aLogDir = Path.GetTempPath();
            }

            if (sLogManager != null)
            {
                // 初期化が2回動くのはおかしい
                return;
            }
            sLogManager = new LogManager( aLogDir, aMaxDate);
        }

        private ExFileWatcher StartLogMonitoring( Form aOwner, DelegateOnUpdateLog aOnUpdateLog, string aLogFileName )
        {
            ExFileWatcher watcher = new ExFileWatcher();
            // FileSystemWatcherの設定
            watcher.Path = Path.GetDirectoryName(aLogFileName);
            watcher.Filter = Path.GetFileName(aLogFileName);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += OnLogFileChanged;
            watcher.SynchronizingObject = aOwner;
            watcher.EnableRaisingEvents = true;
            watcher.SetSeekPoint(aLogFileName);
            OnLogUpdate = aOnUpdateLog;
            return watcher;
        }

        private void CreateDir()
        {
            if (System.IO.Directory.Exists(mLogDir))  // ディレクトリの存在判定.
            {
                // ディレクトリがすでに存在する場合の処理
            }
            else
            {
                // ここでディレクトリを作成する
                System.IO.Directory.CreateDirectory(mLogDir);
            }

            // 作成するファイルをフルパスで設定
            string logFileName = LogFileName;
            string errFileName = ErrorLogFileName;


            if (System.IO.File.Exists(logFileName))  // Logファイルの存在判定.
            {
                // ファイルがすでに存在する場合の処理
            }
            else
            {
                // ここでファイルを作成する.開いたままになってしまうのでCloseする
                System.IO.File.Create(logFileName).Close();
            }


            if (System.IO.File.Exists(errFileName))  // Errファイルの存在判定.
            {
                // ファイルがすでに存在する場合の処理
            }
            else
            {
                // ここでファイルを作成する.開いたままになってしまうのでCloseする
                System.IO.File.Create(errFileName).Close();
            }

            //ExFileWatcher logWatcher = StartLogMonitoring( logFileName);
            //mWatcher.Add( logFileName, logWatcher);

        }

        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            // ログファイルが更新されたときの処理
            string updatedText = "";
            string fileName = e.FullPath;

            ExFileWatcher watcher = mWatcher[fileName];
            try
            {
                // ログファイルの更新部分を読み込み
                using (StreamReader reader = new StreamReader(fileName))
                {
                    reader.BaseStream.Seek(watcher.SeekPoint, SeekOrigin.Begin);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (OnLogUpdate == null)
                        {
                            continue;
                        }
                        OnLogUpdate(fileName, line);
                    }
                    watcher.SeekPoint = reader.BaseStream.Position;
                }

            }
            catch (IOException ex)
            {
                // ログファイルの読み込みエラー
                MessageBox.Show("ログファイルの読み込み中にエラーが発生しました: " + ex.Message);
            }
        }

        protected void Dispose()
        {

            foreach( string key in mWatcher.Keys )
            {
                mWatcher[key].Dispose();
            }
        }
        /// <summary>
        /// 指定日数以上経過した.logファイルを削除する
        /// </summary>
        private void DeleteOldLogFile()
        {
            /// 日付を計算して判定する
            ///
            /// 11/30
            /// 削除条件を経過日数に設定.mMaxDateよりも長く経過したファイルを削除する
            /// 下記サイトのバリエーション３を参考に作成。引数はあえて関数に内包させている。(URL先頭のhを消しています)
            /// ttps://ucolonyen.blogspot.com/2013/08/clinq.html
            ///

            try
            {

                int startPos = 3;   // 日付文字列が開始する位置."Log","Err"ともに3文字のためファイル名の4文字目から日付文字列となっている
                string dateForm = "yyyyMMdd";   // 日付文字列の書式を指定
                DateTime target = DateTime.Today.AddDays(-mMaxDate);

                System.IO.Directory.GetFiles(mLogDir)
                  .Where(f => DateTime.ParseExact(
                    System.IO.Path.GetFileName(f).Substring(startPos, dateForm.Length),
                    dateForm,
                    System.Globalization.DateTimeFormatInfo.InvariantInfo) < target)
                  .ToList()
                  .ForEach(f => System.IO.File.Delete(f));
            }catch( Exception )
            {

            }

        }

        /// <summary>
        /// ログマネージャ
        /// </summary>
        /// <param name="aLogDir">ログ出力するディレクトリ</param>
        /// <param name="aMaxDate">ログを保持する日数(10の場合、ファイルが10以上増えると削除する</param>
        private LogManager(string aLogDir, int aMaxDate = 10)
        {
            /// 日付指定したディレクトリを作成する
            /// 例:aLogDirがC:\temp\log"であれば、
            /// 出力するログファイルは c:\temp\log\Log20181129.log"
            /// または                 c:\temp\log\Err20181129.log"
            /// となる。
            ///
            mLogDir = aLogDir;
            mMaxDate = aMaxDate;
            CreateDir();
            DeleteOldLogFile();
        }
        /// <summary>
        /// ログを出力する
        /// </summary>
        /// <param name="aFormat">ログのフォーマット</param>
        /// <param name="aParams">出力パラメータ</param>
        /// <returns></returns>
        public static int Log(
            string aFormat,
            params object[] aParams)
        {
            lock (sLogManager)
            {
                StringBuilder logText = new StringBuilder(String.Format(aFormat, aParams));
                logText.Insert(0, GetCurrentTime("HH:mm:ss.fff "));
                Console.WriteLine(logText);
                if (sLogManager.mLogDir == "")
                {
                    return 0;
                }
                // Logファイルに出力する
                System.IO.StreamWriter writer = new System.IO.StreamWriter(sLogManager.LogFileName, true,System.Text.Encoding.UTF8);
                writer.WriteLine(logText);
                writer.Close();
                writer.Dispose();
            }

            return 0;
        }


        /// <summary>
        /// エラーログを出力する
        /// </summary>
        /// <param name="aFormat">ログのフォーマット</param>
        /// <param name="aParams">出力パラメータ</param>
        /// <returns></returns>
        ///
        public static int Err(L aL, string aFormat, params object[] aParams)
        {
            lock (sLogManager)
            {
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                StringBuilder logText = new StringBuilder(String.Format(threadId.ToString("X2") + " " + aFormat, aParams));
                if (logText.ToString() == sLogManager.mLastText)
                {
                    return 0;
                }
                sLogManager.mLastText = logText.ToString();
                logText.Insert(0, GetCurrentTime("HH:mm:ss.fff ") + aL.ToString());
                // Errファイルに出力する
                System.IO.StreamWriter writer = new System.IO.StreamWriter(sLogManager.ErrorLogFileName, true);
                writer.WriteLine(logText);
                writer.Close();
                writer.Dispose();
            }

            return 0;
        }


        /// <summary>
        /// 日付指定のファイル名を取得する
        /// </summary>
        /// <param name="aLogOrError">LogかErrかの選択</param>
        /// <returns>日付指定のファイル名.フルパス</returns>
        private string GetTodayFileName(string aLogOrError)
        {
            String result = "";
            DateTime dtToday = DateTime.Today;
            StringBuilder errFileName = new StringBuilder(mLogDir + "\\" + aLogOrError + dtToday.ToString("yyyyMMdd") + ".log");
            result = errFileName.ToString();
            return result;
        }

        /// <summary>
        /// 時間を取得する(形式指定可能)
        /// </summary>
        /// <param name="aFormat"></param>
        /// <returns></returns>
        public static String GetCurrentTime(String aFormat = "HH.mm.ss")
        {
            DateTime currentDateTime = DateTime.Now;
            String currentTime = currentDateTime.ToString(aFormat);
            return currentTime;
        }

        /// <summary>
        /// スレッドIDを取得する(形式指定可能)
        /// </summary>
        /// <param name="aFormat"></param>
        /// <returns></returns>
        public static String GetCurrentThread(String aFormat = "X2")
        {
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            String currentThread = threadId.ToString(aFormat);
            return currentThread;
        }
    }

}
