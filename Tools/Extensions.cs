using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Tools
{

    public static class Extentions
    {
        /// <summary>
        /// string.Formatの拡張。({0}{1}{2}じゃなく、{?}{?}{?}で書く)
        /// </summary>
        /// <param name="aString"></param>
        /// <param name="aArgs"></param>
        /// <returns></returns>
        public static string ExFormat(this string aString, params object[] aArgs)
        {
            var regex = new Regex(@"\{\?\}");
            for (int index = 0; ; index++)
            {
                string newFormat = regex.Replace(aString, "{" + index.ToString() + "}", 1);
                if (newFormat.CompareTo(aString) == 0)
                {
                    break;
                }
                aString = newFormat;
            }
            aString = string.Format(aString, aArgs);
            return aString;
        }

        public static int ToInt(this string aString, int aDefaultValue = 0)
        {
            int result = (int)aString.ToDouble();
            return result;
        }


        public static double ToDouble(this string aString, double aDefaultValue = 0)
        {
            try
            {
                bool isOk = double.TryParse(aString, out double result);
                if (!isOk)
                {
                    result = aDefaultValue;
                }
                return result;
            }
            catch
            {
                return aDefaultValue;
            }
        }



        /// <summary>
        /// 文字列をシングルクォートで囲む  string a="aaa"; a.Q()→"'aaa'"
        /// </summary>
        /// <param name="aString"></param>
        /// <param name="aQuote"></param>
        /// <returns></returns>
        public static string q(this string aString, string aQuote = "\'")
        {
            if (aString == null)
            {
                aString = "";
            }

            if ( aQuote == "" )
            {
                return aString;
            }

            //aString = aString.Replace("\'", "");
            //aString = aString.Replace("\"", "");
            return aQuote + aString + aQuote;
        }

        /// <summary>
        /// 文字列をDateTimeに変更する。
        /// </summary>
        /// <param name="aString"></param>
        /// <returns></returns>
        public static DateTime Date(this string aString)
        {
            DateTime date = aString.DbString().DateInt().Date();
            return date;
        }

        /// <summary>
        /// intをDateTimeに変更する 20170701→2017/07/01
        /// </summary>
        /// <param name="aIntDate"></param>
        /// <returns></returns>
        public static DateTime Date(this int aIntDate)
        {
            if (aIntDate == 0)
            {
                aIntDate = 19720101;
            }
            int year = aIntDate / 10000;
            int month = (aIntDate % 10000) / 100;
            int day = aIntDate % 100;
            DateTime dateTime = new DateTime(year, month, day);
            return dateTime;
        }

        /// <summary>
        /// 数値を日付の文字に変更する 20180701→2017/07/01
        /// </summary>
        /// <param name="aDateInt"></param>
        /// <returns></returns>
        public static string DateString(int aDateInt)
        {
            return aDateInt.Date().DateTimeString();
        }

        /// <summary>
        /// 文字列をdoubleに変更する
        /// </summary>
        /// <param name="aString"></param>
        /// <param name="aDefaultValue"></param>
        /// <returns></returns>
        public static double Double(this string aString, double aDefaultValue = 0.0)
        {
            if (double.TryParse(aString, out double value))
            {
                return value;
            }
            return aDefaultValue;
        }

        /// <summary>
        /// 文字列をintに変更する、変更できない場合は0にする
        /// </summary>
        /// <param name="aString"></param>
        /// <param name="aDefaultValue"></param>
        /// <returns></returns>
        public static int Int(this string aString, int aDefaultValue = 0)
        {
            if (int.TryParse(aString, out int value))
            {
                return value;
            }

            return aDefaultValue;
        }

        /// <summary>
        /// true=1,false=0に変更する
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static int Int(this bool aValue)
        {
            return (aValue) ? 1 : 0;
        }

        /// <summary>
        /// true=1,false=0に変更する
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static int Int(this bool? aValue)
        {
            return aValue.Value.Int();
        }

        /// <summary>
        /// 0=false,0以外=trueに変更する
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static bool Bool(this int aValue)
        {
            return aValue == 0 ? false : true;
        }
        /// <summary>
        /// yyyy/mm/dd→yyyymmdd、HH:MM:SS→HHMMSSに変換
        /// </summary>
        /// <param name="aString"></param>
        /// <returns></returns>
        public static string DbString(this string aString)
        {
            string dbString = aString.Replace(":", "").Replace("/", "");
            return dbString;
        }
        /// <summary>
        /// 日付をyyyymmddの文字に変換する
        /// </summary>
        /// <param name="aDateTime"></param>
        /// <returns></returns>
        public static string DbString(this DateTime aDateTime)
        {
            String dateString = aDateTime.DateTimeString().DbString();
            return dateString;
        }
        /// <summary>
        /// 日付をyyyymmddの文字に変換する
        /// </summary>
        /// <param name="aDateTime"></param>
        /// <returns></returns>
        public static string AboutString(this DateTime aDateTime)
        {
            DateTime now = DateTime.Now;
            if( aDateTime.Year == now.Year  && aDateTime.Month == now.Month && aDateTime.Day == now.Day )
            {
                String timeString = aDateTime.ToString("HH:mm");
                return timeString;

            }
            string dateString = aDateTime.ToString("yy/MM/dd");
            return dateString;
        }

        /// <summary>
        /// 日付型を文字列に変更する
        /// </summary>
        /// <param name="aDateTime"></param>
        /// <returns></returns>
        public static string DateTimeString(this DateTime aDateTime)
        {
            String dateString = aDateTime.ToString("yyMMddHHmmss");
//            String dateString = String.Format("{0:D4}/{1:D2}/{2:D2} {2:D2}:{2:D2}:{2:D2}", aDateTime.Year, aDateTime.Month, aDateTime.Day);
            return dateString;
        }

        public static DateTime ToDateTime(this string aDateTimeText )
        {
            int position = 0;
            int year = 2000 + aDateTimeText.Substring(position, 2).ToInt(); position += 2;
            int month = aDateTimeText.Substring(position, 2).ToInt(); position += 2;
            int day = aDateTimeText.Substring(position, 2).ToInt(); position += 2;
            int hour = aDateTimeText.Substring(position, 2).ToInt(); position += 2;
            int minut = aDateTimeText.Substring(position, 2).ToInt(); position += 2;
            int sec = aDateTimeText.Substring(position, 2).ToInt(); position += 2;

            var dateTime = new DateTime( year, month, day, hour, minut, sec );
            return dateTime;
        }

        /// <summary>
        /// 日付型を数値に変更する
        /// </summary>
        /// <param name="aDateTime"></param>
        /// <returns></returns>
        public static int DateInt(this DateTime aDateTime)
        {
            return aDateTime.Year * 10000 + aDateTime.Month * 100 + aDateTime.Day;
        }

        /// <summary>
        /// 日付型を文字列に変更する
        /// </summary>
        /// <param name="aDateTime"></param>
        /// <returns></returns>
        public static string DateString(this DateTime? aDateTime)
        {
            return aDateTime.Value.DateTimeString();
        }

        /// <summary>
        /// 日付型を数値にする
        /// </summary>
        /// <param name="aDateTime"></param>
        /// <returns></returns>
        public static int DateInt(this DateTime? aDateTime)
        {
            return aDateTime.Value.DateInt();
        }

        /// <summary>
        /// 文字列日付を数値の日付にする(2018/07/01→20180701)
        /// </summary>
        /// <param name="aDateString"></param>
        /// <returns></returns>
        public static int DateInt(this string aDateString)
        {
            aDateString = aDateString.Replace("/", "");
            return aDateString.Int();
        }

        /// <summary>
        /// Bool?をboolに変換
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static bool Bool(this bool? aValue)
        {
            if (aValue == null)
            {
                return false;
            }
            else
            {
                return aValue.Value;
            }
        }
        public static string[] ExSplit( this string aString, char aKugiri, char[] aTrimString = null, int aCount = 9999999)
        {
            string[] result = aString.Split( new char[] { aKugiri }, aCount);
            for( int i = 0; i < result.Length; i++ )
            {
                result[i] = result[i].Trim(aTrimString);
            }
            return result;
        }

        public static string[] SplitWithQuotes(this string aString, char[] aSplitter = null, char[] aTrimString = null, int aMaxSplit = 99999)
        {
            bool isMatch(char aTarget, char[] aPattern)
            {
                for (int i = 0; i < aPattern.Length; i++)
                {
                    if ((aTarget == aPattern[i]))
                    {
                        return true;
                    }
                }
                return false;
            }
            List<string> result = new List<string>();
            bool insideQuotes = false;
            int wordStartIndex = 0;

            if (aSplitter == null)
            {
                aSplitter = new char[] { ',' };
            }

            for (int i = 0; i < aString.Length; i++)
            {
                if (aString[i] == '"')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (isMatch(aString[i], aSplitter) && !insideQuotes)
                {
                    result.Add(aString.Substring(wordStartIndex, i - wordStartIndex));
                    wordStartIndex = i + 1;

                    if (result.Count == aMaxSplit - 1)
                    {
                        break;
                    }
                }
            }

            result.Add(aString.Substring(wordStartIndex));
            for( int i = 0; i < result.Count; i++ )
            {
                result[i] = result[i].Trim(aTrimString);
            }

            return result.ToArray();
        }


        public static string[] ExSplit_(this string value, char aKugiri, char[] aTrimString = null, int aCount = 9999999  )
        {
            string trim( string v, char[] c )
            {
                if( c != null )
                {
                    v.Trim(c);
                }
                return v;
            }
            var parts = new List<string>();
            var currentPart = new List<char>();
            bool inQuotes = false;
            bool isEscape = false; // エスケープ状態を追跡

            for (int i = 0; i < value.Length; i++)
            {
                char currentChar = value[i];
                if (currentChar == '\\' && !isEscape) // エスケープ文字の検出
                {
                    isEscape = true; // 次の文字はエスケープされている
                    continue; // エスケープ文字自体は追加しない
                }

                if (currentChar == '\'' && !isEscape)
                {
                    // エスケープされていないダブルクォートであれば状態をトグル
                    inQuotes = !inQuotes;
                }
                else if (currentChar == aKugiri && !inQuotes)
                {
                    // カンマで分割し、現在の部分をパーツリストに追加
                    parts.Add(trim(new string(currentPart.ToArray()), aTrimString));
                    if( parts.Count > aCount - 1 )
                    {
                        break;
                    }
                    currentPart.Clear();
                }
                else
                {
                    // 現在の文字を部分文字列に追加
                    currentPart.Add(currentChar);
                }

                // エスケープ状態のリセット (次の文字の処理の前に)
                if (isEscape) isEscape = false;
            }

            // 最後の部分を追加
            parts.Add(trim(new string(currentPart.ToArray()), aTrimString));

            return parts.ToArray();
        }
        /// <summary>
        /// 文字に含まれている指定文字の数を取得する
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static int CountText(this string aString, string aCountText)
        {
            int deleteLength = aString.Length - aString.Replace(aCountText, "").Length;
            int count = deleteLength / aCountText.Length;
            return count;
        }

        public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey aKey, TValue aValue)
        {
            if (self.ContainsKey(aKey))
            {
                self[aKey] = aValue;
                return;
            }
            self.Add(aKey, aValue);
        }

        public static bool Update<TKey,TValue>(this Dictionary<TKey, TValue> self, TKey aKey, TValue aValue)
        {
            if (self.ContainsKey(aKey))
            {
                self[aKey] = aValue;
                return true;
            }
            return false;
        }
        public static TValue Get<TKey,TValue>(this Dictionary<TKey, TValue> self, TKey aKey, TValue aDefault )
        {
            if (self.ContainsKey(aKey))
            {
                return self[aKey];
            }
            return aDefault;
        }

        public static bool TryParse<TYPE>(string aText, ref TYPE aResult)
        {
            try
            {
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(TYPE));
                if (converter != null)
                {
                    // ConvertFromString（string text）の戻りはobject
                    aResult = (TYPE)converter.ConvertFromString(aText);
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        public static int[] GetColumnWidth(this ListView aListView)
        {
            List<int> columnWidth = new List<int>();

            for (int i = 0; i < aListView.Columns.Count; i++)
            {
                columnWidth.Add(aListView.Columns[i].Width);
            }
            return columnWidth.ToArray();
        }

        public static void SetColumnWidth(this ListView aListView, int[] aWidth)
        {
            for (int i = 0; i < aListView.Columns.Count; i++)
            {
                aListView.Columns[i].Width = aWidth[i];
            }
        }

        public static int GetCurrentIndex( this ListView aListView )
        {
            if (aListView.SelectedItems == null || aListView.SelectedItems.Count <= 0)
            {
                return -1;
            }
            int index = aListView.SelectedItems[0].Index;
            return index;
        }

        public static void SetCurrentIndex( this ListView aLitView, int aIndex )
        {
            aLitView.Items[aIndex].Selected = true;
            aLitView.Items[aIndex].Focused = true;
        }

        public static string ValueToCsv<TYPE>(this TYPE[] aValue)
        {
            StringBuilder result = new StringBuilder();

            foreach (var value in aValue)
            {
                if (result.Length != 0)
                {
                    result.Append(Common.Spliter);
                }
                result.Append(value.ToString());
            }
            return result.ToString();
        }

        public static void CsvToValue<TYPE>(string aCsvText, out TYPE[] aValues) where TYPE : new()
        {
            string[] items = aCsvText.Split(Common.Spliter);

            List<TYPE> newValue = new List<TYPE>();

            for (int i = 0; i < items.Length; i++)
            {
                TYPE value = new TYPE();
                TryParse(items[i], ref value);
                newValue.Add(value);
            }
            aValues = newValue.ToArray();
        }

        public static void SavePosition(this Splitter aSplitter)
        {
            Common.SaveConfig(aSplitter.Name, aSplitter.SplitPosition.ToString() );
        }

        public static void LoadPoistion( this Splitter aSplitter)
        {
            try
            {
                string value = Common.LoadConfig(aSplitter.Name);
                aSplitter.SplitPosition = value.ToInt();
            }
            catch (Exception e)
            {

            }
        }


        public static void SaveColumn(this ListView aListView)
        {
            int[] values = GetColumnWidth(aListView);
            string csv = values.ValueToCsv();
            Common.SaveConfig( aListView.Name, csv );
        }

        public static bool LoadColumn(this ListView aListView)
        {
            string csv = null;
            try
            {
                csv = Common.LoadConfig(aListView.Name);
                if (csv == null)
                {
                    return false;
                }
                int[] values;
                CsvToValue(csv, out values);
                for( int i = 0; i < values.Length; i++ )
                {
                    if( values[i] <= 0 )
                    {
                        values[i] = 50;
                    }
                }
                aListView.SetColumnWidth(values);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string Dir(this string aString)
        {
            return Path.GetDirectoryName(aString);
        }

        public static string File(this string aString)
        {
            return Path.GetFileName(aString);
        }

        public static List<List<String>> GetCsv(this ListView aListView, bool aWithHeader = false )
        {
            var resultCsv = new List<List<String>>();

            if( aWithHeader )
            {
                List<string> headerCsv = new List<string>();
                foreach(ColumnHeader column in aListView.Columns )
                {
                    headerCsv.Add(column.Text);
                }
                resultCsv.Add(headerCsv);
            }
            var line = new List<String>();
            for (int i = 0; i < aListView.Columns.Count; i++)
            {
                line.Add( aListView.Columns[i].Text );
            }

            foreach (ListViewItem lvi in aListView.Items)
            {
                line.Clear();
                foreach (ListViewItem.ListViewSubItem subItem in lvi.SubItems)
                {
                    line.Add( subItem.Text );
                }
                resultCsv.Add( new List<String>(line.ToArray()));
            }
            return resultCsv;
        }

        public static void AddColoredText( this RichTextBox aRichTextBox, string text, Color color, bool bold = false)
        {
            aRichTextBox.SelectionStart = aRichTextBox.TextLength;
            aRichTextBox.SelectionLength = 0;

            aRichTextBox.SelectionColor = color;
            aRichTextBox.AppendText(text);
            aRichTextBox.SelectionColor = aRichTextBox.ForeColor; // Reset color
        }


    }

    class SHFileOparation
    {
        public enum FileFuncFlags : uint
        {
            FO_MOVE = 0x1,
            FO_COPY = 0x2,
            FO_DELETE = 0x3,
            FO_RENAME = 0x4
        }

        [Flags]
        public enum FILEOP_FLAGS : ushort
        {
            FOF_MULTIDESTFILES = 0x1,
            FOF_CONFIRMMOUSE = 0x2,
            /// <summary>
            /// Don't create progress/report
            /// </summary>
            FOF_SILENT = 0x4,
            FOF_RENAMEONCOLLISION = 0x8,
            /// <summary>
            /// Don't prompt the user.
            /// </summary>
            FOF_NOCONFIRMATION = 0x10,
            /// <summary>
            /// Fill in SHFILEOPSTRUCT.hNameMappings.
            /// Must be freed using SHFreeNameMappings
            /// </summary>
            FOF_WANTMAPPINGHANDLE = 0x20,
            FOF_ALLOWUNDO = 0x40,
            /// <summary>
            /// On *.*, do only files
            /// </summary>
            FOF_FILESONLY = 0x80,
            /// <summary>
            /// Don't show names of files
            /// </summary>
            FOF_SIMPLEPROGRESS = 0x100,
            /// <summary>
            /// Don't confirm making any needed dirs
            /// </summary>
            FOF_NOCONFIRMMKDIR = 0x200,
            /// <summary>
            /// Don't put up error UI
            /// </summary>
            FOF_NOERRORUI = 0x400,
            /// <summary>
            /// Dont copy NT file Security Attributes
            /// </summary>
            FOF_NOCOPYSECURITYATTRIBS = 0x800,
            /// <summary>
            /// Don't recurse into directories.
            /// </summary>
            FOF_NORECURSION = 0x1000,
            /// <summary>
            /// Don't operate on connected elements.
            /// </summary>
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,
            /// <summary>
            /// During delete operation,
            /// warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
            /// </summary>
            FOF_WANTNUKEWARNING = 0x4000,
            /// <summary>
            /// Treat reparse points as objects, not containers
            /// </summary>
            FOF_NORECURSEREPARSE = 0x8000
        }

        //[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        //If you use the above you may encounter an invalid memory access exception (when using ANSI
        //or see nothing (when using unicode) when you use FOF_SIMPLEPROGRESS flag.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FileFuncFlags wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public FILEOP_FLAGS fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }


        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);

        public static bool CopyFile(string aFrom, string aTo)
        {
            SHFILEOPSTRUCT shfos;
            shfos.hwnd = (IntPtr)null;//  this.Handle;
            shfos.wFunc = FileFuncFlags.FO_COPY;
            shfos.pFrom = aFrom;
            shfos.pTo = aTo;
            shfos.fFlags = FILEOP_FLAGS.FOF_NOCONFIRMATION;
            shfos.fAnyOperationsAborted = true;
            shfos.hNameMappings = IntPtr.Zero;
            shfos.lpszProgressTitle = null;

            SHFileOperation(ref shfos);
            return true;
        }


    }


}
