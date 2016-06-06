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
    /// EncodeDecodeControl.xaml 的交互逻辑
    /// </summary>
    public partial class EncodeDecodeControl : UserControl
    {
        public EncodeDecodeControl()
        {
            InitializeComponent();
        }

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
    }
}
