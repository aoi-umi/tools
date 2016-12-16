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
            InputString = "http://127.0.0.1\rhosthttp://127.0.0.2\r\n<a href=\"http://127.0.0.3\">http://127.0.0.3</a>\nhttp://127.0.0.1";
            RegexString = @"(?<!<a.*)(?:http|https)://[^\s]+";
            OutputTreeView.ItemsSource = dataList;
        }
        private string InputString
        {
            get
            {
                if (InputBox.Document == null) return string.Empty;
                TextRange documentRange = new TextRange(InputBox.Document.ContentStart, InputBox.Document.ContentEnd);
                return documentRange.Text;
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
            InputBox.TextChanged -= Input_TextChanged;
            try
            {
                Match m = Regex.Match(InputString, RegexString);
                string output = string.Empty;
                dataList.Clear();
                m_tags.Clear();
                TextRange documentRange = new TextRange(InputBox.Document.ContentStart, InputBox.Document.ContentEnd);
                documentRange.ClearAllProperties();

                //InputTreeView.ItemsSource = GetTreeItem(InputBox.Document);
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
        void CheckWordsInFlowDocument(FlowDocument flowDocument) //do not hightlight keywords in this method
        {
            try
            {
                TextPointer navigator = flowDocument.ContentStart;
                var text = InputString;
                if (string.IsNullOrEmpty(text)) return;
                Match match = Regex.Match(text, RegexString);
                int matchNum = 1;
                while (match.Success)
                {
                    TextPointer start = InputBox.Document.ContentStart;
                    string word = match.Groups[0].ToString();
                    int startPos = match.Index, endOffset = word.Length;

                    var model = new TreeViewItemModel() { Index = startPos, Header = "match" + matchNum + ",Index(" + startPos.ToString() + ")" };
                    int groupNum = 0;
                    foreach (var g in match.Groups)
                    {
                        model.Items.Add(new TreeViewItemModel() { Header = "group" + groupNum + ": " + g.ToString() });
                        ++groupNum;
                    }
                    ++matchNum;
                    dataList.Add(model);

                    Tag t = new Tag();
                    t.Word = word;
                    #region 设置高亮范围
                    t.StartPosition = GetPointer(flowDocument.ContentEnd, start, startPos);
                    t.EndPosition = GetPointer(flowDocument.ContentEnd, t.StartPosition, endOffset);                    
                    m_tags.Add(t);
                    #endregion
                    match = match.NextMatch();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private TextPointer GetPointer(TextPointer docEnd, TextPointer startPointer, int offset)
        {
            int findEnd = offset, textLength = 0;
            while (startPointer != null && docEnd.CompareTo(startPointer) > -1)
            {
                while (startPointer.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text)
                {
                    TextPointerContext context = startPointer.GetPointerContext(LogicalDirection.Forward);
                    if (context == TextPointerContext.ElementEnd)
                    {
                        var next = startPointer.GetNextContextPosition(LogicalDirection.Forward);
                        if (next != null && next.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
                        {
                            textLength += 2;
                            offset -= 2;
                            startPointer = next;
                        }
                    }
                    startPointer = startPointer.GetNextContextPosition(LogicalDirection.Forward);
                }
                var len = startPointer.GetTextRunLength(LogicalDirection.Forward);
                if (textLength + len < findEnd)
                {
                    textLength += len;
                    offset -= len;
                    startPointer = startPointer.GetNextContextPosition(LogicalDirection.Forward);
                }
                else
                {
                    break;
                }
            }
            var tp = startPointer.GetPositionAtOffset(offset, LogicalDirection.Forward);
            return tp;
        }

        private List<TreeViewItemModel> GetTreeItem(FlowDocument document)
        {
            var list = new List<TreeViewItemModel>();
            var start = document.ContentStart;
            while (start != null && document.ContentEnd.CompareTo(start) > -1)
            {
                var model = new TreeViewItemModel();
                var context = start.GetPointerContext(LogicalDirection.Forward);
                //model.Header = context.ToString() + ":";
                switch (context)
                {
                    case TextPointerContext.ElementStart:
                        //    start = start.GetNextContextPosition(LogicalDirection.Forward);
                        //    model.Items = GetTreeItem(document ,ref start);                        
                    case TextPointerContext.None:
                    case TextPointerContext.ElementEnd:
                    //    return list;
                    case TextPointerContext.EmbeddedElement:
                        model.Header = context.ToString() + ":";
                        list.Add(model);
                        break;
                    case TextPointerContext.Text:
                        //default:
                        model.Header = context.ToString() + ":";
                        model.Header += "length(" + start.GetTextRunLength(LogicalDirection.Forward) + ")" + start.GetTextInRun(LogicalDirection.Forward);
                        list.Add(model);
                        break;
                }
                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }
            return list;
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
