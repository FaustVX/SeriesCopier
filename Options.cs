using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using SeriesCopier.Annotations;
using Size = System.Drawing.Size;
using Point = System.Drawing.Point;

namespace SeriesCopier
{
    public class Options : INotifyPropertyChanged
    {
        public static Options Current { get; } = new Options();

        private bool _skipFileOnMD5 = Properties.Settings.Default.SkipFileOnMD5;
        private string _startupFolder = Properties.Settings.Default.StartupFolder;
        private bool _isMaximized = Properties.DirectSave.Default.IsMaximized;
        private Point _position = Properties.DirectSave.Default.Position;
        private Size _size = Properties.DirectSave.Default.Size;
        private uint _copyBlockSize = Properties.Settings.Default.CopyBlockSize;

        public bool SkipFileOnMD5
        {
            get { return _skipFileOnMD5; }
            set
            {
                if (value == _skipFileOnMD5)
                    return;
                Properties.Settings.Default.SkipFileOnMD5 = _skipFileOnMD5 = value;
                OnPropertyChanged();
            }
        }

        public string StartupFolder
        {
            get { return _startupFolder; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value == _startupFolder)
                    return;
                Properties.Settings.Default.StartupFolder = _startupFolder = value;
                OnPropertyChanged();
            }
        }

        public uint CopyBlockSize
        {
            get { return _copyBlockSize; }
            set
            {
                if (value == _copyBlockSize)
                    return;
                Properties.Settings.Default.CopyBlockSize = _copyBlockSize = value;
                OnPropertyChanged();
            }
        }

        public bool IsMaximized
        {
            get { return _isMaximized; }
            set
            {
                if (value == _isMaximized)
                    return;
                Properties.DirectSave.Default.IsMaximized = _isMaximized = value;
                OnPropertyChanged();
            }
        }

        public Point Position
        {
            get { return _position; }
            set
            {
                if (value == _position)
                    return;
                Properties.DirectSave.Default.Position = _position = value;
                OnPropertyChanged();
            }
        }

        public Size Size
        {
            get { return _size; }
            set
            {
                if (value == _size)
                    return;
                Properties.DirectSave.Default.Size = _size = value;
                OnPropertyChanged();
            }
        }

        public void DirectSave()
            => Properties.DirectSave.Default.Save();

        public void SettingSave()
            => Properties.Settings.Default.Save();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}