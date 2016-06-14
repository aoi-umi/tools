using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace tools
{
    /// <summary>
    /// EncodeDecodeControl.xaml 的交互逻辑
    /// </summary>
    public partial class EncodeDecodeControl : UserControl
    {
        public EncodeDecodeControl()
        {
            InitializeComponent();
            MorseList = CreateMorseList();
        }

        private string MorseDefaultShortValue = ".";
        private string MorseDefaultLongValue = "-";
        private string MorsDefaulteSplitValue = " ";
        private List<MorseModel> MorseList { get; set; }

        private string DecodeString
        {
            get { return DecodeBox.Text; }
            set { DecodeBox.Text = value; }
        }

        private string EncodeString
        {
            get { return EncodeBox.Text; }
            set { EncodeBox.Text = value; }
        }

        private void DecodeButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem ti = Tab.SelectedItem as TabItem;
            if (ti != null)
            {
                string output = string.Empty;
                switch (ti.Name)
                {
                    case "base64":
                        output = Base64(DecodeString, true);
                        break;
                    case "caesar":
                        output = Caesar(DecodeString, true);
                        break;
                    case "morse":
                        output = Morse(DecodeString, true);
                        break;
                    
                }
                EncodeString = output;
            }
        }

        private void EncodeButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem ti = Tab.SelectedItem as TabItem;
            if (ti != null)
            {
                string output = string.Empty;
                switch (ti.Name)
                {
                    case "base64":
                        output = Base64(EncodeString, false);
                        break;
                    case "caesar":
                        output = Caesar(EncodeString, false);
                        break;
                    case "morse":
                        output = Morse(EncodeString, false);
                        break;

                }
                DecodeString = output;
            }
        }

        private string Base64(string input, bool decode)
        {
            try
            {
                if (decode)
                {
                    return Encoding.Default.GetString(Convert.FromBase64String(input));
                }
                else
                {
                    return Convert.ToBase64String(Encoding.Default.GetBytes(input));
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string Caesar(string input, bool decode)
        {
            try
            {
                int offset;
                int.TryParse(CaesarOffsetBox.Text, out offset);
                int times = 1;
                if (offset == 0) times = 26;
                string output = string.Empty;

                if (decode)
                {
                    while (times > 0)
                    {
                        foreach (var s in DecodeString)
                        {
                            if (s >= 'a' && s <= 'z')
                            {
                                int b = ((s - offset - 'a') % ('z' - 'a' + 1));
                                if (b >= 0) b += 'a';
                                else b += 'z' + 1;
                                output += Convert.ToChar(b);
                            }
                            else if (s >= 'A' && s <= 'Z')
                            {
                                int b = ((s - offset - 'A') % ('Z' - 'A' + 1));
                                if (b >= 0) b += 'A';
                                else b += 'Z' + 1;
                                output += Convert.ToChar(b);
                            }
                            else output += s;
                        }
                        output += "\r\n";
                        --times;
                        --offset;
                    }
                    return output;
                }
                else
                {
                    while (times > 0)
                    {
                        foreach (var s in EncodeString)
                        {
                            if (s >= 'a' && s <= 'z')
                            {
                                int b = ((s + offset - 'z') % ('z' - 'a' + 1));
                                if (b <= 0) b += 'z';
                                else b += 'a' - 1;
                                output += Convert.ToChar(b);
                            }
                            else if (s >= 'A' && s <= 'Z')
                            {
                                int b = ((s + offset - 'Z') % ('Z' - 'A' + 1));
                                if (b <= 0) b += 'Z';
                                else b += 'A' - 1;
                                output += Convert.ToChar(b);
                            }
                            else output += s;
                        } output += "\r\n";
                        --times;
                        ++offset;
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string Morse(string input, bool decode)
        {
            try
            {
                string output = string.Empty;
                string MorseShortValue = MorseShortValueBox.Text.Trim();
                string MorseLongValue = MorseLongValueBox.Text.Trim();
                string MorseSplitValue = MorseSplitValueBox.Text.Trim();
                if (string.IsNullOrEmpty(MorseShortValue)) MorseShortValue = MorseDefaultShortValue;
                if (string.IsNullOrEmpty(MorseLongValue)) MorseLongValue = MorseDefaultLongValue;
                if (string.IsNullOrEmpty(MorseSplitValue)) MorseSplitValue = MorsDefaulteSplitValue;
                if (MorseShortValue == MorseLongValue || MorseShortValue == MorseSplitValue || MorseLongValue == MorseSplitValue) 
                    throw new Exception(string.Format("{0}、{1}、分隔符三者之间不能相同", MorseDefaultShortValue, MorseDefaultLongValue));
                var list = input.Split(new String[] { MorseSplitValue, " ","\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (decode)
                {
                    foreach (string s in list)
                    {
                        MorseModel model = MorseList.Find(x => x.Value.Replace(MorseDefaultShortValue, MorseShortValue).Replace(MorseDefaultLongValue, MorseLongValue) == s);
                        if (model != null) output += model.Text;
                        else output += s;
                    }
                    return output;
                }
                else
                {
                    foreach (string s in list)
                    {
                        foreach (var c in s)
                        {
                            MorseModel model = MorseList.Find(x => x.Text == c.ToString().ToLower());
                            if (model != null) output += model.Value.Replace(MorseDefaultShortValue, MorseShortValue).Replace(MorseDefaultLongValue, MorseLongValue);
                            else output += c;
                            output += MorseSplitValue;
                        }
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private List<MorseModel> CreateMorseList()
        {
            List<MorseModel> list = new List<MorseModel>() { 
                new MorseModel() { Text = "a",Value = ".-"},
                new MorseModel() { Text = "b",Value = "-..."},
                new MorseModel() { Text = "c",Value = "-.-."},
                new MorseModel() { Text = "d",Value = "-.."},
                new MorseModel() { Text = "e",Value = "."},
                new MorseModel() { Text = "f",Value = "..-."},
                new MorseModel() { Text = "g",Value = "--."},
                new MorseModel() { Text = "h",Value = "...."},
                new MorseModel() { Text = "i",Value = ".."},
                new MorseModel() { Text = "j",Value = ".---"},
                new MorseModel() { Text = "k",Value = "-.-"},
                new MorseModel() { Text = "l",Value = ".-.."},
                new MorseModel() { Text = "m",Value = "--"},
                new MorseModel() { Text = "n",Value = "-."},
                new MorseModel() { Text = "o",Value = "---"},
                new MorseModel() { Text = "p",Value = ".--."},
                new MorseModel() { Text = "q",Value = "--.-"},
                new MorseModel() { Text = "r",Value = ".-."},
                new MorseModel() { Text = "s",Value = "..."},
                new MorseModel() { Text = "t",Value = "-"},
                new MorseModel() { Text = "u",Value = "..-"},
                new MorseModel() { Text = "v",Value = "...-"},
                new MorseModel() { Text = "w",Value = ".--"},
                new MorseModel() { Text = "x",Value = "-..-"},
                new MorseModel() { Text = "y",Value = "-.--"},
                new MorseModel() { Text = "z",Value = "--.."},

                new MorseModel() { Text = "0",Value = "-----"},
                new MorseModel() { Text = "1",Value = ".----"},
                new MorseModel() { Text = "2",Value = "..---"},
                new MorseModel() { Text = "3",Value = "...--"},
                new MorseModel() { Text = "4",Value = "....-"},
                new MorseModel() { Text = "5",Value = "....."},
                new MorseModel() { Text = "6",Value = "-...."},
                new MorseModel() { Text = "7",Value = "--..."},
                new MorseModel() { Text = "8",Value = "---.."},
                new MorseModel() { Text = "9",Value = "----."},

                new MorseModel() { Text = ".",Value = ".-.-.-"},
                new MorseModel() { Text = ":",Value = "---..."},
                new MorseModel() { Text = ",",Value = "--..--"},
                new MorseModel() { Text = ";",Value = "-.-.-."},
                new MorseModel() { Text = "?",Value = "..--.."},
                new MorseModel() { Text = "=",Value = "-...-"},
                new MorseModel() { Text = "'",Value = ".----."},
                new MorseModel() { Text = "/",Value = "-..-."},
                new MorseModel() { Text = "!",Value = "-.-.--"},
                new MorseModel() { Text = "-",Value = "-....-"},
                new MorseModel() { Text = "_",Value = "..--.-"},
                new MorseModel() { Text = "\"",Value = ".-..-."},
                new MorseModel() { Text = "(",Value = "-.--."},
                new MorseModel() { Text = ")",Value = "-.--.-"},
                new MorseModel() { Text = "$",Value = "...-..-"},
                new MorseModel() { Text = "&",Value = "...."},
                new MorseModel() { Text = "@",Value = ".--.-."}, };
            return list;
        }
    }

    public class MorseModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
