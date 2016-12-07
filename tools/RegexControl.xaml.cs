using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
            InputString = "http://127.0.0.1\r\nhosthttp://127.0.0.2\r\n<a href=\"http://127.0.0.3\">http://127.0.0.3</a>";
            RegexString = @"(?<!<a.*)(?:http|https)://[^\s]+";
            OutputTreeView.ItemsSource = dataList;
        }
        private string InputString
        {
            get
            {
                if (InputBox.Document == null) return string.Empty;
                TextRange documentRange = new TextRange(InputBox.Document.ContentStart, InputBox.Document.ContentEnd);
                return documentRange.Text.Trim();
            }
            set
            {
                InputBox.Document.Blocks.Clear();
                var p = new Paragraph() { LineHeight = 1 };
                p.Inlines.Add(new Run(value));
                InputBox.Document.Blocks.Add(p);
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

        Brush highlightBackground1 = Brushes.Yellow;
        Brush highlightBackground2 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80c0ff"));
        ObservableCollection<TreeViewItemModel> dataList = new ObservableCollection<TreeViewItemModel>();
        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(RegexString) || string.IsNullOrEmpty(InputString)) return;
            try
            {
                InputBox.TextChanged -= Input_TextChanged;
                Match m = Regex.Match(InputString, RegexString);
                string output = string.Empty;
                dataList.Clear();
                m_tags.Clear();
                TextRange documentRange = new TextRange(InputBox.Document.ContentStart, InputBox.Document.ContentEnd);
                documentRange.ClearAllProperties();
                CheckWordsInFlowDocument(InputBox.Document);

                for (int i = 0; i < m_tags.Count; i++)
                {
                    TextRange range = new TextRange(m_tags[i].StartPosition, m_tags[i].EndPosition);
                    range.ApplyPropertyValue(TextElement.BackgroundProperty, i % 2 == 0 ? highlightBackground1 : highlightBackground2);
                }

                OutputString = DateTime.Now.ToString("HH:mm:ss") + "\r\n共" + dataList.Count + "个匹配项";
            }
            catch (Exception ex)
            {
                OutputString = DateTime.Now.ToString("HH:mm:ss") + "\r\n" + ex.ToString();
            }

            InputBox.TextChanged += Input_TextChanged;
        }      

        new struct Tag
        {
            public TextPointer StartPosition;
            public TextPointer EndPosition;
            public string Word;
        }

        List<Tag> m_tags = new List<Tag>();
        internal void CheckWordsInFlowDocument(FlowDocument flowDocument) //do not hightlight keywords in this method
        {
            TextPointer navigator = flowDocument.ContentStart;
            
            var text = InputString;
            if (string.IsNullOrEmpty(text)) return;
            Match match = Regex.Match(text, RegexString);
            int matchNum = 1;
            while (match.Success)
            {
                TextPointer start = InputBox.Document.ContentStart;
                var startPos = match.Index;
                var model = new TreeViewItemModel() { Index = startPos, Header = "match" + matchNum + ",Index(" + startPos.ToString() + ")" };
                int groupNum = 0;
                foreach (var g in match.Groups)
                {
                    model.Items.Add(new TreeViewItemModel() { Header = "group" + groupNum + ": " + g.ToString() });
                    ++groupNum;
                }
                ++matchNum;
                dataList.Add(model);

                string trueSearchstring = match.Groups[0].ToString();
                int position = text.IndexOf(trueSearchstring);
                string word = text.Substring(position, trueSearchstring.Length);
                var textLength = 0;
                var startOffset = 0;
                
                //var s = flowDocument.ContentStart;
                //while (flowDocument.ContentEnd.CompareTo(s) > -1)
                //{
                //    var x = s.GetPointerContext(LogicalDirection.Forward);
                //    s = s.GetNextContextPosition(LogicalDirection.Forward);
                //}         
                do
                {
                    var context = start.GetPointerContext(LogicalDirection.Forward);
                    while (context != TextPointerContext.Text)
                    {
                        if(context == TextPointerContext.ElementStart)
                            ++startOffset;
                        start = start.GetNextContextPosition(LogicalDirection.Forward);
                        context = start.GetPointerContext(LogicalDirection.Forward);
                    }
                    textLength += start.GetTextRunLength(LogicalDirection.Forward);
                    start = start.GetNextContextPosition(LogicalDirection.Forward);
                } while (textLength < startPos);

                var endOffset = startOffset;
                while (textLength - startPos - word.Length < 0 && start != null)
                {
                    var context = start.GetPointerContext(LogicalDirection.Forward);
                    while (context != TextPointerContext.Text && context != TextPointerContext.None)
                    {
                        if (context == TextPointerContext.ElementStart)
                            ++endOffset;
                        start = start.GetNextContextPosition(LogicalDirection.Forward);
                        context = start.GetPointerContext(LogicalDirection.Forward);
                    }
                    textLength += start.GetTextRunLength(LogicalDirection.Forward);
                    start = start.GetNextContextPosition(LogicalDirection.Forward);
                }

                Tag t = new Tag();
                t.StartPosition = navigator.GetPositionAtOffset(startPos + startOffset, LogicalDirection.Forward);
                t.EndPosition = navigator.GetPositionAtOffset(startPos + word.Length + endOffset, LogicalDirection.Forward);
                t.Word = word;
                m_tags.Add(t);

                text = text.Substring(position + trueSearchstring.Length);
                match = match.NextMatch();
            }
        }
    }

    public class TreeViewItemModel
    {
        public int Index { get; set; }
        public string Header { get; set; }
        public List<TreeViewItemModel> Items { get; set; }
        public TreeViewItemModel()
        {
            Items = new List<TreeViewItemModel>();
        }
    }
}
