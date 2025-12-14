using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Tools
{
    public class Common
    {
        public static bool Init()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); // memo: Shift-JISを扱うためのおまじない
            return true;
        }
        static Encoding sSjisEnc = Encoding.GetEncoding(932);
        static char _Spliter = ',';
        public static char Spliter{
            get
            {
                return _Spliter;
            }
            set
            {
                _Spliter = value;
            }
        }


        public enum NENGOU
        {
            Meiji = 0,
            Taishou,
            Shouwa,
            Heisei
        };

        public static void Swap<TYPE>(ref TYPE aValue1, ref TYPE aValue2)
        {
            TYPE temp = aValue1;
            aValue1 = aValue2;
            aValue2 = temp;
        }

        public static string[] FindFile(string aStartDir, string aFileWildCard = "*.*", DateTime? aTime = null )
        {
            try
            {
                // 指定されたディレクトリからファイルを取得
                string[] files = Directory.GetFiles(aStartDir, aFileWildCard);

                string[] filteredFiles = files;
                // 指定日付以降のファイルをフィルタリング
                if ( aTime != null )
                {
                    filteredFiles = Array.FindAll(files, file =>
                    {
                        DateTime fileDate = File.GetLastWriteTime(file);
                        return fileDate >= aTime;
                    });
                }

                // 指定されたディレクトリからサブディレクトリを取得
                string[] subDirectories = Directory.GetDirectories(aStartDir);

                // サブディレクトリ内のファイルを再帰的に検索
                foreach (string subDir in subDirectories)
                {
                    DateTime dirDate = File.GetLastWriteTime(subDir);
                    if(dirDate < aTime)
                    {
                        continue;
                    }

                    string[] subDirFiles = FindFile(subDir, aFileWildCard, aTime);
                    if (subDirFiles.Length > 0)
                    {
                        filteredFiles = filteredFiles.Concat(subDirFiles).ToArray();
                    }
                }

                return filteredFiles;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return new string[0];
            }
        }

        public static bool LoadPosition( Control aControl)
        {
            // App.configからフォームの位置とサイズを読み込む
            var appSettings = ConfigurationManager.AppSettings;
            if (appSettings[$"{aControl.Name}.LocationX"] != null && appSettings[$"{aControl.Name}.LocationY"] != null &&
                appSettings[$"{aControl.Name}.Width"] != null && appSettings[$"{aControl.Name}.Height"] != null)
            {
                int x = int.Parse(appSettings[$"{aControl.Name}.LocationX"]);
                int y = int.Parse(appSettings[$"{aControl.Name}.LocationY"]);
                int width = int.Parse(appSettings[$"{aControl.Name}.Width"]);
                int height = int.Parse(appSettings[$"{aControl.Name}.Height"]);
                aControl.Location = new Point(x, y);
                aControl.Size = new Size(width, height);
                return true;
            }
            return false;
        }

        public static void SavePosition( Control aControl)
        {
            // フォームの位置とサイズをApp.configに保存する
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;

            try
            {
                appSettings[$"{aControl.Name}.LocationX"].Value = aControl.Location.X.ToString();
                appSettings[$"{aControl.Name}.LocationY"].Value = aControl.Location.Y.ToString();
                appSettings[$"{aControl.Name}.Width"].Value = aControl.Size.Width.ToString();
                appSettings[$"{aControl.Name}.Height"].Value = aControl.Size.Height.ToString();
            }
            catch( Exception )
            {
                appSettings.Add($"{aControl.Name}.LocationX", aControl.Location.X.ToString());
                appSettings.Add($"{aControl.Name}.LocationY", aControl.Location.Y.ToString());
                appSettings.Add($"{aControl.Name}.Width", aControl.Size.Width.ToString());
                appSettings.Add($"{aControl.Name}.Height", aControl.Size.Height.ToString());

            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        public static void LoadText( Control aControl )
        {
            // フォームの位置とサイズをApp.configに保存する
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;
            try
            {
                string key = $"{aControl.Name}.Text";
                string text = appSettings[key].Value;
                aControl.Text = text;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);

            }
            catch (Exception)
            {
                appSettings.Add($"{aControl.Name}.Text", "");
                return;
            }

        }

        public static void SaveText( Control aControl )
        {
            // フォームの位置とサイズをApp.configに保存する
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;

            try
            {
                appSettings[$"{aControl.Name}.Text"].Value = aControl.Text;
            }
            catch( Exception )
            {
                appSettings.Add($"{aControl.Name}.Text", aControl.Text);

            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        public static void LoadComboBoxItems( ComboBox aComboBox, string aFileName = null)
        {
            try
            {
                if (aFileName == null)
                {
                    aFileName = aComboBox.Name + ".dat";
                }
                var items = Common.FileToStringArray(aFileName);
                aComboBox.Items.AddRange(items.ToArray());
            }
            catch (Exception)
            {

            }
        }
        public static void SaveComboBoxItems( ComboBox aComboBox, string aFileName = null, int aMaxItemCount = 10)
        {
            if (aFileName == null)
            {
                aFileName = aComboBox.Name + ".dat";
            }
            string newText = aComboBox.Text;
            if (newText == "")
            {
                return;
            }
            if (aComboBox.Items.Count > 0 && aComboBox.Text == aComboBox.Items[0].ToString())
            {
                return;
            }
            int itemCounts = aComboBox.Items.Count - 1;
            if (itemCounts > aMaxItemCount)
            {
                itemCounts = aMaxItemCount;
            }
            for (int i = itemCounts; i >= 0; i--)
            {
                string itemText = aComboBox.Items[i].ToString();
                if (newText == itemText)
                {
                    aComboBox.Items.RemoveAt(i);
                }
            }
            aComboBox.Items.Insert(0, newText);
            List<string> items = new List<string>();
            foreach (var item in aComboBox.Items)
            {
                items.Add(item.ToString());
            }
            aComboBox.Text = newText;
            Common.StringArrayToFile(aFileName, items.ToArray());
        }


        public static int GetHoveredColumnIndex(System.Windows.Forms.ListView aListView, Point screenPoint)
        {
            // マウスのスクリーン座標からListView内の座標に変換
            Point listViewPoint = aListView.PointToClient(screenPoint);

            // ホバーしたカラムのインデックスを取得
            int columnIndex = -1;

            int left = 0;
            for (int i = 0; i < aListView.Columns.Count; i++)
            {
                int right = left + aListView.Columns[i].Width;

                if (listViewPoint.X >= left && listViewPoint.X <= right)
                {
                    columnIndex = i;
                    break;
                }

                left = right;
            }

            return columnIndex;
        }

        public static void ShowListViewHomerItem( ListView aListView, ListViewItem aItem, Point aMousePosition )
        {

            // マウスの位置をスクリーン座標で取得
            Point mousePosition = aListView.PointToScreen(aMousePosition);

            // ホバーしたカラムのインデックスを取得
            int columnIndex = Common.GetHoveredColumnIndex(aListView, mousePosition);

            // カラムのインデックスが有効な場合、サブアイテムの内容を取得
            if (columnIndex >= 0 && columnIndex < aListView.Columns.Count)
            {
                string subItemText = aItem.SubItems[columnIndex].Text;

                // ツールチップに表示
                ToolTip toolTip = new ToolTip();
                toolTip.SetToolTip(aListView, subItemText);
            }
        }

        public static string RelativePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            var uriBase = new Uri(Assembly.GetExecutingAssembly().Location);
            var newUri = new Uri(uriBase, path);
            path = newUri.LocalPath;
            return path;
        }

        public static void AddTextSjis(string aFileName, string aLine)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); // memo: Shift-JISを扱うためのおまじない
            try
            {
                // テキストをファイルに追記
                if (!File.Exists(aFileName))
                {
                    using (StreamWriter sw = File.CreateText(aFileName))
                    {
                        // 何もしません。空のファイルが作成されます。
                    }
                }

                using (StreamWriter writer = new StreamWriter(aFileName, true, Encoding.GetEncoding(932)))
                {
                    writer.WriteLine(aLine);
                }
            }
            catch (Exception e)
            {
                LogManager.Err( new L(), e.Message);
            }

        }

        // aBaseの文字
        public static string CopyString(
            string aBase,               ///< コピー元
            ref int aPosition,          ///< コピー元の文字の位置
            char aSpliter               ///< 区切り文字
        )
        {
            string toString = "";
            bool stringIn = false;

            for (int i = aPosition; i<aBase.Length; i++)
            {
                if (aBase[i] == '\"')
                {
                    stringIn = !stringIn;
                    continue;
                }
                if (stringIn)
                {
                    toString += aBase[i];
                    continue;
                }
                if (aBase[i] == aSpliter)
                {
                    aPosition = i + 1;
                    return toString;
                }
                toString += aBase[i];
            }
            aPosition = aBase.Length;
            return toString;
        }
        public static string ItemsToString(
            List<string> aItems,
            string   aDivChar = null
        )
        {
            if( aDivChar == null )
            {
                aDivChar = Common.Spliter.ToString();
            }
            string dataString = "";
            for( int i = 0; i<aItems.Count; i++ )
            {
                if( i>0 )
                {
                    dataString += aDivChar;
                }
                dataString += aItems[i];
            }
            return dataString;
        }
        public static List<string> StringToItems(
            string aString,
            char   aDivChar = '\0'
        )
        {
            int     pos = 0;
            if( aDivChar == '\0' )
            {
                aDivChar = Spliter;
            }
            List<string>    items = new List<string>();
            while(true)
            {
                int     orgPos = pos;
                string item = CopyString( aString, ref pos, aDivChar );
                if( orgPos==pos )
                {
                    break;
                }
                items.Add( item );
            }
            return items;
        }
        public static string GetDateText(
            int aYear,
            int aMonth,
            int aDay
        )
        {
            string dateText = string.Format( "{0:0000}{1:00}{2:00}", aYear, aMonth, aDay );
            return dateText;
        }
        public static string GetDateText( DateTime aDate )
        {
            string dateText = GetDateText( aDate.Year, aDate.Month, aDate.Day );
            return dateText;
        }

        public static void GetDateFromText(
            string aDateText,
            ref int aYear,
            ref int aMonth,
            ref int aDay
        )
        {
            aYear = int.Parse(aDateText.Substring(0, 4));
            aMonth = int.Parse(aDateText.Substring(4, 2));
            aDay = int.Parse(aDateText.Substring(6, 2));
        }

        public static System.Text.Encoding GetFileEncoding( string aFilename )
        {
            byte[] bs = System.IO.File.ReadAllBytes(aFilename);
            //文字コードを取得する
            System.Text.Encoding encoding = GetCode(bs);
            //デコードして表示する
            return encoding;
        }

        public static Color StringToColor(string aColor)
        {
            if (aColor[0] == '#')
            {
                int red = Convert.ToInt32(aColor.Substring(1, 2), 16);
                int green = Convert.ToInt32(aColor.Substring(3, 2), 16);
                int blue = Convert.ToInt32(aColor.Substring(5, 2), 16);

                return Color.FromArgb(red, green, blue);
            }
            else
            {
                return Color.FromName(aColor);
            }
        }

        public static int Assert(bool aCondition, string aMessage, bool aShowMessage=true, [CallerFilePath] string aFileName = "", [CallerLineNumber] int aLine = 0)
        {
            if (aCondition)
            {
                return 1;
            }
            StackTrace stackTrace = new StackTrace(true);
            string message = "ソースファイル{?}({?}行目".ExFormat(aFileName, aLine);
            if (aMessage != null)
            {
                message = message + "\r\n" + aMessage;
            }
            if( aShowMessage )
            {
                MessageBox.Show(message);
            }
            return 0;
        }


        public static bool EntryValue(string aLine, out string aEntry, out string aValue, char aSplitChar = '=')
        {
            string[] items = aLine.Split(new char[] { aSplitChar }, 2);

            if (items.Length <= 0)
            {
                aEntry = "";
                aValue = "";
                return false;
            }
            if( items.Length ==1 )
            {
                aEntry = items[0].Trim();
                aValue = "";
                return true;
            }
            aEntry = items[0].Trim();
            aValue = items[1].Trim();
            return true;
        }


        public static void FileToStringArrayEx(
            string aFileName,
            ref List<string> aStringArray,
            string aIncludeMark = "@"
        )
        {
            string basePath = "";

            if (Path.IsPathRooted(aFileName))
            {
                basePath = Path.GetDirectoryName(aFileName);
            }
            aStringArray = FileToStringArray(aFileName );
            string currentLine = "";
            try
            {
                for (int i = aStringArray.Count-1; i>=0; i--)
                {
                    currentLine = aStringArray[i];
                    if (!aStringArray[i].StartsWith(aIncludeMark))
                    {
                        continue;
                    }
                    aStringArray.RemoveAt(i);
                    List<string> includeList = new List<string>();
                    string includeFile = currentLine.Remove(0, aIncludeMark.Length);
                    if (!Path.IsPathRooted(includeFile))
                    {
                        includeFile = Path.Combine(basePath, includeFile);
                    }

                    FileToStringArrayEx(includeFile, ref includeList );

                    aStringArray.InsertRange(i, includeList);
                }
            }
            catch( Exception ex )
            {
                LogManager.Err( new L(), $"{currentLine}:{ex.Message}" );
            }


        }


        public static List<string>    FileToStringArray(
            string aFileName,
            string aCommentHeader=null
        )
        {
            System.Text.Encoding encoding = GetFileEncoding(aFileName);

            List<string> result = new List<string>();
            result.Clear();
            try
            {
                StreamReader reader = new StreamReader(aFileName, Encoding.UTF8);
                if (reader == null)
                {
                    return null; ;
                }
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    if (aCommentHeader != null)
                    {
                        if (line.IndexOf(aCommentHeader) == 0)
                        {
                            continue;
                        }
                    }
                    result.Add(line);
                }
                reader.Close();
            }catch(Exception)
            {
                return null;
            }
            return result;
        }
        public static void StringArrayToFile(
            string aFileName,
            string[] aStringArray,
            Encoding aEncoding = null
        )
        {
            if( aEncoding == null )
            {
                aEncoding = Encoding.UTF8;
            }
            StreamWriter writer = new StreamWriter(aFileName, false, aEncoding);
            writer.NewLine = "\n";
            foreach( string line in aStringArray )
            {
                writer.WriteLine( line );
            }
            writer.Close();
        }

        public static void BackupFile(string aFileName, string aBackupDir, TimeSpan aDeleteSpan)
        {

                // バックアップディレクトリが存在しない場合は作成する
                CreateDirectory(aBackupDir);

                string backupFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(aFileName)}";
                string originalFilePath = Path.Combine(Directory.GetCurrentDirectory(), aFileName);
                string backupFilePath = Path.Combine(aBackupDir, backupFileName);

                // ファイルのコピー
                File.Copy(originalFilePath, backupFilePath);

                // 削除期限を計算
                DateTime deleteThreshold = DateTime.Now - aDeleteSpan;

                // バックアップディレクトリ内の古いファイルを削除
                string[] filesInBackupDir = Directory.GetFiles(aBackupDir);
                foreach (string file in filesInBackupDir)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < deleteThreshold)
                    {
                        File.Delete(file);
                    }
                }
        }

        public static bool IsMatchFileName(string input, string pattern)
        {
            // ワイルドカードを正規表現パターンに変換
            string regexPattern = "^" + Regex.Escape(pattern)
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".")
                    + "$";

            // 正規表現で一致するかどうかを確認
            bool result = Regex.IsMatch(input, regexPattern);
            return result;
        }

        public static bool RawFileCopy(string srcName, string destName)
        {
            const int BUFSIZE = 2048; // 1度に処理するサイズ
            byte[] buf = new byte[BUFSIZE]; // 読み込み用バッファ
            byte[] ZEROARRAY = new byte[BUFSIZE]; // 0埋め用

            int readSize; // Readメソッドで読み込んだバイト数

            using (FileStream src = new FileStream(
                srcName, FileMode.Open, FileAccess.Read))
            using (FileStream dest = new FileStream(
                destName, FileMode.Create, FileAccess.Write))
            {

                while (true)
                {
                    try
                    {
                        readSize = src.Read(buf, 0, BUFSIZE); // 読み込み
                    }
                    catch
                    {
                        return false;
                        //// 読み込みに失敗した場合
                        //Console.WriteLine("read error at " + src.Position);

                        //if (src.Length - src.Position < BUFSIZE)
                        //{
                        //    readSize = (int)(src.Length - src.Position);
                        //}
                        //else
                        //{
                        //    readSize = BUFSIZE;
                        //}
                        //src.Seek(readSize, SeekOrigin.Current);
                        //dest.Write(ZEROARRAY, 0, readSize); // 0埋めで書き込み
                        //continue;
                    }
                    if (readSize == 0)
                    {
                        break; // コピー完了
                    }
                    dest.Write(buf, 0, readSize); // 書き込み
                }
            }
            return true;
        }

        public static bool CopyFile(string aFrom, string aTo, bool aIsMove = true)
        {
            bool result = true;
            try
            {
                if (aIsMove)
                {
                    File.Move(aFrom, aTo);
                }
                else
                {
                    File.Copy(aFrom, aTo);
                }
            }
            catch (Exception ex)
            {
                result = false;
                MessageBox.Show(ex.Message, "エラー");
            }
            return result;
        }

        /// <summary>
        /// 指定したファイルを削除する。ファイル名にはワイルドカードのしても可。
        /// </summary>
        /// <param name="aFileName"></param>
        /// <returns>true=成功,false=失敗</returns>
        public static bool DeleteFiles(String aFileName)
        {
            try
            {
                String[] files = Directory.GetFiles(
                    System.IO.Path.GetDirectoryName(aFileName), System.IO.Path.GetFileName(aFileName));

                foreach (var file in files)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static int Find(
            List<string> aStringArray,
            string aText
        )
        {
            for (int i = 0; i < aStringArray.Count; i++)
            {
                if (aStringArray[i].CompareTo(aText) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private static int FindProfileEntryIndex(
            List<string> aProfileText,
            string aSection,
            string aEntry
        )
        {
            string section = "[" + aSection + "]";
            // セクションを探す
            int sectionIndex = Find(aProfileText, section);

            if (sectionIndex == -1)
            {
                return -1;
            }
            int entryIndex = -1;
            for (int index = sectionIndex + 1; index<aProfileText.Count; index++)
            {
                int pos = 0;
                string entry = Common.CopyString(aProfileText[index], ref pos, '=');
                entry = entry.Trim();
                if (aEntry.CompareTo(entry) == 0)
                {
                    entryIndex = index;
                    break;
                }
            }
            return entryIndex;
        }

        public static bool GetProfileText(
            string aFileName,
            string aSectionName,
            string aEntryName,
            ref string aText
        )
        {
            List <string> profileText =  FileToStringArray(aFileName);
            int entryIndex = FindProfileEntryIndex( profileText,aSectionName, aEntryName);
            if (entryIndex < 0)
            {
                return false;
            }
            int pos = 0;
            Common.CopyString(profileText[entryIndex], ref pos, '=');
            aText = Common.CopyString(profileText[entryIndex], ref pos, '=');
            return true;
        }

        public static string GetProfileText(
            string aFileName,
            string aSectionName,
            string aEntryName
        )
        {
            string profileText = "";
            GetProfileText( aFileName, aSectionName, aEntryName, ref profileText );
            return profileText;
        }

        public static bool PutProfileText(
            string aFileName,
            string aSectionName,
            string aEntryName,
            string aText
        )
        {
            List<string> profileText = null;
            try
            {
                profileText = FileToStringArray(aFileName);
            }catch(Exception)
            {
                // 読めなかった場合は無視
            }
            string entryLine = aEntryName + "=" + aText;

            int entryIndex = FindProfileEntryIndex(profileText, aSectionName, aEntryName);
            if (entryIndex >= 0)
            {
                profileText[entryIndex] = entryLine;
                StringArrayToFile(aFileName, profileText.ToArray());
                return true;

            }

            string section = "[" + aSectionName + "]";
            int sectionIndex = Find(profileText, section);
            if (sectionIndex == -1)
            {
                // セクションがなかった場合
                profileText.Add(section);
                profileText.Add(entryLine);
            }
            else
            {
                // エントリがなかった場合
                profileText.Insert(sectionIndex + 1, entryLine);
            }

            StringArrayToFile(aFileName, profileText.ToArray());

            return true;
        }

        public static void SetComboItem(
            ref ComboBox    aComboBox,
            List<string>    aItems
        )
        {
            for (int i = 0; i < aItems.Count; i++)
            {
                aComboBox.Items.Add(aItems[i]);
            }
        }

        public static void Execute(string aFileName, bool aIsExplorer = true)
        {
            if( aFileName=="" )
            {
                return;
            }
            if (!aIsExplorer)
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(aFileName);
                return;
            }

            if( Directory.Exists(aFileName))
            {
                System.Diagnostics.Process.Start("EXPLORER.EXE", @"""{?}""".ExFormat(aFileName));
                return;
            }

            if (System.IO.File.Exists(aFileName))
            {
                System.Diagnostics.Process.Start("EXPLORER.EXE", @"/select,""{?}""".ExFormat(aFileName));
                return;
            }


            string dir = System.IO.Path.GetDirectoryName(aFileName);
            System.Diagnostics.Process.Start("EXPLORER.EXE", @"""{?}""".ExFormat(dir));

        }


        public static string DictToString<K, V>( IDictionary<K, V> aDictonary )
        {
            List<string> items = new List<string>();
            foreach (KeyValuePair<K, V> kvp in aDictonary)
            {
                items.Add(kvp.Key.ToString() + "=" + kvp.Value.ToString());
            }
            //var items = from keyValue in aDictonary
            //            select keyValue.Key.ToString() + ":" + keyValue.Value.ToString();

            return string.Join(Common.Spliter.ToString(), items);
        }

        public static Dictionary<string, string> DictFromString(string aDictCsv)
        {
            var dict = new Dictionary<string, string>();
            var items = aDictCsv.Split(',');
            foreach (var item in items)
            {
                if( item.Length <= 0 )
                {
                    continue;
                }
                var keyName = item.Split('=');
                dict.Add(keyName[0], keyName[1]);
            }
            return dict;
        }

        public static bool IsZenkaku(string str) {
          int num = sSjisEnc.GetByteCount(str);
          return num == str.Length * 2;
        }

        public static bool IsHankaku(string str) {
          int num = sSjisEnc.GetByteCount(str);
          return num == str.Length;
        }

        private static System.Text.Encoding GetCode(byte[] bytes)
        {
            const byte bEscape = 0x1B;
            const byte bAt = 0x40;
            const byte bDollar = 0x24;
            const byte bAnd = 0x26;
            const byte bOpen = 0x28;    //'('
            const byte bB = 0x42;
            const byte bD = 0x44;
            const byte bJ = 0x4A;
            const byte bI = 0x49;

            int len = bytes.Length;
            byte b1, b2, b3, b4;

            //Encode::is_utf8 は無視

            bool isBinary = false;
            for (int i = 0; i < len; i++)
            {
                b1 = bytes[i];
                if (b1 <= 0x06 || b1 == 0x7F || b1 == 0xFF)
                {
                    //'binary'
                    isBinary = true;
                    if (b1 == 0x00 && i < len - 1 && bytes[i + 1] <= 0x7F)
                    {
                        //smells like raw unicode
                        return System.Text.Encoding.Unicode;
                    }
                }
            }
            if (isBinary)
            {
                return null;
            }

            //not Japanese
            bool notJapanese = true;
            for (int i = 0; i < len; i++)
            {
                b1 = bytes[i];
                if (b1 == bEscape || 0x80 <= b1)
                {
                    notJapanese = false;
                    break;
                }
            }
            if (notJapanese)
            {
                return System.Text.Encoding.ASCII;
            }

            for (int i = 0; i < len - 2; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                b3 = bytes[i + 2];

                if (b1 == bEscape)
                {
                    if (b2 == bDollar && b3 == bAt)
                    {
                        //JIS_0208 1978
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    else if (b2 == bDollar && b3 == bB)
                    {
                        //JIS_0208 1983
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    else if (b2 == bOpen && (b3 == bB || b3 == bJ))
                    {
                        //JIS_ASC
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    else if (b2 == bOpen && b3 == bI)
                    {
                        //JIS_KANA
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    if (i < len - 3)
                    {
                        b4 = bytes[i + 3];
                        if (b2 == bDollar && b3 == bOpen && b4 == bD)
                        {
                            //JIS_0212
                            //JIS
                            return System.Text.Encoding.GetEncoding(50220);
                        }
                        if (i < len - 5 &&
                            b2 == bAnd && b3 == bAt && b4 == bEscape &&
                            bytes[i + 4] == bDollar && bytes[i + 5] == bB)
                        {
                            //JIS_0208 1990
                            //JIS
                            return System.Text.Encoding.GetEncoding(50220);
                        }
                    }
                }
            }

            //should be euc|sjis|utf8
            //use of (?:) by Hiroki Ohzaki <ohzaki@iod.ricoh.co.jp>
            int sjis = 0;
            int euc = 0;
            int utf8 = 0;
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((0x81 <= b1 && b1 <= 0x9F) || (0xE0 <= b1 && b1 <= 0xFC)) &&
                    ((0x40 <= b2 && b2 <= 0x7E) || (0x80 <= b2 && b2 <= 0xFC)))
                {
                    //SJIS_C
                    sjis += 2;
                    i++;
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((0xA1 <= b1 && b1 <= 0xFE) && (0xA1 <= b2 && b2 <= 0xFE)) ||
                    (b1 == 0x8E && (0xA1 <= b2 && b2 <= 0xDF)))
                {
                    //EUC_C
                    //EUC_KANA
                    euc += 2;
                    i++;
                }
                else if (i < len - 2)
                {
                    b3 = bytes[i + 2];
                    if (b1 == 0x8F && (0xA1 <= b2 && b2 <= 0xFE) &&
                        (0xA1 <= b3 && b3 <= 0xFE))
                    {
                        //EUC_0212
                        euc += 3;
                        i += 2;
                    }
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if ((0xC0 <= b1 && b1 <= 0xDF) && (0x80 <= b2 && b2 <= 0xBF))
                {
                    //UTF8
                    utf8 += 2;
                    i++;
                }
                else if (i < len - 2)
                {
                    b3 = bytes[i + 2];
                    if ((0xE0 <= b1 && b1 <= 0xEF) && (0x80 <= b2 && b2 <= 0xBF) &&
                        (0x80 <= b3 && b3 <= 0xBF))
                    {
                        //UTF8
                        utf8 += 3;
                        i += 2;
                    }
                }
            }
            //M. Takahashi's suggestion
            //utf8 += utf8 / 2;

            System.Diagnostics.Debug.WriteLine(
                string.Format("sjis = {0}, euc = {1}, utf8 = {2}", sjis, euc, utf8));
            if (euc > sjis && euc > utf8)
            {
                //EUC
                return System.Text.Encoding.GetEncoding(51932);
            }
            else if (sjis > euc && sjis > utf8)
            {
                //SJIS
                return System.Text.Encoding.GetEncoding(932);
            }
            else if (utf8 > euc && utf8 > sjis)
            {
                //UTF8
                return System.Text.Encoding.UTF8;
            }

            return null;
        }
        public readonly static Dictionary<string,string> DictHalfFullAlphabet
        = new Dictionary<string,string>()
        {
            { "А", "A"},{ "В", "B" },{"С","C" },{"Т","T" },{"Е","E" },{"О","O"},
            { "Р","P" },{ "М", "M" },{"н","H" },
            { "а", "a"},{"о","o" },{"в","B" },{"е","e" },{"р","p" },{"с","c" },{"т","T" },{"ь", "b" },

            { "Ａ", "A"},{ "Ｂ", "B" },{"Ｃ", "C"},{ "Ｄ","D"},{"Ｅ","E" },{"Ｆ","F"},
            { "Ｇ", "G"},{ "Ｈ", "H" },{"Ｉ", "I"},{ "Ｊ","J"},{"Ｋ","K" },{"Ｌ","L"},
            { "Ｍ", "M"},{ "Ｎ", "N" },{"Ｏ", "O"},{ "Ｐ","P"},{"Ｑ","Q" },{"Ｒ","R"},
            { "Ｓ", "S"},{ "Ｔ", "T" },{"Ｕ", "U"},{ "Ｖ","V"},{"Ｗ","W" },{"Ｘ","X"},
            { "Ｙ", "Y"},{ "Ｚ", "Z" },
            { "ａ", "a"},{ "ｂ", "b" },{"ｃ", "c"},{ "ｄ","d"},{"ｅ","e" },{"ｆ","f"},
            { "ｇ", "g"},{ "ｈ", "h" },{"ｉ", "i"},{ "ｊ","j"},{"ｋ","k" },{"ｌ","l"},
            { "ｍ", "m"},{ "ｎ", "n" },{"ｏ", "o"},{ "ｐ","p"},{"ｑ","q" },{"ｒ","r"},
            { "ｓ", "s"},{ "ｔ", "t" },{"ｕ", "u"},{ "ｖ","v"},{"ｗ","w" },{"ｘ","x"},
            { "ｙ", "y"},{ "ｚ", "z" },

            { "（", "("},{ "）", ")" },

            {"ア","ｱ"},{"イ","ｲ"},{"ウ","ｳ"},{"エ","ｴ"},{"オ","ｵ"},
            {"カ","ｶ"},{"キ","ｷ"},{"ク","ｸ"},{"ケ","ｹ"},{"コ","ｺ"},
            {"サ","ｻ"},{"シ","ｼ"},{"ス","ｽ"},{"セ","ｾ"},{"ソ","ｿ"},
            {"タ","ﾀ"},{"チ","ﾁ"},{"ツ","ﾂ"},{"テ","ﾃ"},{"ト","ﾄ"},
            {"ナ","ﾅ"},{"ニ","ﾆ"},{"ヌ","ﾇ"},{"ネ","ﾈ"},{"ノ","ﾉ"},
            {"ハ","ﾊ"},{"ヒ","ﾋ"},{"フ","ﾌ"},{"ヘ","ﾍ"},{"ホ","ﾎ"},
            {"マ","ﾏ"},{"ミ","ﾐ"},{"ム","ﾑ"},{"メ","ﾒ"},{"モ","ﾓ"},
            {"ヤ","ﾔ"},{"ユ","ﾕ"},{"ヨ","ﾖ"},{"ワ","ﾜ"},{"ヲ","ｦ"},{"ン","ﾝ"},
            {"ラ","ﾗ"},{"リ","ﾘ"},{"ル","ﾙ"},{"レ","ﾚ"},{"ロ","ﾛ"},
            {"ャ","ｬ"},{"ュ","ﾕ"},{"ョ","ｮ"}
        };

        public static bool IsShiftJis( string aText )
        {
            Init();
            // Shift-JISエンコーディングを取得
            Encoding sjis = Encoding.GetEncoding(932);

            // 文字列をShift-JISにエンコード
            byte[] sjisBytes = sjis.GetBytes(aText);

            // エンコードされたバイト配列を文字列にデコード
            string decodedString = sjis.GetString(sjisBytes);

            // 元の文字列とデコード後の文字列が同じか確認
            bool isShiftJis = decodedString == aText;
            return isShiftJis;
        }

        public static string ShiftJisToUtf8( string aText )
        {
            var shiftjis = Encoding.GetEncoding(932);

            byte[] shiftJisBytes = shiftjis.GetBytes(aText);
            byte[] utf8Bytes = Encoding.Convert(shiftjis, Encoding.UTF8, shiftJisBytes);
            string result = Encoding.UTF8.GetString(utf8Bytes);

            return result;
        }

        public static string ZenToHan( string aText )
        {
            // Shift-JISからUTF-8への変換
            if( IsShiftJis( aText ) )
            {
                aText = ShiftJisToUtf8( aText );
            }

            string hankakuText = aText;
            foreach (var item in DictHalfFullAlphabet)
            {
                string full = item.Key;
                string half = item.Value;
                hankakuText = hankakuText.Replace(full, half);
            }

            return hankakuText;
        }

        public static string FileNameConvert(string aFileName)
        {
            // ファイル名に使用できない文字の正規表現
            string invalidCharsPattern = "[\\\\/:*?\"<>|]";

            // 全角に変換する文字のマッピング
            var charMap = new Dictionary<char, char>
        {
            { '\\', '￥' },
            { '/', '／' },
            { ':', '：' },
            { '*', '＊' },
            { '?', '？' },
            { '"', '”' },
            { '<', '＜' },
            { '>', '＞' },
            { '|', '｜' }
        };

            // 正規表現を使ってファイル名に使用できない文字を検出し、全角に変換する
            string convertedFileName = Regex.Replace(aFileName, invalidCharsPattern, match => charMap[match.Value[0]].ToString());

            return convertedFileName;
        }

        public static string PathCombine( string aDir, string aFilename )
        {
            string path = aDir + "/" + aFilename;
            path = path.Replace("\\/", "\\");
            path = path.Replace("/\\", "\\");
            path = path.Replace("//", "\\");
            path = path.Replace("/", "\\");
            return path;
        }

        public static string GetTempFilename( string aExt = "tmp" )
        {
            string tempFilename = Path.ChangeExtension(Path.GetTempFileName(), aExt);
            return tempFilename;
        }

        public static System.Configuration.ExeConfigurationFileMap CONFIG_NAME
        {
            get
            {
                var mapfile = new ExeConfigurationFileMap { ExeConfigFilename = Path.ChangeExtension(Application.ExecutablePath, "cfg") };
                return mapfile;
            }
        }
        public static void SaveConfig( string aName, string aValue )
        {
            var config = ConfigurationManager.OpenMappedExeConfiguration(CONFIG_NAME, ConfigurationUserLevel.None);
            config.AppSettings.Settings.Add(aName, aValue);
            config.AppSettings.Settings[aName].Value = aValue;

            config.Save();
        }

        public static string LoadConfig( string aName )
        {
            var config = ConfigurationManager.OpenMappedExeConfiguration(CONFIG_NAME, ConfigurationUserLevel.None);
            var value = config.AppSettings.Settings[aName];
            if (value == null )
            {
                return null;
            }
            return value.Value;

        }

        public static DirectoryInfo CreateDirectory( string path )
        {
            if (Directory.Exists(path))
            {
                return null;
            }
            return Directory.CreateDirectory(path);
        }

        static ProcessMan sProcessMan = new ProcessMan();
        public static int ExecuteProcess(string aCommandLine, string aArgment, int aTimeOut)
        {
            DateTime start = DateTime.Now;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(aCommandLine, aArgment);
            bool result = p.WaitForExit(aTimeOut * 1000);
            sProcessMan.Add(p.Id);
            if( result)
            {
                sProcessMan.Del(p.Id);
                TimeSpan span = DateTime.Now - start;
                return span.Seconds / 1000;
            }
            // 指定時間内に終わらなければ強制終了する
            KillProcess(p.Id);
            sProcessMan.Del(p.Id);

            return 0;
        }

        public static void KillProcess( int aPid )
        {
            // 指定時間内に終わらなければ強制終了する
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "taskkill.exe";
            processInfo.Arguments = $"/f /t /PID {aPid}";
            processInfo.CreateNoWindow = true;
            Process.Start(processInfo);

            sProcessMan.Del(aPid);
        }


        private class ProcessMan : IDisposable
        {
            List<int> _PidList = new List<int>();
            public ProcessMan()
            {

            }

            ~ProcessMan()
            {
                Dispose();
            }

            public void Add(int aId)
            {
                lock (_PidList)
                {
                    _PidList.Add(aId);
                }
            }

            public void Del(int aId)
            {
                lock (_PidList)
                {
                    for (int i = 0; i < _PidList.Count; i++)
                    {
                        if (_PidList[i] == aId)
                        {
                            _PidList.RemoveAt(i);
                            break;
                        }
                    }
                }

            }

            public void Dispose()
            {
                lock (_PidList)
                {
                    for (int i = _PidList.Count - 1; i >= 0; i--)
                    {
                        Common.KillProcess(_PidList[i]);
                        Del(_PidList[i]);
                    }
                }
            }
        }
    }
}