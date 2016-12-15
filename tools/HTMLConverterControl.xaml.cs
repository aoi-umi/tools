using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using HTMLConverter;
using System.Collections.Generic;

namespace tools
{
    /// <summary>
    /// HTMLConverterControl.xaml 的交互逻辑
    /// </summary>
    public partial class HTMLConverterControl : UserControl
    {
        public HTMLConverterControl()
        {
            InitializeComponent();
        }

        private void HtmlToXaml_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Text = string.Empty;
            string output = string.Empty;
            try
            {
                output = HtmlToXamlConverter.ConvertHtmlToXaml(HtmlBox.Text, true);
                XamlBox.Text = output;
                //PreviewRichTextBox.Document = XamlReader.Load(new XmlTextReader(new StringReader(output))) as FlowDocument;

            }
            catch (Exception ex)
            {
                OutputBox.Text = ex.ToString();
            }
        }

        private void PreviewOnRichTextBox_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Text = string.Empty;
            try
            {
                PreviewRichTextBox.Document = XamlReader.Parse(XamlBox.Text) as FlowDocument;
                var start = PreviewRichTextBox.Document.ContentStart;
                PreviewTreeView.ItemsSource = GetTreeItem(PreviewRichTextBox.Document, ref start);

            }
            catch (Exception ex)
            {
                OutputBox.Text = ex.ToString();
            }
        }

        private List<TreeViewItemModel> GetTreeItem(FlowDocument document,ref TextPointer tp)
        {
            var list = new List<TreeViewItemModel>();
            var start = tp;
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
                    //case TextPointerContext.None:
                    //case TextPointerContext.ElementEnd:
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

    }
}
