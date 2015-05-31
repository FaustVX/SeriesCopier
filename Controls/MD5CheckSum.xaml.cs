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
    /// Interaction logic for MD5CheckSum.xaml
    /// </summary>
    public partial class MD5CheckSum : UserControl, INotifyPropertyChanged
    {
        private string _newName;
        private string _existingName;
        private string _newMD5;
        private string _existingMD5;

        public event Action<MD5CheckSum> OnMD5= delegate {  };

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

        public string NewMD5
        {
            get { return _newMD5; }
            set
            {
                if (value == NewMD5)
                    return;
                _newMD5 = value;
                OnPropertyChanged();
                if (!(string.IsNullOrWhiteSpace(NewMD5) && string.IsNullOrWhiteSpace(ExistingMD5)))
                    OnMD5(this);
            }
        }

        public string ExistingMD5
        {
            get { return _existingMD5; }
            set
            {
                if (value == ExistingMD5)
                    return;
                _existingMD5 = value;
                OnPropertyChanged();
                if (!(string.IsNullOrWhiteSpace(NewMD5) && string.IsNullOrWhiteSpace(ExistingMD5)))
                    OnMD5(this);
            }
        }

        public MD5CheckSum()
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
