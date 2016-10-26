using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace tools
{
    /// <summary>
    /// DataControl.xaml 的交互逻辑
    /// </summary>
    public partial class DataControl : UserControl
    {
        public DataControl()
        {
            InitializeComponent();
        }
        private string InputString
        {
            get { return inputBox.Text; }
        }

        private string OutputString
        {
            set { outputBox.Text = value; }
        }

        private string DataBaseName
        {
            get { return DataBaseBox.Text; }
        }

        private string DataBasePre
        {
            get { return DataBasePreBox.Text; }
        }

        private string TableName
        {
            get { return TableBox.Text; }
        }

        private string ModelName
        {
            get { return ModelBox.Text; }
        }

        private string IgnoreIdName
        {
            get
            {
                return IgnoreIDName.Text.Trim();
            }
        }

        private string IgnoreCharForProcedureString
        {
            get { return IgnoreCharForProcedure.Text; }
        }

        private string IgnoreCharForCSharpModelString
        {
            get { return IgnoreCharForCSharpModel.Text; }
        }

        private string IgnoreCharForPreAndSufString
        {
            get { return IgnoreCharForPreAndSuf.Text; }
        }

        private string IgnoreCharForSplitString
        {
            get { return IgnoreCharForSplit.Text; }
        }

        private string Prefix
        {
            get { return Pre.Text; }
        }

        private string Suffix
        {
            get { return Suf.Text; }
        }        

        private void MakeButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem ti = Tab.SelectedItem as TabItem;
            if (ti != null)
            {
                var split = InputString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                switch (ti.Name)
                {
                    case "Procedure":
                        OutputString = ForProcedure(split);
                        break;
                    case "CSharpModel":
                        OutputString = ForCSharpModel(split);
                        break;
                    case "AddPreAndSuf":
                        OutputString = ForAddPreAndSuf(split);
                        break;
                    case "SplitString":
                        OutputString = ForSplitString(split);
                        break;
                    case "RegReplaceString": 
                        OutputString = ForReplaceString(InputString);
                        break;
                    case "JsonBeautify":
                        OutputString = ForBeautifyJsonString(InputString);
                        break;
                }
            }
        }

        private string ForProcedure(string[] split)
        {
            string p = string.Empty;
            string beginif = string.Empty;
            string endif = string.Empty;
            string beginelse = string.Empty;
            string endelse = string.Empty;
            switch (DataBaseName)
            {
                case "sql server": p = "CREATE PROCEDURE p_" + TableName + "_Save\r\n{0}AS\r\nBEGIN\r\nSET NOCOUNT ON;\r\n{1}END\r\nGO\r\n";
                    beginif = beginelse = "BEGIN\r\n";
                    endif = "END\r\n"; break;
                case "mysql": p = "CREATE PROCEDURE p_" + TableName + "_Save\r\n{0}BEGIN\r\n{1}END\r\n";
                    beginif = "THEN\r\n";
                    endif = "END IF;\r\n";
                    break;
            }
            string args = "(\r\n";
            var insertString = "IF " + DataBasePre + IgnoreIdName + " is null or " + DataBasePre + IgnoreIdName + " = 0\r\n" + beginif + "INSERT  INTO " + TableName + "(\r\n";
            try
            {
                foreach (var s in split)
                {
                    var a = s.Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (a.Length < 2) throw new Exception("\r\n请按\r\narg1 int\r\narg2 int\r\n...\r\n的格式输入");
                    args += DataBasePre + a[0] + " " + a[1] + ",\r\n";
                    if ((bool)IgnoreIDBox.IsChecked && s.Trim().Split(' ')[0].ToLower() == IgnoreIdName.ToLower())
                    {
                        continue;
                    }
                    insertString += s.Trim().Split(' ')[0] + ",\r\n";
                }
                if (insertString.EndsWith(",\r\n"))
                {
                    insertString = insertString.Remove(insertString.Length - 3);
                    insertString += "\r\n";
                }
                if (args.EndsWith(",\r\n"))
                {
                    args = args.Remove(args.Length - 3);
                    args += "\r\n";
                }
                args += ")\r\n";

                insertString += ")\r\nVALUES(\r\n";
                var updateString = "ELSE \r\n" + beginelse + "UPDATE " + TableName + "\r\nSET\r\n";
                foreach (var s in split)
                {
                    if ((bool)IgnoreIDBox.IsChecked && s.Trim().Split(' ')[0].ToLower() == IgnoreIdName.ToLower())
                    {
                        continue;
                    }
                    insertString += DataBasePre + s.Trim().Split(' ')[0] + ",\r\n";
                    updateString += s.Trim().Split(' ')[0] + " = " + DataBasePre + s.Trim().Split(' ')[0] + ",\r\n";
                }
                if (insertString.EndsWith(",\r\n"))
                {
                    insertString = insertString.Remove(insertString.Length - 3, 1);
                }
                if (updateString.EndsWith(",\r\n"))
                {
                    updateString = updateString.Remove(updateString.Length - 3, 1);
                }
                insertString += ");\r\n" + endif;
                updateString += "WHERE " + IgnoreIdName + " = " + DataBasePre + IgnoreIdName + "\r\nEND\r\n";
                var output = string.Format(p, args, insertString + ((bool)UpdateBox.IsChecked ? updateString : ""));
                var ignore = IgnoreCharForProcedureString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string i in ignore)
                {
                    output = output.Replace(i, "");
                }
                return output;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string ForCSharpModel(string[] split)
        {
            try
            {
                string modelString = "[Serializable]\r\n[DataContract]\r\npublic class {0}\r\n{{\r\n{1}}}";
                string args = "";
                foreach (var s in split)
                {
                    var a = s.Trim().Replace(",", "").Replace("not null", "").Replace("null", "").Split(new char[] { ' ','\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (a.Length < 2) throw new Exception("\r\n请按\r\narg1 int\r\narg2 int\r\n...\r\n或\r\n描述 arg1 int\r\n描述 arg2 int\r\n...\r\n的格式输入");
                    string description = string.Empty;
                    string name = string.Empty;
                    string type = string.Empty;
                    if (a.Length == 2)
                    {
                        name = a[0];
                        type = a[1].ToLower();
                    }
                    else
                    {
                        description = a[0];
                        name = a[1];
                        type = a[2].ToLower();
                    }
                    if(!string.IsNullOrEmpty(description)) args += "/// <summary>\r\n/// " + description + "\r\n/// </summary>\r\n";
                    args += "[DataMember]\r\npublic ";
                    if (type.IndexOf("int") >= 0 || type.IndexOf("tinyint") >= 0)
                    {
                        args += " int " + name;
                    }
                    else if (type.IndexOf("varchar") >= 0)
                    {
                        args += " string " + name;
                    }
                    else if (type.IndexOf("float") >= 0)
                    {
                        args += " double " + name;
                    }
                    else if (type.IndexOf("decimal") >= 0)
                    {
                        args += " decimal " + name;
                    }
                    else if (type.IndexOf("bit") >= 0)
                    {
                        args += " bool " + name;
                    }
                    else if (type.IndexOf("datetime") >= 0)
                    {
                        args += " DateTime " + name;
                    }
                    else
                    {
                        args += " undefined " + name;
                    }
                    args += " { get; set; }\r\n\r\n";
                }
                var output = string.Format(modelString, ModelName, args);
                var ignore = IgnoreCharForCSharpModelString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string i in ignore)
                {
                    output = output.Replace(i, "");
                }
                return output;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string ForAddPreAndSuf(string[] split)
        {
            string output = string.Empty;
            try
            {
                foreach (var s in split)
                {
                    //var a = s.Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    output += Prefix + s.Trim() + Suffix + "\r\n";
                }
                var ignore = IgnoreCharForPreAndSufString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string i in ignore)
                {
                    output = output.Replace(i, "");
                }
                return output;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string ForSplitString(string[] split)
        {
            string output = string.Empty;
            try
            {
                int splitColumnNum = int.Parse(SplitColumnNum.Text);
                if (splitColumnNum <= 0) throw new Exception("列数请输入大于0的数字");
                foreach (var s in split)
                {
                    var a = s.Trim().Split(new string[] { SplitBox.Text }, StringSplitOptions.RemoveEmptyEntries);
                    if(a.Length >= splitColumnNum)
                        output += a[splitColumnNum - 1] + "\r\n";
                }
                var ignore = IgnoreCharForSplitString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string i in ignore)
                {
                    output = output.Replace(i, "");
                }
                return output;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string ForReplaceString(string input)
        {
            string output = string.Empty;
            try
            {
                if ((bool)IsMatchOnly.IsChecked)
                {
                    var match = Regex.Match(input, OldStringBox.Text);
                    for (; match.Success; match = match.NextMatch())
                    {
                        output += Regex.Replace(match.Groups[0].ToString(), OldStringBox.Text, Regex.Unescape(NewStringBox.Text));
                    }
                    return output;
                }
                else
                    return Regex.Replace(input, OldStringBox.Text, Regex.Unescape(NewStringBox.Text));
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string ForBeautifyJsonString(string input)
        {
            try
            {
                //格式化json字符串
                JsonSerializer serializer = new JsonSerializer();
                TextReader tr = new StringReader(input);
                JsonTextReader jtr = new JsonTextReader(tr);
                object obj = serializer.Deserialize(jtr);
                if (obj != null)
                {
                    StringWriter textWriter = new StringWriter();
                    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 2,
                        IndentChar = ' '
                    };
                    serializer.Serialize(jsonWriter, obj);
                    return textWriter.ToString();
                }
                else
                {
                    return input;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
