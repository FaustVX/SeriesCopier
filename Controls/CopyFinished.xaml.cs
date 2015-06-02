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
using SeriesCopier.Annotations;

namespace SeriesCopier.Controls
{
    /// <summary>
    /// Interaction logic for CopyFinished.xaml
    /// </summary>
    public partial class CopyFinished : UserControl, INotifyPropertyChanged
    {
        private string _speed;
        private string _duration;
        public event Action<CopyFinished> OnReset = delegate { };

        public string Speed
        {
            get { return _speed; }
            set
            {
                if (value == Speed || string.IsNullOrWhiteSpace(value))
                    return;
                _speed = value;
                OnPropertyChanged();
            }
        }

        public string Duration
        {
            get { return _duration; }
            set
            {
                if (value == Duration || string.IsNullOrWhiteSpace(value))
                    return;
                _duration = value;
                OnPropertyChanged();
            }
        }

        public CopyFinished()
        {
            DataContext = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((UIElement) sender).IsEnabled = false;
            OnReset(this);
        }
    }
}
