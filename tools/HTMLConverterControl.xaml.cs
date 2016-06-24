using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using HTMLConverter;

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

        private void HtmlBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PreviewRichTextBox.Document = XamlReader.Parse(XamlBox.Text) as FlowDocument;
            }
            catch (Exception ex)
            {
                OutputBox.Text = ex.ToString();
            }
        }
    }
}
