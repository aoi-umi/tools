using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace tools {
    public class NotifyPropertyChanged : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void MyPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void MyPropertyChanged() {
            var method = new StackTrace().GetFrame(1).GetMethod();
            var name = method.Name.Replace("set_", "");
            MyPropertyChanged(name);
        }
    }

    public delegate void VoidEventHandler();
}
