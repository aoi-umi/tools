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

                InputTreeView.ItemsSource = GetTreeItem(InputBox.Document);
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
            TextPointer navigator = flowDocument.ContentStart;

            var text = InputString;
            if (string.IsNullOrEmpty(text)) return;
            Match match = Regex.Match(text, RegexString);
            int matchNum = 1;
            int hadReadLength = 0;
            while (match.Success)
            {
                TextPointer start = InputBox.Document.ContentStart;
                string word = match.Groups[0].ToString();
                int startPos = match.Index, endPos = startPos + word.Length;

                var model = new TreeViewItemModel() { Index = startPos, Header = "match" + matchNum + ",Index(" + startPos.ToString() + ")" };
                int groupNum = 0;
                foreach (var g in match.Groups)
                {
                    model.Items.Add(new TreeViewItemModel() { Header = "group" + groupNum + ": " + g.ToString() });
                    ++groupNum;
                }
                ++matchNum;
                dataList.Add(model);

                var textLength = 0;
                var startOffset = startPos;
                var endOffset = endPos;
                Tag t = new Tag();
                t.Word = word;
                while (start != null && flowDocument.ContentEnd.CompareTo(start) > -1 && textLength < startPos)
                {
                    var context = start.GetPointerContext(LogicalDirection.Forward);
                    if (context != TextPointerContext.Text)
                    {
                        start = start.GetNextContextPosition(LogicalDirection.Forward);
                        continue;
                    }
                    int len = start.GetTextRunLength(LogicalDirection.Forward);
                    textLength += len;
                    if(textLength < startPos) startOffset -= len;
                }
                t.StartPosition = start.GetPositionAtOffset(startOffset, LogicalDirection.Forward);

                while (start != null && flowDocument.ContentEnd.CompareTo(start) > -1 && textLength < startOffset + word.Length)
                {
                    var context = start.GetPointerContext(LogicalDirection.Forward);
                    if (context != TextPointerContext.Text)
                    {
                        start = start.GetNextContextPosition(LogicalDirection.Forward);
                        continue;
                    }
                    int len = start.GetTextRunLength(LogicalDirection.Forward);
                    textLength += len;
                    if(textLength < startPos + word.Length) endOffset -= len;
                }
                t.EndPosition = start.GetPositionAtOffset(endOffset, LogicalDirection.Forward);

                m_tags.Add(t);
                //var s = flowDocument.ContentStart;
                //while (flowDocument.ContentEnd.CompareTo(s) > -1)
                //{
                //    var x = s.GetPointerContext(LogicalDirection.Forward);
                //    s = s.GetNextContextPosition(LogicalDirection.Forward);
                //}         

                //do
                //{
                //    var context = start.GetPointerContext(LogicalDirection.Forward);
                //    while (context != TextPointerContext.Text)
                //    {
                //        if (context == TextPointerContext.ElementStart || context == TextPointerContext.ElementEnd)
                //            ++startOffset;
                //        start = start.GetNextContextPosition(LogicalDirection.Forward);
                //        context = start.GetPointerContext(LogicalDirection.Forward);
                //    }
                //    textLength += start.GetTextRunLength(LogicalDirection.Forward);
                //    start = start.GetNextContextPosition(LogicalDirection.Forward);
                //} while (textLength < startPos);

                //while (textLength - startPos - word.Length < 0 && start != null)
                //{
                //    var context = start.GetPointerContext(LogicalDirection.Forward);
                //    while (context != TextPointerContext.Text && context != TextPointerContext.None)
                //    {
                //        if (context == TextPointerContext.ElementStart)
                //            ++endOffset;
                //        start = start.GetNextContextPosition(LogicalDirection.Forward);
                //        context = start.GetPointerContext(LogicalDirection.Forward);
                //    }
                //    textLength += start.GetTextRunLength(LogicalDirection.Forward);
                //    start = start.GetNextContextPosition(LogicalDirection.Forward);
                //}





                //text = text.Substring(startPos + word.Length);
                match = match.NextMatch();
            }
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
                        model.Header = context.ToString() + ":";
                        //    start = start.GetNextContextPosition(LogicalDirection.Forward);
                        //    model.Items = GetTreeItem(document ,ref start);
                        list.Add(model);
                        break;
                    case TextPointerContext.None:
                    case TextPointerContext.ElementEnd:
                    //    return list;
                    case TextPointerContext.EmbeddedElement:
                    case TextPointerContext.Text:
                        //default:
                        model.Header = context.ToString() + ":";
                        model.Header += start.GetTextInRun(LogicalDirection.Forward);
                        list.Add(model);
                        break;
                }
                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }
            return list;
        }

        // Returns the offset for the specified position relative to any containing paragraph.
        int GetOffsetRelativeToParagraph(TextPointer position)
        {
            // Adjust the pointer to the closest forward insertion position, and look for any
            // containing paragraph.
            Paragraph paragraph = (position.GetInsertionPosition(LogicalDirection.Forward)).Paragraph;

            // Some positions may be not within any Paragraph; 
            // this method returns an offset of -1 to indicate this condition.
            return (paragraph == null) ? -1 : paragraph.ContentStart.GetOffsetToPosition(position);
        }

        // Returns a TextPointer to a specified offset into a specified paragraph. 
        TextPointer GetTextPointerRelativeToParagraph(Paragraph paragraph, int offsetRelativeToParagraph)
        {
            // Verify that the specified offset falls within the specified paragraph.  If the offset is
            // past the end of the paragraph, return a pointer to the farthest offset position in the paragraph.
            // Otherwise, return a TextPointer to the specified offset in the specified paragraph.
            return (offsetRelativeToParagraph > paragraph.ContentStart.GetOffsetToPosition(paragraph.ContentEnd))
                ? paragraph.ContentEnd : paragraph.ContentStart.GetPositionAtOffset(offsetRelativeToParagraph);
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
