using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using SeriesCopier.Annotations;

namespace SeriesCopier
{
    public class OutputFileInfo : INotifyPropertyChanged
    {
        private bool _isPrepared, _isCopied;
        private double _progress;
        private bool? _copy;
        private bool _alertExist;
        private bool _alertFinish;
        private string _newName;
        private bool _waitCopy;

        public bool? Copy
        {
            get { return _copy; }
            set
            {
                _copy = value;
                OnPropertyChanged();
            }
        }

        public string NewName
        {
            get { return _newName; }
            set
            {
                _newName = value;
                OnPropertyChanged();
            }
        }

        public string OriginalName { get; set; }
        public string OriginalFolder { get; set; }

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (Progress == value)
                    return;
                _progress = value;
                OnPropertyChanged();
            }
        }

        public bool IsCopied
        {
            get { return _isCopied; }
            set
            {
                if (value == IsCopied)
                    return;
                _isCopied = value;
                OnPropertyChanged();
            }
        }

        public bool IsPrepared
        {
            get { return _isPrepared; }
            set
            {
                if (value == IsPrepared)
                    return;
                _isPrepared = value;
                OnPropertyChanged();
            }
        }

        public bool WaitCopy
        {
            get { return _waitCopy; }
            set
            {
                if (value == WaitCopy)
                    return;
                _waitCopy = value;
                OnPropertyChanged();
            }
        }

        public OutputFileInfo()
        {
            PropertyChanged = delegate { };
            Copy = IsPrepared = IsCopied = true;
            NewName = OriginalName = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => Application.Current?.Dispatcher?.Invoke(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }
}