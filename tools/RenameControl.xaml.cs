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
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tools {
    /// <summary>
    /// RenameControl.xaml 的交互逻辑
    /// </summary>
    public partial class RenameControl : UserControl {
        public RenameControl() {
            InitializeComponent();
            bgWorker = new BackgroundWorker();
            FileList = new ObservableCollection<FileInfoModel>();

            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            FileListView.ItemsSource = FileList;
            CurrView = RenameByNewNameView;
            MainBox.DataContext = state;
            AddCharset();
            ViewList = new List<ViewModel>() {
            new ViewModel(){
                View = RenameByNewNameView,
                Handler = PreviewByNewName,
            },
            new ViewModel(){
                View = RenameByReplaceStringView,
                Handler = PreviewByReplace,
            },
            new ViewModel(){
                View = RenameByEncodingView,
                Handler = PreviewByEncoding,
            },
            new ViewModel(){
                View = RenameByInsertStringView,
                Handler = PreviewByInsert,
            },
            new ViewModel(){
                View = RenameBySplitStringView,
                Handler = PreviewBySplitString,
            },
        };
        }
        private List<ViewModel> ViewList;
        private StateModel state = new StateModel()
        {
            //IsEnabled = true
            StatusMessage = "文件数：0"
        };

        private BackgroundWorker bgWorker { get; set; }
        private void SetStatusMessage(string value) {
            state.StatusMessage = $"{value} ({DateTime.Now})";
        }

        private UIElement CurrView { get; set; }

        private ObservableCollection<FileInfoModel> FileList { get; set; }

        private bool NotMatch(string name) {
            if (string.IsNullOrEmpty(state.FilterString))
                return false;
            bool match = Regex.IsMatch(name, state.FilterString);
            return (state.IsGetMatch && !match) || (!state.IsGetMatch && match);
        }
        private void GetFileList(string path) {
            DirectoryInfo folder = new DirectoryInfo(path);
            if (state.IsGetChildDir) {
                foreach (DirectoryInfo CurrFolder in folder.GetDirectories()) {
                    if (NotMatch(CurrFolder.Name)) continue;
                    GetFileList(CurrFolder.FullName);
                }
            }
            if (state.IsGetFile) {
                foreach (FileInfo CurrFile in folder.GetFiles()) {
                    if (NotMatch(CurrFile.Name)) continue;
                    AddFile(new FileInfoModel()
                    {
                        Path = CurrFile.DirectoryName,
                        OldFilename = CurrFile.Name
                    });
                }
            }
            if (state.IsGetFolder) {
                foreach (DirectoryInfo CurrFolder in folder.GetDirectories()) {
                    if (NotMatch(CurrFolder.Name)) continue;
                    AddFile(new FileInfoModel()
                    {
                        Path = CurrFolder.Parent.FullName,
                        OldFilename = CurrFolder.Name
                    });
                }
            }

            SetStatusMessage("文件数：" + FileList.Count);
        }

        private void AddFile(FileInfoModel model) {
            Application.Current.Dispatcher.Invoke(delegate {
                FileList.Add(model);
            });
        }

        private string GetSuf(string str) {
            string Suf = string.Empty;
            int index = str.LastIndexOf(".");
            if (index >= 0) Suf = str.Substring(index);
            return Suf;
        }

        private void AddCharset() {
            foreach (EncodingInfo ei in Encoding.GetEncodings()) {
                Encoding e = ei.GetEncoding();
                if (true) {
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

        private void ShowMessage(string message) {
            MessageBox.Show(message);
        }

        private void ResetFileInfo(FileInfoModel fileinfo) {
            fileinfo.NewFilename = "";
            fileinfo.IsCreateNewDir = false;
            fileinfo.NewDir = "";
            fileinfo.Status = "";
        }
        private void PreviewByNewName() {
            int Num;
            int NumLen = NumBox.Text.Length;
            string Suf = SufBox.Text.Trim();
            if (!int.TryParse(NumBox.Text, out Num)) { NumBox.Focus(); throw new Exception("请输入正确整数"); }
            foreach (var fileinfo in FileList) {
                ResetFileInfo(fileinfo);
                string NewSuf = string.Empty;
                if (!string.IsNullOrEmpty(Suf)) NewSuf = "." + Suf;
                else NewSuf = GetSuf(fileinfo.OldFilename);
                var flag = "<>";
                if (NewNameBox.Text.IndexOf(flag) >= 0)
                    fileinfo.NewFilename = string.Format("{0}{1}", NewNameBox.Text, NewSuf).Replace(flag, Num.ToString("D" + NumLen));
                else
                    fileinfo.NewFilename = string.Format("{0}{1}{2}", NewNameBox.Text, Num.ToString("D" + NumLen), NewSuf);
                ++Num;
            }
        }

        private void PreviewByReplace() {
            try {
                foreach (var fileinfo in FileList) {
                    ResetFileInfo(fileinfo);
                    fileinfo.NewFilename = Regex.Replace(fileinfo.OldFilename, OldStringBox.Text, NewStringBox.Text);
                }
            } catch (Exception ex) {
                ShowMessage(ex.ToString());
            }
        }

        private void PreviewByEncoding() {
            string charset = (CharsetBox.SelectedItem as ComboBoxItem).Tag.ToString();
            Encoding encoding = Encoding.GetEncoding(charset);
            Encoding defaultEncoding = Encoding.Default;
            foreach (var fileinfo in FileList) {
                ResetFileInfo(fileinfo);
                fileinfo.NewFilename = encoding.GetString(defaultEncoding.GetBytes(fileinfo.OldFilename));
            }
        }

        private void PreviewByInsert() {
            int InsertIndex;
            if (!int.TryParse(InsertIndexBox.Text, out InsertIndex)) { InsertIndexBox.Focus(); throw new Exception("请输入正确整数"); }
            foreach (var fileinfo in FileList) {
                ResetFileInfo(fileinfo);
                int SufIndex = fileinfo.OldFilename.LastIndexOf(".");
                string Name = SufIndex >= 0 ? fileinfo.OldFilename.Substring(0, SufIndex) : fileinfo.OldFilename;
                string Suf = GetSuf(fileinfo.OldFilename);
                int TrueInsertIndex = InsertIndex;
                if (InsertIndex > Name.Length) TrueInsertIndex = Name.Length;
                else if (InsertIndex < 0) TrueInsertIndex = Name.Length + InsertIndex < 0 ? 0 : Name.Length + InsertIndex;
                fileinfo.NewFilename = Name.Insert(TrueInsertIndex, InsertStringBox.Text) + Suf;
            }
        }

        private void PreviewBySplitString() {
            string splitString = SplitStringBox.Text;
            if (string.IsNullOrEmpty(splitString)) throw new Exception("请输入分隔字符串");
            foreach (var fileinfo in FileList) {
                ResetFileInfo(fileinfo);
                int lastIndex = fileinfo.OldFilename.LastIndexOf(splitString);
                if (lastIndex >= 0) {
                    fileinfo.NewFilename = fileinfo.OldFilename.Substring(lastIndex + 1);
                    fileinfo.IsCreateNewDir = true;
                    fileinfo.NewDir = fileinfo.OldFilename.Substring(0, lastIndex).Replace(splitString, @"\");
                } else {
                    fileinfo.NewFilename = fileinfo.OldFilename;
                }
            }
        }

        private int Rename() {
            int SuccessNum = 0;
            foreach (var fileinfo in FileList) {
                try {
                    if (string.IsNullOrEmpty(fileinfo.NewFilename)) throw new Exception("新文件名不能为空");
                    string OldFullName = Path.Combine(fileinfo.Path, fileinfo.OldFilename);
                    if (File.Exists(OldFullName)) {
                        if (fileinfo.IsCreateNewDir && !string.IsNullOrEmpty(fileinfo.NewDir)) {
                            string NewPath = Path.Combine(fileinfo.Path, fileinfo.NewDir);
                            Directory.CreateDirectory(NewPath);
                            File.Move(OldFullName, Path.Combine(NewPath, fileinfo.NewFilename));
                        } else if (fileinfo.OldFilename != fileinfo.NewFilename)
                            File.Move(OldFullName, Path.Combine(fileinfo.Path, fileinfo.NewFilename));
                    } else if (Directory.Exists(OldFullName)) {
                        if (fileinfo.IsCreateNewDir && !string.IsNullOrEmpty(fileinfo.NewDir)) {
                            string NewPath = Path.Combine(fileinfo.Path, fileinfo.NewDir);
                            Directory.CreateDirectory(NewPath);
                            Directory.Move(OldFullName, Path.Combine(NewPath, fileinfo.NewFilename));
                        } else if (fileinfo.OldFilename != fileinfo.NewFilename)
                            Directory.Move(OldFullName, Path.Combine(fileinfo.Path, fileinfo.NewFilename));
                    } else {
                        throw new Exception($"无效路径，不存在文件{OldFullName}");
                    }
                    ++SuccessNum;
                    fileinfo.IsSuccess = true;
                    fileinfo.Status = "修改成功";
                } catch (Exception ex) {
                    fileinfo.IsSuccess = false;
                    fileinfo.Status = ex.Message.Trim();
                }

            }
            return SuccessNum;
        }

        #region event
        private void GetFileList_Click(object sender, RoutedEventArgs e) {
            GetFile();
        }

        private void GetFileListAfterClear_Click(object sender, RoutedEventArgs e) {
            if (FileList.Count > 0)
                FileList.Clear();
            GetFile();
        }

        private async void GetFile() {
            try {
                if (string.IsNullOrEmpty(state.FilePath.Trim())) {
                    PathBox.Focus();
                    throw new Exception("请输入路径");
                }
                await Task.Run(() =>
                {
                    state.IsEnabled = false;
                    GetFileList(state.FilePath);
                });
            } catch (Exception ex) {
                ShowMessage(ex.Message);
            } finally {
                state.IsEnabled = true;
            }
        }

        private void RenameSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ViewList == null) return;
            var selected = ViewList[RenameSelectBox.SelectedIndex].View;
            if (selected != CurrView) {
                CurrView.Visibility = Visibility.Collapsed;
                CurrView = selected;
                CurrView.Visibility = Visibility.Visible;
            }
        }

        private void PathBox_PreviewDrop(object sender, DragEventArgs e) {
            state.FilePath = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }

        private void PathBox_PreviewDragOver(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }
        private void Preview_Click(object sender, RoutedEventArgs e) {
            if (ViewList == null) return;
            try {
                var Handler = ViewList[RenameSelectBox.SelectedIndex].Handler;
                Handler();
            } catch (Exception ex) {
                ShowMessage(ex.Message);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e) {
            if (FileList.Count > 0)
                FileList.Clear();
            SetStatusMessage("文件数：" + FileList.Count);
        }

        private void Rename_Click(object sender, RoutedEventArgs e) {
            if (!bgWorker.IsBusy) {
                IsEnabled = false;
                bgWorker.RunWorkerAsync();
            }
        }

        private void FileListView_Drop(object sender, DragEventArgs e) {
            if (isDragingItem) {
                isDragingItem = false;
                int index = GetCurrentIndex(FileListView, e.GetPosition);
                if (FileListView.SelectedItems.Count > 0 && index >= 0 && index != FileList.IndexOf(FileListView.SelectedItems[0] as FileInfoModel)) {
                    foreach (FileInfoModel item in FileListView.SelectedItems) {
                        FileList.Move(FileList.IndexOf(item), index);
                    }
                }
            } else {
                Array list = e.Data.GetData(DataFormats.FileDrop) as Array;
                foreach (var l in list) {
                    string s = l as string;
                    if (!string.IsNullOrEmpty(s)) {
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
            SetStatusMessage("文件数：" + FileList.Count);
        }

        private void FileListView_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                if (FileListView.SelectedItems.Count == 0) {
                    return;
                }

                for (int i = FileListView.SelectedItems.Count - 1; i >= 0; i--) {
                    FileList.RemoveAt(FileListView.Items.IndexOf(FileListView.SelectedItems[i]));
                }
                SetStatusMessage("文件数：" + FileList.Count);
            }
        }

        bool isDragingItem = false;
        bool isOnItem = false;
        private void FileListView_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            int index = GetCurrentIndex(FileListView, e.GetPosition);
            isOnItem = index >= 0;
        }
        private void FileListView_MouseMove(object sender, MouseEventArgs e) {
            if (isOnItem && !isDragingItem && e.LeftButton == MouseButtonState.Pressed) {
                isDragingItem = true;
                DragDrop.DoDragDrop(FileListView, FileListView.SelectedItems, DragDropEffects.Move);
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e) {
            int successNum = Rename();
            e.Result = successNum;
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            IsEnabled = true;
            SetStatusMessage($"修改完毕：{e.Result}/{FileList.Count}");
        }
        #endregion

        #region drag listview item
        private delegate Point GetPositionDelegate(IInputElement element);
        private int GetCurrentIndex(ListView lv, GetPositionDelegate getPosition) {
            int index = -1;
            for (int i = 0; i < lv.Items.Count; ++i) {
                ListViewItem item = lv.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                if (IsMouseOverTarget(item, getPosition)) {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition) {
            if (target == null) return false;
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }
        #endregion
    }

    public class FileInfoModel : NotifyPropertyChanged {
        public FileInfoModel() {
        }

        public string Path { get; set; }

        public string OldFilename { get; set; }

        public string NewDir {
            get { return _NewDir; }
            set { if (_NewDir != value) { _NewDir = value; MyPropertyChanged("NewDir"); } }
        }

        public string NewFilename {
            get { return _NewFilename; }
            set { if (_NewFilename != value) { _NewFilename = value; MyPropertyChanged("NewFilename"); } }
        }

        public string Status {
            get { return _Status; }
            set { if (_Status != value) { _Status = value; MyPropertyChanged("Status"); } }
        }

        public bool IsCreateNewDir { get; set; }

        public bool IsSuccess {
            get { return _IsSuccess; }
            set { if (_IsSuccess != value) { _IsSuccess = value; MyPropertyChanged("IsSuccess"); } }
        }

        private string _NewDir { get; set; }
        private string _NewFilename { get; set; }
        private string _Status { get; set; }
        private bool _IsSuccess { get; set; }
    }

    public class SuccessConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool success = (bool)value;
            return success ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return null;
        }
    }

    public class ViewModel {
        public UIElement View;
        public VoidEventHandler Handler;
    }
    public class StateModel : NotifyPropertyChanged {
        public StateModel() {
        }
        public bool IsEnabled {
            get { return _IsEnabled; }
            set { _IsEnabled = value; MyPropertyChanged(); }
        }
        private bool _IsEnabled = true;

        public string FilePath {
            get { return _FilePath; }
            set {
                _FilePath = value; MyPropertyChanged();
            }
        }
        private string _FilePath = "";

        public string FilterString {
            get { return _FilterString; }
            set {
                _FilterString = value; MyPropertyChanged();
            }
        }
        private string _FilterString = "";

        public bool IsGetMatch {
            get { return _IsGetMatch; }
            set {
                _IsGetMatch = value; MyPropertyChanged();
            }
        }
        private bool _IsGetMatch;

        public bool IsGetFile {
            get { return _IsGetFile; }
            set {
                _IsGetFile = value; MyPropertyChanged();
            }
        }
        private bool _IsGetFile = true;

        public bool IsGetFolder {
            get { return _IsGetFolder; }
            set {
                _IsGetFolder = value; MyPropertyChanged();
            }
        }
        private bool _IsGetFolder;

        public bool IsGetChildDir {
            get { return _IsGetChildDir; }
            set {
                _IsGetChildDir = value; MyPropertyChanged();
            }
        }
        private bool _IsGetChildDir;

        public string StatusMessage {
            get { return _StatusMessage; }
            set {
                _StatusMessage = value; MyPropertyChanged();
            }
        }
        private string _StatusMessage;
    }
}
