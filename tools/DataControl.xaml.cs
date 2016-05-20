using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                }
            }
        }

        private string ForProcedure(string[] split)
        {
            string p = "CREATE PROCEDURE p_" + TableName + "_Save\r\n{0}AS\r\nBEGIN\r\nSET NOCOUNT ON;\r\n{1}END\r\nGO\r\n";
            string args = "(\r\n";
            var insertString = "IF @" + IgnoreIdName + " is null or @" + IgnoreIdName + " = 0\r\nBEGIN\r\nINSERT  INTO " + TableName + "(\r\n";
            try
            {
                foreach (var s in split)
                {
                    var a = s.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (a.Length < 2) throw new Exception("\r\n请按\r\narg1 int\r\narg2 int\r\n...\r\n的格式输入");
                    args += "@" + a[0] + " " + a[1] + ",\r\n";
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
                var updateString = "ELSE \r\nBEGIN\r\nUPDATE " + TableName + "\r\nSET\r\n";
                foreach (var s in split)
                {
                    if ((bool)IgnoreIDBox.IsChecked && s.Trim().Split(' ')[0].ToLower() == IgnoreIdName.ToLower())
                    {
                        continue;
                    }
                    insertString += "@" + s.Trim().Split(' ')[0] + ",\r\n";
                    updateString += s.Trim().Split(' ')[0] + " = @" + s.Trim().Split(' ')[0] + ",\r\n";
                }
                if (insertString.EndsWith(",\r\n"))
                {
                    insertString = insertString.Remove(insertString.Length - 3, 1);
                }
                if (updateString.EndsWith(",\r\n"))
                {
                    updateString = updateString.Remove(updateString.Length - 3, 1);
                }
                insertString += ");\r\nEND\r\n";
                updateString += "WHERE " + IgnoreIdName + " = @" + IgnoreIdName + "\r\nEND\r\n";
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
                    var a = s.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (a.Length < 2) throw new Exception("\r\n请按\r\narg1 int\r\narg2 int\r\n...\r\n的格式输入");
                    args += "[DataMember]\r\npublic ";
                    if (a[1].ToLower().IndexOf("int") >= 0 || a[1].ToLower().IndexOf("tinyint") >= 0)
                    {
                        args += " int " + a[0];
                    }
                    else if (a[1].ToLower().IndexOf("varchar") >= 0)
                    {
                        args += " string " + a[0];
                    }
                    else if (a[1].ToLower().IndexOf("float") >= 0)
                    {
                        args += " double " + a[0];
                    }
                    else if (a[1].ToLower().IndexOf("decimal") >= 0)
                    {
                        args += " decimal " + a[0];
                    }
                    else if (a[1].ToLower().IndexOf("bit") >= 0)
                    {
                        args += " bool " + a[0];
                    }
                    else if (a[1].ToLower().IndexOf("datetime") >= 0)
                    {
                        args += " DateTime " + a[0];
                    }
                    else
                    {
                        args += " undefined " + a[0];
                    }
                    args += " { get; set; }\r\n\r\n";
                }
                var output = string.Format(modelString, ModelName, args);
                var ignore = IgnoreCharForCSharpModelString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                    var a = s.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    output += Prefix + a[0] + Suffix + "\r\n";
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
    }
}
