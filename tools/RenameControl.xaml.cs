using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace tools
{
    /// <summary>
    /// RenameControl.xaml 的交互逻辑
    /// </summary>
    public partial class RenameControl : UserControl
    {
        public RenameControl()
        {
            InitializeComponent();
            bgWorker = new BackgroundWorker();
            FileList = new ObservableCollection<FileInfoModel>();

            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            FileListView.ItemsSource = FileList;
            CurrView = RenameByNewNameView;
            AddCharset();
        }

        private BackgroundWorker bgWorker { get; set; }

        private string FilePath
        {
            get { return PathBox.Text; }
            set { if (PathBox.Text != value) PathBox.Text = value; }
        }

        private string FilterString
        {
            get { return FilterBox.Text; }
            set { if (FilterBox.Text != value) FilterBox.Text = value; }
        }

        private bool IsGetMatch
        {
            get { return (bool)IsGetMatchBox.IsChecked; }
        }

        private bool IsGetFile
        {
            get { return (bool)IsGetFileBox.IsChecked; }
        }

        private bool IsGetFolder
        {
            get { return (bool)IsGetFolderBox.IsChecked; } 
        }

        private bool IsGetChildDir
        {
            get { return (bool)IsGetChildDirBox.IsChecked; }
        }

        private string StatusMessage
        {
            set { StatusMessageBox.Text = value + " (" + DateTime.Now + ")"; }
        }

        private DockPanel CurrView { get; set; }

        private ObservableCollection<FileInfoModel> FileList { get; set; }

        private void GetFileList(string path)
        {            
            DirectoryInfo folder = new DirectoryInfo(path);
            if (IsGetChildDir)
            {
                foreach (DirectoryInfo CurrFolder in folder.GetDirectories())
                {
                    GetFileList(CurrFolder.FullName);
                }
            }
            if (IsGetFile)
            {
                foreach (FileInfo CurrFile in folder.GetFiles())
                {
                    if (!string.IsNullOrEmpty(FilterString))
                    {
                        bool match = Regex.IsMatch(CurrFile.Name, FilterString);
                        if ((IsGetMatch && !match) || (!IsGetMatch && match)) continue;
                    }
                    FileList.Add(new FileInfoModel()
                    {
                        Path = CurrFile.DirectoryName,
                        OldFilename = CurrFile.Name
                    });
                }
            }
            if (IsGetFolder)
            {
                foreach (DirectoryInfo CurrFolder in folder.GetDirectories())
                {
                    if (!string.IsNullOrEmpty(FilterString))
                    {
                        if (Regex.IsMatch(CurrFolder.Name, FilterString)) continue;
                    }
                    FileList.Add(new FileInfoModel()
                    {
                        Path = CurrFolder.Parent.FullName,
                        OldFilename = CurrFolder.Name
                    });
                }
            }
            StatusMessage = "文件数：" + FileList.Count;
        }

        private string GetSuf(string str)
        {
            string Suf = string.Empty;
            int index = str.LastIndexOf(".");
            if (index >= 0) Suf = str.Substring(index);
            return Suf;
        }

        private void AddCharset()
        {
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                Encoding e = ei.GetEncoding();
                if (true)
                {
                    //Console.Write("{0,-18} ", ei.Name);
                    //Console.Write("{0,-9} ", e.CodePage);
                    //Console.Write("{0,-18} ", e.BodyName);
                    //Console.Write("{0,-18} ", e.HeaderName);
                    //Console.Write("{0,-18} ", e.WebName);
                    //Console.WriteLine("{0} ", e.EncodingName);
                    ComboBoxItem charset = new ComboBoxItem() { Content = e.EncodingName + "(" + e.WebName + ")", Tag = e.WebName };
                    CharsetBox.Items.Add(charset);
                }

            }
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void PreviewByNewName()
        {
            int Num = 0;
            int NumLen = NumBox.Text.Length;
            string Suf = SufBox.Text.Trim();
            if (!int.TryParse(NumBox.Text, out Num)) { NumBox.Focus(); throw new Exception("请输入正确整数"); }
            foreach (var fileinfo in FileList)
            {
                string NewSuf = string.Empty;
                if (!string.IsNullOrEmpty(Suf)) NewSuf = "." + Suf;
                else NewSuf = GetSuf(fileinfo.OldFilename);
                if (NewNameBox.Text.IndexOf("<>") >= 0)
                    fileinfo.NewFilename = string.Format("{0}{1}", NewNameBox.Text, NewSuf).Replace("<>", Num.ToString("D" + NumLen));
                else
                    fileinfo.NewFilename = string.Format("{0}{1}{2}", NewNameBox.Text, Num.ToString("D" + NumLen), NewSuf);
                ++Num;
                fileinfo.IsCreateNewDir = false;
                fileinfo.NewDir = "";
                fileinfo.Status = "";
            }
        }

        private void PreviewByReplace()
        {
            try
            {
                foreach (var fileinfo in FileList)
                {
                    fileinfo.NewFilename = Regex.Replace(fileinfo.OldFilename, OldStringBox.Text, NewStringBox.Text);
                    fileinfo.IsCreateNewDir = false;
                    fileinfo.NewDir = "";
                    fileinfo.Status = "";
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.ToString());
            }
        }

        private void PreviewByEncoding()
        {
            string charset = (CharsetBox.SelectedItem as ComboBoxItem).Tag.ToString();
            Encoding encoding = Encoding.GetEncoding(charset);
            Encoding defaultEncoding = Encoding.Default;
            foreach (var fileinfo in FileList)
            {
                fileinfo.NewFilename = encoding.GetString(defaultEncoding.GetBytes(fileinfo.OldFilename));
                fileinfo.IsCreateNewDir = false;
                fileinfo.NewDir = "";
                fileinfo.Status = "";
            }
        }

        private void PreviewByInsert()
        {
            int InsertIndex = 0;
            if (!int.TryParse(InsertIndexBox.Text, out InsertIndex)) { InsertIndexBox.Focus(); throw new Exception("请输入正确整数"); }
            foreach (var fileinfo in FileList)
            {
                int SufIndex = fileinfo.OldFilename.LastIndexOf(".");
                string Name = SufIndex >= 0 ? fileinfo.OldFilename.Substring(0, SufIndex) : fileinfo.OldFilename;
                string Suf = GetSuf(fileinfo.OldFilename);
                int TrueInsertIndex = InsertIndex;
                if (InsertIndex > Name.Length) TrueInsertIndex = Name.Length;
                else if (InsertIndex < 0) TrueInsertIndex = Name.Length + InsertIndex < 0 ? 0 : Name.Length + InsertIndex;
                fileinfo.NewFilename = Name.Insert(TrueInsertIndex, InsertStringBox.Text) + Suf;
                fileinfo.IsCreateNewDir = false;
                fileinfo.NewDir = "";
                fileinfo.Status = "";
            }
        }

        private void PreviewByResetName()
        {
            foreach (var fileinfo in FileList)
            {
                fileinfo.NewFilename = "";
                fileinfo.IsCreateNewDir = false;
                fileinfo.NewDir = "";
                fileinfo.Status = "";
            }
        }

        private void PreviewBySplitString()
        {
            string splitString = SplitStringBox.Text;
            if (string.IsNullOrEmpty(splitString)) throw new Exception("请输入分隔字符串");
            foreach (var fileinfo in FileList)
            {
                int lastIndex = fileinfo.OldFilename.LastIndexOf(splitString);
                if (lastIndex >= 0)
                {
                    fileinfo.NewFilename = fileinfo.OldFilename.Substring(lastIndex + 1);
                    fileinfo.IsCreateNewDir = true;
                    fileinfo.NewDir = fileinfo.OldFilename.Substring(0, lastIndex).Replace(splitString, @"\");
                }
                else
                {
                    fileinfo.NewFilename = fileinfo.OldFilename;
                }
                fileinfo.Status = "";
            }
        }

        private int Rename()
        {
            int SuccessNum = 0;
            foreach (var fileinfo in FileList)
            {
                try
                {
                    if (string.IsNullOrEmpty(fileinfo.NewFilename)) throw new Exception("新文件名不能为空");
                    string OldFullName = Path.Combine(fileinfo.Path, fileinfo.OldFilename);
                    if (File.Exists(OldFullName))
                    {
                        if (fileinfo.IsCreateNewDir && !string.IsNullOrEmpty(fileinfo.NewDir))
                        {
                            string NewPath = Path.Combine(fileinfo.Path, fileinfo.NewDir);
                            Directory.CreateDirectory(NewPath);
                            File.Move(OldFullName, Path.Combine(NewPath, fileinfo.NewFilename));
                        }
                        else if (fileinfo.OldFilename != fileinfo.NewFilename)
                            File.Move(OldFullName, Path.Combine(fileinfo.Path, fileinfo.NewFilename));
                    }
                    else if (Directory.Exists(OldFullName))
                    {
                        if (fileinfo.IsCreateNewDir && !string.IsNullOrEmpty(fileinfo.NewDir))
                        {
                            string NewPath = Path.Combine(fileinfo.Path, fileinfo.NewDir);
                            Directory.CreateDirectory(NewPath);
                            Directory.Move(OldFullName, Path.Combine(NewPath, fileinfo.NewFilename));
                        }
                        else if (fileinfo.OldFilename != fileinfo.NewFilename)
                            Directory.Move(OldFullName, Path.Combine(fileinfo.Path, fileinfo.NewFilename));
                    }
                    else
                    {
                        throw new Exception($"无效路径，不存在文件{OldFullName}");
                    }
                    ++SuccessNum;
                    fileinfo.IsSuccess = true;
                    fileinfo.Status = "修改成功";
                }
                catch (Exception ex)
                {
                    fileinfo.IsSuccess = false;
                    fileinfo.Status = ex.Message.Trim();
                }

            }
            return SuccessNum;
        }

        #region event
        private void GetFileList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath.Trim()))
                {
                    PathBox.Focus();
                    throw new Exception("请输入路径");
                }
                GetFileList(FilePath);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        private void GetFileListAfterClear_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.Count > 0)
                FileList.Clear();
            try
            {
                if (string.IsNullOrEmpty(FilePath.Trim()))
                {
                    PathBox.Focus();
                    throw new Exception("请输入路径");
                }
                GetFileList(FilePath);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        private void RenameSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (RenameSelectBox.SelectedIndex)
            {
                case 0:
                    if (CurrView != RenameByNewNameView)
                    {
                        CurrView.Visibility = Visibility.Collapsed;
                        CurrView = RenameByNewNameView;
                        CurrView.Visibility = Visibility.Visible;
                    }
                    break;
                case 1:
                    if (CurrView != RenameByReplaceStringView)
                    {
                        CurrView.Visibility = Visibility.Collapsed;
                        CurrView = RenameByReplaceStringView;
                        CurrView.Visibility = Visibility.Visible;
                    }
                    break;
                case 2: 
                    if (CurrView != RenameByEncodingView)
                    {
                        CurrView.Visibility = Visibility.Collapsed;
                        CurrView = RenameByEncodingView;
                        CurrView.Visibility = Visibility.Visible;
                    }
                    break;
                case 3:
                    if (CurrView != RenameByInsertStringView)
                    {
                        CurrView.Visibility = Visibility.Collapsed;
                        CurrView = RenameByInsertStringView;
                        CurrView.Visibility = Visibility.Visible;
                    }
                    break;
                case 4:
                    if (CurrView != RenameBySplitStringView)
                    {
                        CurrView.Visibility = Visibility.Collapsed;
                        CurrView = RenameBySplitStringView;
                        CurrView.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }

        private void PathBox_PreviewDrop(object sender, DragEventArgs e)
        {
            FilePath = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }

        private void PathBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int SelectedIndex = RenameSelectBox.SelectedIndex;

                switch (SelectedIndex)
                {
                    case 0:
                        PreviewByNewName();
                        break;
                    case 1:
                        PreviewByReplace();
                        break;
                    case 2:
                        PreviewByEncoding();
                        break;
                    case 3:
                        PreviewByInsert();
                        break;
                    case 4:
                        PreviewBySplitString();
                        break;
                    default:
                        PreviewByResetName();
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (FileList.Count > 0)
                FileList.Clear();
            StatusMessage = "文件数：" + FileList.Count;
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (!bgWorker.IsBusy)
            {
                IsEnabled = false;
                bgWorker.RunWorkerAsync();
            }
        }

        private void FileListView_Drop(object sender, DragEventArgs e)
        {
            if (isDragingItem)
            {
                isDragingItem = false;
                int index = GetCurrentIndex(FileListView, e.GetPosition);
                if (FileListView.SelectedItems.Count > 0 && index >= 0 && index != FileList.IndexOf(FileListView.SelectedItems[0] as FileInfoModel))
                {
                    foreach (FileInfoModel item in FileListView.SelectedItems)
                    {
                        FileList.Move(FileList.IndexOf(item), index);
                    }
                }
            }
            else
            {
                Array list = e.Data.GetData(DataFormats.FileDrop) as Array;
                foreach (var l in list)
                {
                    string s = l as string;
                    if (!string.IsNullOrEmpty(s))
                    {
                        int index = s.LastIndexOf("\\");
                        FileInfoModel model = new FileInfoModel()
                        {
                            Path = s.Substring(0, index + 1),
                            OldFilename = s.Substring(index + 1)
                        };
                        FileList.Add(model);
                    }
                }
            }
            StatusMessage = "文件数：" + FileList.Count;  
        }

        private void FileListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (FileListView.SelectedItems.Count > 0)
                {
                    for (int i = FileListView.SelectedItems.Count - 1; i >= 0; i--)
                    {
                        FileList.RemoveAt(FileListView.Items.IndexOf(FileListView.SelectedItems[i]));
                    }
                    StatusMessage = "文件数：" + FileList.Count;
                }                
            }
        }

        bool isDragingItem = false;
        private void FileListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragingItem && e.LeftButton == MouseButtonState.Pressed)
            {
                isDragingItem = true;
                DragDrop.DoDragDrop(FileListView, FileListView.SelectedItems, DragDropEffects.Move);
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int successNum = Rename();
            e.Result = successNum;
            BackgroundWorker worker = sender as BackgroundWorker;            
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsEnabled = true;
            StatusMessage = string.Format("修改完毕：{0}/{1}", e.Result, FileList.Count);
        }
        #endregion

        #region drag listview item
        private delegate Point GetPositionDelegate(IInputElement element);
        private int GetCurrentIndex(ListView lv, GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0; i < lv.Items.Count; ++i)
            {
                ListViewItem item = lv.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                if (IsMouseOverTarget(item, getPosition))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            if (target == null) return false;
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }
        #endregion
    }

    public class FileInfoModel : INotifyPropertyChanged
    {
        public FileInfoModel()
        {
        }

        public string Path { get; set; }

        public string OldFilename { get; set; }

        public string NewDir
        {
            get { return _NewDir; }
            set { if (_NewDir != value) { _NewDir = value; MyPropertyChanged("NewDir"); } }
        }

        public string NewFilename
        {
            get { return _NewFilename; }
            set { if (_NewFilename != value) { _NewFilename = value; MyPropertyChanged("NewFilename"); } }
        }

        public string Status
        {
            get { return _Status; }
            set { if (_Status != value) { _Status = value; MyPropertyChanged("Status"); } }
        }

        public bool IsCreateNewDir { get; set; }

        public bool IsSuccess
        {
            get { return _IsSuccess; }
            set { if (_IsSuccess != value) { _IsSuccess = value; MyPropertyChanged("IsSuccess"); } }
        }        

        public event PropertyChangedEventHandler PropertyChanged;

        private string _NewDir { get; set; }
        private string _NewFilename { get; set; }
        private string _Status { get; set; }
        private bool _IsSuccess { get; set; }

        private void MyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class SuccessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool success = (bool)value;
            return success ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
