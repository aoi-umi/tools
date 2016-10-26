using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;

namespace tools
{
    /// <summary>
    /// RegexControl.xaml 的交互逻辑
    /// </summary>
    public partial class RegexControl : UserControl
    {
        public RegexControl()
        {
            InitializeComponent();
            InputString = "http://127.0.0.1\r\nhosthttp://127.0.0.1\r\n<a href=\"http://127.0.0.1\">http://127.0.0.1</a>";
            RegexString = @"(?<!<a.*)(?:http|https)://[^\s]+";
            MatchBox.IsHighlight = true;
        }
        private string InputString
        {
            get
            {
                return InputBox.Text.Trim();
            }
            set
            {
                InputBox.Text = value;
            }
        }

        private string OutputString
        {
            set
            {
                OutputBox.Text = value;
            }
        }

        private string RegexString
        {
            get
            {
                return RegexBox.Text.Trim();
            }
            set
            {
                RegexBox.Text = value;
            }
        }        

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(RegexString) && !(string.IsNullOrEmpty(InputString)))
                {
                    Match m = Regex.Match(InputString, RegexString);
                    string output = string.Empty;
                    int matchNum = 1;
                    while (m.Success)
                    {
                        output += "match" + matchNum + ",Index(" + m.Index.ToString() + "):\r\n";
                        int groupNum = 0;
                        foreach (var g in m.Groups)
                        {
                            output += "\tgroup" + groupNum + ": " + g.ToString() + "\r\n";
                            ++groupNum;
                        }
                        m = m.NextMatch();
                        ++matchNum;
                    }
                    //string html = "<html><head><style>.f-pre {{ overflow: hidden; text-align: left; white-space: pre-wrap; word-break: break-all; word-wrap: break-word;}}" +
                    //    ".match {{ color:black;background-color:yellow;}}</style>" + 
                    //    "</head><body><pre class=\"f-pre\">{0}</body></pre></html>";
                    //if (!string.IsNullOrEmpty(output)) MatchString = string.Format(html, Regex.Replace(InputString, RegexString, "<font class=\"match\">$0</font>"));
                    //else MatchString = string.Empty;
                    MatchBox.SearchText = RegexString;
                    MatchBox.Text = InputString;
                    OutputString = output;
                }
            }
            catch (Exception ex)
            {
                MatchBox.Text = MatchBox.SearchText = string.Empty;
                OutputString = DateTime.Now.ToString("HH:mm:ss") + "\r\n" + ex.ToString();
            }
        }

        private static string ConvertExtendedASCII(string HTML)
        {
            string retVal = "";
            char[] s = HTML.ToCharArray();

            foreach (char c in s)
            {
                if (Convert.ToInt32(c) > 127)
                    retVal += "&#" + Convert.ToInt32(c) + ";";
                else
                    retVal += c;
            }

            return retVal;
        }

        private void WriteLog()
        {
            StackTrace st = new StackTrace(true);
            StackFrame[] sfs = st.GetFrames();
            if (sfs.Length >= 2)
            {
                StackFrame sf = sfs[1];
                Console.WriteLine(" File: {0}; Class: {1}; Method: {2};Line: {3};Column: {4}", sf.GetFileName(), this.GetType().Name, sf.GetMethod().Name, sf.GetFileLineNumber(), sf.GetFileColumnNumber());
            }            
        }

        //public void SuppressScriptErrors(WebBrowser wb, bool Hide)
        //{
        //    FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
        //    if (fiComWebBrowser == null) return;

        //    object objComWebBrowser = fiComWebBrowser.GetValue(wb);
        //    if (objComWebBrowser == null) return;

        //    objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide });
        //}
    }
}
