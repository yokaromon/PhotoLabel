using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;


namespace Tools{
    public class ParameterDict
    {
        string mFileName = Common.RelativePath(@"Format.ini");
        public string FileName
        {
            get
            {
                return mFileName;
            }
        }
        Dictionary<string, int> mNameIndex = new Dictionary<string, int>();
        private EntryDict mDict = new EntryDict();// Dictionary<string, ExDict>();
        public const string COMMENT_MARK = "#";
        public const string INCLUDE_MARK = "@";

        public class ExDict : List<(string, string)>
        {
            public ExDict()
            {
            }

            public ExDict(List<(string, string)> aOrgDictionary )// : base(dictionary)
            {
                this.Clear();

                foreach ( var item in aOrgDictionary )
                {
                    this.Add(item);
                }
            }

            public string Get( string aKey, string aDefault = "" )
            {
                if( ContainsKey(aKey) )
                {
                    return this[aKey];
                }
                return aDefault;
            }

            public string[] Keys
            {
                get
                {
                    List<string> list = new List<string>();
                    foreach( var item in this )
                    {
                        list.Add(item.Item1);
                    }
                    return list.ToArray();
                }
            }

            public int IndexOf(string aKey)
            {
                var keys = Keys;
                for (int index = 0; index < keys.Length; index++)
                {
                    if( keys[index] == aKey )
                    {
                        return index;
                    }
                }
                return -1;
            }

            public string this[string aKey]
            {
                set
                {
                    int index = IndexOf(aKey);
                    if( index>=0 )
                    {
                        LogManager.Err( new L(), $"�L�[[{aKey}]���d�����Ă��܂�");
                        this[index] = (this[index].Item1, value);
                    }
                    else
                    {
                        this.Add((aKey, value));
                    }
                }
                get
                {
                    int index = IndexOf(aKey);
                    return this[index].Item2;
                }
            }

            public bool ContainsKey( string aKey )
            {
                bool result = (IndexOf(aKey) >= 0);
                return result;
            }

            public void Add( string aKey, string aValue )
            {
                base.Add((aKey, aValue));
            }

            public void Remove( string aKey )
            {
                int index = IndexOf(aKey);
                base.RemoveAt( index );
            }
        }
        class EntryDict : Dictionary<string, ExDict>
        {
            public EntryDict()
            {
            }

            public EntryDict(IDictionary<string, ExDict> dictionary) : base(dictionary)
            {
            }
        }

        static bool IsCommentLine( String aLine )
        {
            bool result = (aLine.StartsWith( ";" ) || aLine.StartsWith( "#" ) );
            return result;
        }
        class EntryIndex{
            public String mFileName;
            public List<string> mOrgList;

            public EntryIndex( string aFileName )
            {
                mFileName = aFileName;
                mOrgList = new List<string>();
            }

            public void Add( String aLine )
            {
                mOrgList.Add( aLine );
            }
        }

        static EntryDict Copy( EntryDict aEntryMap )
        {
            EntryDict entryMap = new EntryDict();
            foreach (var entryMapKey in aEntryMap.Keys)
            {
                ExDict exDict = new ExDict();
                foreach (var exDictKey in aEntryMap[entryMapKey].Keys)
                {
                    exDict.Add(exDictKey, aEntryMap[entryMapKey][exDictKey].ToString() );
                }
                entryMap.Add(entryMapKey, exDict);
            }
            return entryMap;
        }

        private ExDict  CreateDictionary( List<string> aList )
        {
            ExDict  result = new ExDict();

            int i = 0;
            try
            {
                for (i = 0; i < aList.Count; i++)
                {
                    String line = aList[i];
                    bool isCommentLine = IsCommentLine( line );
                    if(isCommentLine)
                    {
                        continue;
                    }
                    bool isEntry = Common.EntryValue(line, out string entry, out string value);
                    if (!isEntry)
                    {
                        continue;
                    }
                    if( entry.EndsWith( "[]" ))
                    {
                        if( value=="" )
                        {
                            continue;
                        }
                        int index = GetLastIndex(entry);
                        entry = entry.Replace("[]", $"[{index}]");
                    }
                    result[entry] = value;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
                LogManager.Err(new L(), ex.Message);
                //string error = list[i];
            }
            return result;
        }

        public static void ValiableExpansion( ref List<string> aString )
        {
            Dictionary<string,string> values = new Dictionary<string,string>();

            while( true )
            {
                bool isChange = false;

                for (int i = aString.Count - 1; i >= 0; i--)
                {
                    if (aString[i].StartsWith("$"))
                    {
                        var items = aString[i].Split(new char[] { '=' }, 2);
                        values.Add(items[0], items[1]);
                        aString.RemoveAt(i);
                        isChange = true;
                    }
                }


                for ( int i = 0; i < aString.Count; i++ )
                {
                    var matchs = new Regex(@"\[\$(.+?)\]").Matches(aString[i]);
                    if(matchs.Count<=0 )
                    {
                        continue;
                    }
                    for( int j = 0; j < matchs.Count; j++ )
                    {
                        string valiable = matchs[j].ToString();
                        valiable = valiable.Substring(1, valiable.Length - 2);
                        if( !values.ContainsKey(valiable) )
                        {
                            continue;
                        }
                        aString[i] = aString[i].Replace("["+valiable+"]", values[valiable]);
                    }
                }
                if (!isChange)
                {
                    break;
                }
            }
        }

        public void StructExpansion()
        {
            string[] sectionList = SectionList;

            string search = @"\[\$(.+?)\.(.+?)\]";

            for ( int i = 0; i < sectionList.Length; i++ )
            {
                var keys = GetKeyArray(sectionList[i]);
                for( int j = 0; j < keys.Length; j++ )
                {
                    string value = GetValue( sectionList[i], keys[j] );
                    var matchs = new Regex(search).Matches(value);
                    if( matchs.Count<=0 )
                    {
                        continue;
                    }
                    string org = matchs[0].ToString();
                    var items = org.Replace("[$", "").Replace("]", "").Split('.');
                    value = GetValue(items[0], items[1]);
                    string expansionValue = value.Replace(org, value);
                }
            }
        }

        //private static bool IsContinue( string aLine )
        //{
        //    bool result = aLine.Trim().EndsWith("&");
        //    return result;
        //}

        public static bool IsContinue(List<string> aList, int aCurrentLine)
        {
            if (aList.Count <= aCurrentLine+1 )
            {
                return false;
            }
            if (aList[aCurrentLine + 1].StartsWith("\t") || aList[aCurrentLine + 1].StartsWith(" "))
            {
                return true;
            }
            return false;
        }

        public static void ConnectMulitLine( List<string> lines )
        {
            for( int i = 0; i < lines.Count; i++)
            {
                while (ParameterDict.IsContinue(lines, i))
                {
                    lines[i] = lines[i] + lines[i + 1] + "\r\n";
                    lines.RemoveAt(i + 1);
                }
            }
        }

        public static void JoinContinueLine( ref  List <string> aIniFileContents )
        {
            for (int i = 0; i < aIniFileContents.Count; i++)
            {
                // 1. �擪�̃^�u��X�y�[�X���폜
                // aIniFileContents[i] = aIniFileContents[i].TrimStart('\t', ' ');

                if(aIniFileContents[i].IndexOf("_���M����") ==0 )
                {
                    int a = 5;
                }
                // 2. ������̍Ōオ&�ŏI���ꍇ�A���̍s��ǉ�
                bool isContinue = IsContinue(aIniFileContents, i);

                if ( isContinue )
                {
                    // aIniFileContents[i+1] = aIniFileContents[i+1].TrimStart('\t', ' ');
                    // aIniFileContents[i] = aIniFileContents[i].TrimEnd('&');
                    aIniFileContents[i] += aIniFileContents[i + 1];
                    aIniFileContents.RemoveAt(i + 1);
                    i--; // ���݂̍s���������ꂽ�̂ŁA���̃��[�v�ł͓����C���f�b�N�X���ēx��������
                }
            }
        }

        // �擪���X�y�[�X�̍s����������
        public static void DivideContinueLine( ref List <string> aIniFileContents )
        {
            List<string> result = new List <string>();
            for( int i = aIniFileContents.Count - 1; i >= 0; i-- )
            {
                string currentLine = aIniFileContents[i];
                if( currentLine.IndexOf("_�񎟌^�����")==0 )
                {
                    int a = 5;
                }
                currentLine = Regex.Replace(currentLine, @"(&|;)(\s+)", "$1\n$2");
                string[] line = currentLine.Split('\n');

                for( int j = line.Length - 1; j >= 0; j-- )
                {
                  result.Insert(0, line[j]);
                }
            }
            aIniFileContents = result;
        }

        public int GetLastIndex( string aKeyName )
        {
            if (!mNameIndex.ContainsKey(aKeyName))
            {
                mNameIndex.Add(aKeyName, 0);
            }
            else
            {
                mNameIndex[aKeyName] = mNameIndex[aKeyName] + 1;
            }
            return mNameIndex[aKeyName];
        }

        public ParameterDict( String aFileName )
        {
            List<string> iniFileContents = new List<string>();
            int line = 0;

            mFileName = aFileName;
            if( !File.Exists( aFileName ) )
            {
                var fs = File.CreateText( aFileName );
                fs.Close();
            }
            Common.FileToStringArrayEx(aFileName, ref iniFileContents, INCLUDE_MARK);
            for( int i = iniFileContents.Count - 1; i >= 0; i-- )
            {
                if(iniFileContents[i].StartsWith( "#") )
                {
                    iniFileContents.RemoveAt(i);
                }
            }

            JoinContinueLine(ref iniFileContents);

            ValiableExpansion(ref iniFileContents);

            string section = "";
            List<string> sectionContents = new List<string>();


            for (line = 0; line < iniFileContents.Count; line++)
            {
                if (iniFileContents[line].Length <= 1)
                {
                    continue;
                }

                if (!iniFileContents[line].StartsWith("["))
                {
                    sectionContents.Add(iniFileContents[line]);
                    continue;
                }


                string newSection = iniFileContents[line].Trim('[', ']');
                if (sectionContents.Count > 0)
                {
                    ExDict dict = CreateDictionary(sectionContents);
                    mDict.Add(section, dict);
                }

                section = newSection;
                sectionContents = new List<String>();
            }

            if (section.Length > 0)
            {
                ExDict dict = CreateDictionary(sectionContents);
                mDict.Add(section, dict);
            }

            StructExpansion();
        }

        public bool WriteString(string aSection, string aKey, string aValue )
        {
            uint result = WindowsApi.WritePrivateProfileString( aSection, aKey, aValue, mFileName );
            return (result!=0);
        }

        public bool WriteInt(string aSection, string aKey, int aValue )
        {
            bool result = WriteString(aSection, aKey, aValue.ToString());
            return result;
        }

        public int ReadInt(string aSection, string aKey, int aDefaultValiue = 0 )
        {
            string value = ReadString(aSection, aKey, null);
            if( value == null )
            {
                return aDefaultValiue;
            }
            return value.ToInt();
        }

        public String ReadString( string aSection, string aKey, string aDefaultValue = "" )
        {
            const int length = 1024;
            System.Text.StringBuilder value = new System.Text.StringBuilder(length) ;

            uint result = WindowsApi.GetPrivateProfileString( aSection, aKey, "", value, length, mFileName );
            if( result==0 )
            {
                return aDefaultValue;
            }
            return value.ToString();
        }


        public ExDict this[string aSection]
        {
            get
            {
                if( !mDict.ContainsKey( aSection ) )
                {
                    return null;
                }
                return mDict[aSection];
            }
        }

        public string[] SectionList
        {
            get
            {
                return (string[])(mDict.Keys).ToArray();
            }
        }

//        public bool SetValue(string aSection, string aKey, string aValue)
//        {
//            try
//            {
//                mDict[aSection][aKey] = aValue;
//            }catch(Exception)
//            {
//                if( !mDict.ContainsKey(aSection) )
//                {
//                    mDict.Add( aSection, new ExDict() );
//                }
//                if( !mDict[aSection].ContainsKey( aKey ) )
//                {
//                    mDict[aSection].Add( aKey, aValue );
//                }
//                return false;
//            }
//            return true;
//        }
//
//        public bool SetValue(string aSection, string aKey, RegisterInfo aRegisterInfo )
//        {
//            try
//            {
//                mDict[aSection][aKey] = aRegisterInfo;
//            }
//            catch
//            {
//                if (!mDict.ContainsKey(aSection))
//                {
//                    mDict.Add(aSection, new ExDict());
//                }
//                if (!mDict[aSection].ContainsKey(aKey))
//                {
//                    mDict[aSection].Add(aKey, aRegisterInfo);
//                }
//                return false;
//            }
//
//            return true;
//        }
//
//        public bool SetValue(string aSection, string aKey, short aValue )
//        {
//            bool result = SetValue(aSection, aKey, aValue.ToString() );
//            return result;
//        }
//
//        public bool SetValue(string aSection, string aKey, double aValue )
//        {
//            bool result = false;
//            lock( mDict )
//            {
//                result = SetValue(aSection, aKey, aValue.ToString());
//            }
//            return result;
//        }

        private bool IsExist( string aSection, string aKey )
        {
            if( !mDict.ContainsKey( aSection ))
            {
                return false;
            }
            if( !mDict[aSection].ContainsKey( aKey ) )
            {
                return false;
            }
            return true;
        }

        public String GetValue(string aSection, string aKey, string aDefaultValue = null, [CallerFilePath] string aFileName = "", [CallerLineNumber] int aLine = 0)
        {
            String value = aDefaultValue;
            try {
                if( !IsExist( aSection, aKey ) )
                {
                    if( aDefaultValue!=null )
                    {
                        return aDefaultValue;
                    }

                    return null;
                }
                lock( mDict )
                {
                    value = mDict[aSection][aKey].ToString();
                }
            }
            catch( Exception )
            {
                if( aSection!="ControlStoreId" )
                {
                    Common.Assert(false, "Ini�t�@�C�����ݒ肳��Ă��܂���[{?}]-[{?}]".ExFormat(aSection, aKey), false, aFileName, aLine);
                }
                return aDefaultValue;
            }
            return value;
        }

        public void SetValue(string aSection, string aKey, string aValue)
        {
            if(mDict[aSection].ContainsKey(aKey) )
            {
                mDict[aSection][aKey] = aValue;
            }
            else
            {
                mDict[aSection].Add(aKey, aValue);
            }
        }

        public static string GetSection( string aLine )
        {
            string section = null;

            aLine = aLine.Trim();
            if(aLine.StartsWith("[") && aLine.EndsWith("]") && aLine.Length > 2 )
            {
                section = aLine.Substring(1, aLine.Length - 2);
            }

            return section;
        }

        private static void ReplaceValues( ref List<string> aOrgList, ref int aOrgListPosition, ref List<string> aNewList, Dictionary<string,string> aKeyValue, ref string aCurrentSection )
        {
            for(; aOrgListPosition < aOrgList.Count; aOrgListPosition++ )
            {
                string newSection = GetSection(aOrgList[aOrgListPosition]);
                if( newSection != null && newSection != aCurrentSection )
                {
                    aCurrentSection = newSection;
                    return;
                }

                string[] keyValue = aOrgList[aOrgListPosition].Split(new char[] { '=' }, 2);
                if( aKeyValue.ContainsKey(keyValue[0]))
                {
                    string keyName = keyValue[0];
                    string newValue = aKeyValue[keyName];
                    aKeyValue.Remove(keyName);
                    if(newValue==null )
                    {
                        aOrgList[aOrgListPosition] = "#" + aOrgList[aOrgListPosition];
                        continue;
                    }
                    aNewList.Add($"{keyName}={newValue}");

                }
                else
                {
                    aNewList.Add(aOrgList[aOrgListPosition] );
                }
            }

            if( aKeyValue == null )
            {
                return;
            }
            foreach( string key in aKeyValue.Keys )
            {
                aNewList.Add($"{key}={aKeyValue[key]}");
            }
        }


        public static void SaveValues( string aSection, Dictionary<string,string> aKeyValues,string aInifileName = null, string aCurrentSection = null )
        {
            List<string> list = Common.FileToStringArray(aInifileName);
            JoinContinueLine(ref list);
            List<string> newList = new List<string>();

            for( int i = 0; i < list.Count; i++ )
            {
                if (list[i].StartsWith( INCLUDE_MARK))
                {
                    string path = Path.GetDirectoryName(aInifileName);
                    path = Common.RelativePath(path);
                    string nextInifile = Path.Combine(path, list[i].Substring(1));

                    SaveValues(aSection, aKeyValues, nextInifile, aCurrentSection);
                }

                string checkLine = list[i].Trim();
                if (checkLine.StartsWith("[") && checkLine.EndsWith("]") && checkLine.Length > 2)
                {
                    aCurrentSection = checkLine.Substring(1, checkLine.Length - 2);
                    newList.Add((string)list[i]);
                    continue;
                }
                if ( aCurrentSection != aSection )
                {
                    newList.Add(list[i]);
                    continue;
                }
                ReplaceValues( ref list, ref i, ref newList, aKeyValues, ref aCurrentSection );
            }
            DivideContinueLine(ref newList);
            if( !list.SequenceEqual( newList ) )
            {
                string backupPath = "..\\backup";
                backupPath = Common.RelativePath(backupPath);
                Common.BackupFile(aInifileName, backupPath, new TimeSpan(24 * 7,0,0));
                Common.StringArrayToFile(aInifileName, newList.ToArray());
            }
        }

        public static void DeleteEntry( string aSection, string[] aKeys, string aIniFileName )
        {
            Dictionary<string, string> deleteEntrys = new Dictionary<string, string>();
            for( int i = 0; i < aKeys.Length; i++ )
            {
                deleteEntrys.Add(aKeys[i], null);
            }
            SaveValues( aSection, deleteEntrys, aIniFileName );
        }


        public String[] GetValueArray( String aSection, String aKey )
        {
            String value = "";
            try {
                if (!IsExist(aSection, aKey))
                {
                    return null;
                }
                lock ( mDict )
                {
                    value = mDict[aSection][aKey].ToString();
                }
            }
            catch( Exception )
            {
                return null;
            }
            String[] items = value.Split( Common.Spliter );
            return items;
        }

        public (string,string)[] GetKeyValueArray( String aSection )
        {
            List<(String,String)> valueArray = new List<(String,String)> ();
            if (!mDict.ContainsKey(aSection))
            {
                return null;
            }

            foreach( var key in mDict[aSection].Keys )
            {
                valueArray.Add((key, mDict[aSection][key]));
            }
            return valueArray.ToArray();
        }

        public string[] GetKeyArray( String aSection )
        {
            List<string> keyArray = new List<string>();
            var keyValueArray = GetKeyValueArray(aSection);
            if( keyValueArray == null )
            {
                return null;
            }
            foreach( var keyValue in keyValueArray )
            {
                keyArray.Add(keyValue.Item1);
            }
            return keyArray.ToArray();
        }



        public short GetInt16(string aSection, string aKey, short  aDefaultValue = 0 )
        {
            string stringValue = GetValue( aSection, aKey, "" );
            if( stringValue=="" )
            {
                return aDefaultValue;
            }
            short result = Convert.ToInt16( stringValue );
            return result;
        }

        public double GetDouble(string aSection, string aKey, double  aDefaultValue = 0.0 )
        {
            string stringValue = GetValue( aSection, aKey, "" );
            if( stringValue=="" )
            {
                return aDefaultValue;
            }
            short result = Convert.ToInt16( stringValue );
            return result;
        }

        private void MakeEntryIndex( string aFileName, ref List<EntryIndex> aEntryIndexList )
        {
            string basePath = "";
            if( Path.IsPathRooted( aFileName ) )
            {
                basePath = Path.GetDirectoryName(aFileName);
            }
            EntryIndex entryIndex = new EntryIndex( aFileName );

            StreamReader reader = new StreamReader(aFileName, Encoding.UTF8);
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                entryIndex.Add(line);
            }
            reader.Close();

            aEntryIndexList.Add(entryIndex);
            for (int i = 0; i < entryIndex.mOrgList.Count; i++)
            {
                if (entryIndex.mOrgList[i].StartsWith(COMMENT_MARK))
                {
                    string includeFile = entryIndex.mOrgList[i].Remove(0, COMMENT_MARK.Length);
                    if( !Path.IsPathRooted(includeFile) )
                    {
                        includeFile = Path.Combine(basePath, includeFile);
                    }
                    List<EntryIndex> entryIndexList = new List<EntryIndex>();
                    EntryIndex includeList = new EntryIndex(includeFile);
                    MakeEntryIndex(includeFile, ref entryIndexList);
                    aEntryIndexList.AddRange(entryIndexList);
                }
            }
        }


        private static bool IsSection( string aLine )
        {
            if( aLine.StartsWith("[") )
            {
                return true;
            }
            return false;
        }

        private object sSaveLock = new object();
        private bool Save( EntryDict aWorkMap, String aFileName )
        {
            string fileName = "";
            string sectionName = "";
            StreamWriter streamWriter = null;

            List<EntryIndex> entryIndexList = new List<EntryIndex>();
            MakeEntryIndex(aFileName, ref entryIndexList);

            lock ( sSaveLock )
            {
                foreach ( var entryIndex in entryIndexList )
                {
                    if (fileName != entryIndex.mFileName)
                    {
                        if( streamWriter!=null )
                        {
                            streamWriter.Close();
                        }
                        streamWriter = new StreamWriter(entryIndex.mFileName);
                        fileName = entryIndex.mFileName;
                    }

                    foreach( var line in entryIndex.mOrgList )
                    {
                        if( IsSection( line ) )
                        {
                            if(sectionName!="")
                            {
                                // �����o����Ȃ������G���g��������Ώ����o��
                                foreach (var key in aWorkMap[sectionName].Keys)
                                {
                                    streamWriter.WriteLine(String.Format("{0}={1}", key, aWorkMap[sectionName][key]));
                                }
                            }

                            streamWriter.WriteLine(line);
                            sectionName = line.Trim( '[', ']' );
                            continue;
                        }
                        bool isCommentLine = IsCommentLine(line );
                        if( isCommentLine )
                        {
                            streamWriter.WriteLine(line);
                            continue;
                        }
                        bool isEntry = Common.EntryValue(line, out string entry, out string value);
                        if( isEntry )
                        {
                            value = GetValue(sectionName, entry);
                            streamWriter.WriteLine(String.Format("{0}={1}", entry, value)) ;
                            aWorkMap[sectionName].Remove(entry);
                            continue;
                        }
                        streamWriter.WriteLine(line);
                    }

                }
                if( streamWriter!=null )
                {
                    streamWriter.Close();
                }
            }

            return true;

        }

        static object sLockObject = new object();
        public bool Save( string aFileName, string[] aSaveParams )
        {
//            EntryDict workMap = Copy(mDict);
//
//            //            bool result = Save( workMap,aFileName );
//            //            return result;
//            lock( sLockObject )
//            {
//                Task.Run(() => Save(workMap, aFileName));
//            }
            for( int i = 0; i <  aSaveParams.Length; i++ )
            {

            }
            return true;
        }

        private EntryDict GetBinaryMap()
        {
            return mDict;
        }

        public void PutBinaryMap()
        {
//            Save(INI_FILE_NAME);
        }

        public static int DictTest()
        {
            String iniFile = @"c:\job\int\konamise\tagawa\source\GrindingMan\bin\Debug\GrindingManTest.ini";
            var dict = new ParameterDict( iniFile);
            // dict.SetValue( "LOG", "test", "1" );
            // dict.Save( iniFile );
            return 0;
        }

        class DictEnv
        {
            private static Dictionary<string, ParameterDict> _DictList = new Dictionary<string, ParameterDict>();

            public ParameterDict this[string aFileName]
            {
                get
                {
                    if (!_DictList.ContainsKey(aFileName))
                    {
                        var dict = new ParameterDict(aFileName);
                        _DictList.Add(aFileName, dict);
                    }
                    return _DictList[aFileName];
                }
            }

            static public DictEnv dict = new DictEnv();
        }
    }
}