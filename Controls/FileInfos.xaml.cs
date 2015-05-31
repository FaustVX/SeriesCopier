using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeriesCopier.Controls
{
    /// <summary>
    /// Interaction logic for FileInfos.xaml
    /// </summary>
    public partial class FileInfos : UserControl, INotifyPropertyChanged
    {
        private string _newName;
        private string _existingName;
        private string _newSize;
        private string _existingSize;

        public string NewName
        {
            get { return _newName; }
            set
            {
                if (value == NewName)
                    return;
                _newName = value;
                OnPropertyChanged();
            }
        }

        public string ExistingName
        {
            get { return _existingName; }
            set
            {
                if (value == ExistingName)
                    return;
                _existingName = value;
                OnPropertyChanged();
            }
        }

        public string NewSize
        {
            get { return _newSize; }
            set
            {
                if (value == NewSize)
                    return;
                _newSize = value;
                OnPropertyChanged();
            }
        }

        public string ExistingSize
        {
            get { return _existingSize; }
            set
            {
                if (value == ExistingSize)
                    return;
                _existingSize = value;
                OnPropertyChanged();
            }
        }
        public FileInfos()
        {
            DataContext = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
