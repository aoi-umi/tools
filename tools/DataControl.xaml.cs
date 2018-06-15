using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Schema;
using System.ComponentModel;

namespace tools
{
    /// <summary>
    /// DataControl.xaml 的交互逻辑
    /// </summary>
    public partial class DataControl : UserControl
    {
        List<RegexOptionModel> regexOptionsList;
        public DataControl()
        {
            InitializeComponent();
            regexOptionsList = new List<RegexOptionModel>();
            foreach (var val in Enum.GetValues(typeof(RegexOptions))) {
                var enumVal = (RegexOptions)val;
                if (enumVal != RegexOptions.None) {
                    regexOptionsList.Add(new RegexOptionModel() {
                        RegexOption = enumVal
                    });
                }
            }
            RegOptBox.ItemsSource = regexOptionsList;
        }
        private string InputString
        {
            get { return inputBox.Text; }
        }

        private string OutputString
        {
            set { outputBox.Text = value; }
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
                        OutputString = ForJsonBeautifyString(InputString);
                        break;
                }
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
                var regOptions = RegexOptions.None;
                foreach (var item in RegOptBox.Items)
                {
                    var val = item as RegexOptionModel;
                    if (val.Selected)
                        regOptions = regOptions | val.RegexOption;
                }
                if ((bool)IsMatchOnly.IsChecked)
                {
                    var match = Regex.Match(input, OldStringBox.Text, regOptions);
                    for (; match.Success; match = match.NextMatch())
                    {
                        output += Regex.Replace(match.Groups[0].ToString(), OldStringBox.Text, Regex.Unescape(NewStringBox.Text));
                    }
                    return output;
                }
                else
                    return Regex.Replace(input, OldStringBox.Text, Regex.Unescape(NewStringBox.Text), regOptions);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string ForJsonBeautifyString(string input)
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

        private void ReplaceOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = (ReplaceOption.SelectedItem as ComboBoxItem)?.Tag.ToString();
            switch (type)
            {
                case "Json2String":
                    OldStringBox.Text = @"([\s]+)?""([\s\S]*?)""([\s]+)?";
                    NewStringBox.Text = @"\\""$2\\""";
                    break;
                case "String2Json":
                    OldStringBox.Text = @"\\""";
                    NewStringBox.Text = @"""";
                    break;
            }
        }
    }

    class RegexOptionModel: INotifyPropertyChanged
    {
        public RegexOptions RegexOption;
        public string RegexOptionName {
            get{
                return RegexOption.ToString();
            }
        }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected != value)
                {
                    selected = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Selected"));
                    }
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
