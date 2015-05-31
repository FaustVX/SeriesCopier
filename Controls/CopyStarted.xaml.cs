using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SeriesCopier.Controls
{
    /// <summary>
    /// Interaction logic for CopyStarted.xaml
    /// </summary>
    public partial class CopyStarted : UserControl, INotifyPropertyChanged
    {
        private string _fileName;
        private string _speed;
        private string _eta;
        private bool _isRunning;
        private double _percentage;
        //private readonly HashSet<Line> _lines = new HashSet<Line>();
        private TimeSpan _elapsed;
        public ObservableCollection<double> Speeds { get; } = new ObservableCollection<double>();

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

        public void AddSpeed(double speed)
            => Application.Current?.Dispatcher?.Invoke(()
                =>
            {
                Speeds.Add(speed);
                //ReDraw();
            });

        //private void ReDraw()
        //{
        //    if (Speeds.Count < 2)
        //        return;

        //    foreach (var line in _lines)
        //        canvas.Children.Remove(line);
        //    _lines.Clear();

        //    var max = Speeds.Max();
        //    var width = ((int?)(((canvas.Parent as FrameworkElement))?.Parent as CopyStarted)?.RenderSize.Width) ?? 0;
        //    var height = ((int?)(((canvas.Parent as FrameworkElement))?.Parent as CopyStarted)?.RenderSize.Height) ?? 0;
        //    var last = new Point();

        //    for (int i = 0; i < Speeds.Count; i++)
        //    {
        //        var s = Speeds[i];
        //        var posX = (double)i / (Speeds.Count - 1) * width;
        //        var posY = height - (s / max * height);

        //        if (last == default(Point))
        //        {
        //            last = new Point(posX, posY);
        //            continue;
        //        }
                
        //        var line = new Line() { X1 = last.X, Y1 = last.Y, X2 = posX, Y2 = posY, Stroke = Brushes.LightSkyBlue, StrokeThickness = 1};
        //        line.SetValue(Canvas.ZIndexProperty, 0);
        //        _lines.Add(line);

        //        last = new Point(posX, posY);
        //        canvas.Children.Add(line);
        //    }
        //    UpdateLayout();
        //}

        public CopyStarted()
        {
            Speeds.Add(0);
            _isRunning = true;
            DataContext = this;
            InitializeComponent();
            //LayoutUpdated += (o, s) => ReDraw();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => Application.Current?.Dispatcher.Invoke(() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }
}
