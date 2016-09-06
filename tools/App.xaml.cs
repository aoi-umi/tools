using System;
using System.Windows;
using System.Windows.Threading;

namespace tools
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var result = MessageBox.Show(
                string.Format("{0}{1}{1}{2}", e.Exception.ToString(), Environment.NewLine, "是否退出?"),
                "未处理错误",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Shutdown();
            }
            e.Handled = true;
        }
    }
}
