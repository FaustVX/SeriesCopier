using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Helpers.XAML;

namespace SeriesCopier.Controls
{
    /// <summary>
    /// Interaction logic for CopyStarted.xaml
    /// </summary>
    public partial class CopyStarted : UserControl, INotifyPropertyChanged
    {
        public event Action<CopyStarted> OnStop;

        private string _fileName;
        private string _speed;
        private string _eta;
        private bool _isRunning;
        private double _percentage;
        private TimeSpan _elapsed;
        public Button[] Options { get; } = new Button[1];

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (value == IsRunning)
                    return;
                _isRunning = value;
                OnPropertyChanged();
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (value == FileName)
                    return;
                _fileName = value;
                OnPropertyChanged();
            }
        }

        public string Speed
        {
            get { return _speed; }
            set
            {
                if (value == Speed || value == "")
                    return;
                _speed = value;
                OnPropertyChanged();
            }
        }

        public string ETA
        {
            get { return _eta; }
            set
            {
                if (value == ETA || value == "")
                    return;
                _eta = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan Elapsed
        {
            get { return _elapsed; }
            set
            {
                if (value == Elapsed)
                    return;
                _elapsed = value;
                OnPropertyChanged();
            }
        }

        public double Percentage
        {
            get { return _percentage; }
            set
            {
                if (value == Percentage)
                    return;
                _percentage = value;
                OnPropertyChanged();
            }
        }

        public CopyStarted()
        {
            MouseEnter += delegate
            {
                itemsControl.Visibility = Visibility.Visible;
            };
            MouseLeave += delegate
            {
                itemsControl.Visibility = Visibility.Collapsed;
            };

            Button btn = null;
            btn = new Button {Content = "Stop"}.OnClick(delegate
            {
                OnStop?.Invoke(this);
                btn.IsEnabled = false;
            });
            Options[0] = btn;

            _isRunning = true;
            DataContext = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => Application.Current?.Dispatcher.Invoke(() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }
}
