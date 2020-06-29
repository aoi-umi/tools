using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tools {
    public class NotifyPropertyChanged: INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void MyPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public delegate void VoidEventHandler();
}
