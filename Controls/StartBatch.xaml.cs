﻿using System;
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
    /// Interaction logic for StartBatch.xaml
    /// </summary>
    public partial class StartBatch : UserControl, INotifyPropertyChanged
    {
        private int _progress;
        private int _maxFiles;
        private string _path;
        private double _fileProgress;
        private string _eta;

        public StartBatch()
        {
            DataContext = this;
            InitializeComponent();
        }

        public int Progress
        {
            get { return _progress; }
            set
            {
                if (value == Progress)
                    return;
                _progress = value;
                OnPropertyChanged();
                _fileProgress = 0;
                TotalProgress = Progress;
                OnPropertyChanged(nameof(FileProgress));
            }
        }

        public double TotalProgress { get; private set; }

        public double FileProgress
        {
            get { return _fileProgress; }
            set
            {
                if (value > 1)
                    value /= 100;
                _fileProgress = value;
                OnPropertyChanged();
                TotalProgress = Progress + FileProgress;
                OnPropertyChanged(nameof(TotalProgress));
            }
        }

        public int MaxFiles
        {
            get { return _maxFiles; }
            set
            {
                if (value == MaxFiles || value < 1)
                    return;
                _maxFiles = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                if (value == Path)
                    return;
                _path = value;
                OnPropertyChanged();
            }
        }

        public string ETA
        {
            get { return _eta; }
            set
            {
                if (value == _eta)
                    return;
                _eta = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
