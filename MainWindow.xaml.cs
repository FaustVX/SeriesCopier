using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shell;
using Forms = System.Windows.Forms;
using Helpers;
using Helpers.XAML;
using SeriesCopier.Controls;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

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

        //[NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => Application.Current?.Dispatcher?.Invoke(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly List<FileInfo> _inputFiles = new List<FileInfo>();
        public ObservableCollection<OutputFileInfo> OutputFiles { get; } = new ObservableCollection<OutputFileInfo>();
        public ObservableCollection<FrameworkElement> LogFiles { get; } = new ObservableCollection<FrameworkElement>();

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private bool _isStarted;

        public bool IsStarted
        {
            get { return _isStarted; }
            set
            {
                _isStarted = value;
                OnPropertyChanged();
            }
        }
        
        public MainWindow()
        {
            _isStarted = true;
            DataContext = this;
            InitializeComponent();
            
            Width = Options.Current.Size.Width;
            Height = Options.Current.Size.Height;
            Left = Options.Current.Position.X;
            Top = Options.Current.Position.Y;
            WindowState = Options.Current.IsMaximized ? WindowState.Maximized : WindowState.Normal;

            SizeChanged += delegate
            {
                if (WindowState != WindowState.Maximized)
                {
                    Options.Current.Size = new Size((int) Width, (int) Height);
                    Options.Current.Position = new Point((int) Left, (int) Top);
                }
            };

            LocationChanged += delegate
            {
                if (WindowState != WindowState.Maximized)
                {
                    Options.Current.Size = new Size((int) Width, (int) Height);
                    Options.Current.Position = new Point((int) Left, (int) Top);
                }
            };

            Closing += delegate
            {
                Options.Current.IsMaximized = (WindowState == WindowState.Maximized);

                if (!Options.Current.IsMaximized)
                {
                    Options.Current.Size = new Size((int) Width, (int) Height);
                    Options.Current.Position = new Point((int) Left, (int) Top);
                }

                Options.Current.DirectSave();
            };

            var help = new Button() {Content = "Aide"}
                .OnClick(btn =>
                {
                    Log(new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        Children =
                        {
                            new UniformGrid()
                            {
                                Rows = 1,
                                Children =
                                {
                                    new TextBlock() {Text = "La fleche à droite pour afficher / cacher ce panneau ==>"},
                                    new TextBlock() {Text = "===>", HorizontalAlignment = HorizontalAlignment.Right}
                                }
                            },
                            new TextBlock()
                            {
                                Text = "Deplacer la barre à droite pour agrandir / réduire ce panneau"
                            },
                            new TextBlock()
                            {
                                Inlines =
                                {
                                    new Run("Le premier champ de texte correspond au "),
                                    new Hyperlink(new Run("Regex"))
                                    {
                                        NavigateUri = new Uri(@"http://www.regexr.com/3b3a6")
                                    }.Do(link => link.RequestNavigate += (s, e) =>
                                    {
                                        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                                        e.Handled = true;
                                    }),
                                    new Run(" à appliquer")
                                }
                            },
                            new TextBlock()
                            {
                                Text = "Le 2nd champ de texte est le motif à appliquer aux fichier," +
                                       " mettre entre accolades le n° du groupe défini avec le regex"
                            }
                        }
                    });
                });
            var options = new Button() {Content = "Options"}
                .OnClick(btn =>
                {
                    Grid gridFolder = null;
                    Log(new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        Children =
                        {
                            (gridFolder = new Grid()
                            {
                                ColumnDefinitions =
                                {
                                    new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)},
                                    new ColumnDefinition() {Width = GridLength.Auto}
                                },
                                Children =
                                {
                                    new TextBlock().Do(txt =>
                                        txt.SetBinding(TextBlock.TextProperty, new Binding()
                                        {
                                            Source = Options.Current,
                                            Path = new PropertyPath(nameof(Options.StartupFolder)),
                                            Mode = BindingMode.OneWay
                                        })).Do(txt =>
                                            txt.SetValue(Grid.ColumnProperty, 0)),
                                    new Button() {Content = "...", Width = 50}.OnClick(() =>
                                    {
                                        var folder = new Forms.FolderBrowserDialog()
                                        {
                                            SelectedPath = Options.Current.StartupFolder
                                        };
                                        if (folder.ShowDialog() == Forms.DialogResult.OK)
                                            Options.Current.StartupFolder = folder.SelectedPath;
                                    }).Do(txt =>
                                        txt.SetValue(Grid.ColumnProperty, 1))
                                }
                            }),
                            new CheckBox()
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center,
                                Content =
                                    "Automatiquement passer au fichier suivant en cas de fichier identique" +
                                    Environment.NewLine +
                                    "(Il faut lancer le test MD5)"
                            }.Do(chk => chk.SetBinding(ToggleButton.IsCheckedProperty, new Binding()
                            {
                                Source = Options.Current,
                                Path = new PropertyPath(nameof(Options.SkipFileOnMD5)),
                                Mode = BindingMode.TwoWay
                            }))
                        }
                    }, new Button() {Content = "Dossier Params"}.OnClick(() =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(
                                new FileInfo(ConfigurationManager
                                    .OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal)
                                    .FilePath).Directory?.FullName));
                        }
                        catch (Exception ex)
                        {
                            Log(ex.Message);
                        }
                    }),
                        new Button() {Content = "Save"}.OnClick(() => Options.Current.SettingSave()));
                });
            App.MainWindow.Log("Application started", help, options);

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            RemovedFiles.OnClearList += removedLog =>
                LogFiles.Remove(LogFiles.First(log => (log as Panel)?.Children.Contains(removedLog) ?? false));
        }

        private void Log(string info, params Button[] options)
            => Application.Current.Dispatcher.Invoke(() => Log(new TextBlock() {Text = info, VerticalAlignment = VerticalAlignment.Center}, options));

        private void Log(FrameworkElement element, params Button[] options)
        {
            FrameworkElement grid = null, stackPanel = null;

            LogFiles.Insert(0, grid = new Grid()
            {
                MinHeight = 21,
                ColumnDefinitions =
                {
                    new ColumnDefinition() {Width = GridLength.Auto},
                    new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)}
                },
                Children =
                {
                    element.Do(e => e.SetValue(Grid.ColumnProperty, 1))
                        .Do(e => e.VerticalAlignment = VerticalAlignment.Center),
                    new TextBlock() {Text = $"[{DateTime.Now:T}]: ", VerticalAlignment = VerticalAlignment.Stretch}
                        .Do(txt => txt.SetValue(Grid.ColumnProperty, 0))
                        .Do(g => g.MouseEnter += delegate { stackPanel.Visibility = Visibility.Visible; })
                        .Do(txt=>txt.VerticalAlignment= VerticalAlignment.Center),
                    (stackPanel = new StackPanel()
                    {
                        Visibility = Visibility.Collapsed,
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    }
                        .Do(stack => stack.Children.Add(
                            new Button()
                            {
                                Content = "X",
                                Width = 19,
                                Height = 19,
                                Margin = new Thickness(1)
                            }.Do(btn => btn.Click += delegate { LogFiles.Remove(grid); })
                                .Do(btn => btn.SetValue(Grid.ColumnProperty, 0))))
                        .Do(stack => stack.SetValue(Grid.ColumnSpanProperty, 2))
                        .Do(stack => options.ForEach(btn =>
                        {
                            btn.Margin = new Thickness(1);
                            btn.Height = 19;
                            stack.Children.Add(btn);
                        })))
                }
            }
                .Do(g => g.MouseLeave += delegate { stackPanel.Visibility = Visibility.Collapsed; }));
        }

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            var regex=new Regex(RegexTemplate.Text, RegexOptions.IgnoreCase);

            foreach (var file in OutputFiles)
            {
                var matches = regex.Match(new FileInfo(file.OriginalName).Name);
                file.NewName = matches.Success
                    ? StringFormat(OutputTemplate.Text, file.OriginalName,
                        matches.Groups.Cast<Group>()
                            .Select(g => g.Value == "" ? "" : g.Value.PadLeft(2, '0'))
                            .Cast<object>()
                            .ToArray())
                    : file.OriginalName;

                if (matches.Success)
                {
                    if (file.Copy == false)
                        file.Copy = true;
                }
                else
                    file.Copy = false;
            }
        }

        private void Button_NavigateInput_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog(){ SelectedPath = Options.Current.StartupFolder };
            if (dialog.ShowDialog() != Forms.DialogResult.OK)
                return;

            //PathToInputFolder.Text = dialog.SelectedPath;
            ListFiles(dialog.SelectedPath);
        }

        private void ListFiles(string path, bool clear = true)
        {
            var directory = new DirectoryInfo(path);

            if (clear)
            {
                _inputFiles.Clear();
                OutputFiles.Clear();
            }

            var files = directory.EnumerateFiles("*", SearchOption.AllDirectories)
                .OrderBy(file => file.Name)
                .Do(f => f.ForEach(_inputFiles.Add));

            (from file in files
                where file.Exists
                let regex = Regex.Match(file.Name, RegexTemplate.Text, RegexOptions.IgnoreCase)
                select new OutputFileInfo()
                {
                    Copy = regex.Success,
                    NewName = regex.Success
                        ? StringFormat(OutputTemplate.Text, file.Name,
                            regex.Groups.Cast<Group>().Select(g => g.Value.PadLeft(2, '0')).ToArray())
                        : file.Name,
                    OriginalName = RemovePath(file, new DirectoryInfo(path)),
                    OriginalFolder = path
                }).ForEach(OutputFiles.Add);
        }

        private static string StringFormat(string template, string defaultName, params object[] args)
        {
            try
            {
                return string.Format(template, args);
            }
            catch (Exception ex)
            {
                var grid = new Grid()
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition() {Width = new GridLength(25)},
                        new ColumnDefinition() {Width = GridLength.Auto}
                    },
                    RowDefinitions =
                    {
                        new RowDefinition(),
                        new RowDefinition()
                    },
                    Children =
                    {
                        new TextBlock() {Text = defaultName}
                            .Do(txt => txt.SetValue(Grid.ColumnSpanProperty, 2)),
                        new TextBlock() {Text = ex.Message}
                            .Do(txt => txt.SetValue(Grid.ColumnProperty, 1))
                            .Do(txt => txt.SetValue(Grid.RowProperty, 1))
                    }
                };

                App.MainWindow.Log(grid);
                return defaultName;
            }
        }

        private static string RemovePath(FileInfo file, DirectoryInfo baseDir)
        {
            if (file.Directory?.FullName == baseDir.FullName)
                return file.Name;

            var result = file.Name;
            
            for (var dir = file.Directory; dir?.FullName != baseDir.FullName; dir = dir?.Parent)
                result = dir?.Name + "\\" + result;

            return result;
        }

        private static string RemovePath(DirectoryInfo dir, DirectoryInfo baseDir)
        {
            var result = "";

            for (; dir?.FullName != baseDir.FullName + result; result = dir?.Name + "\\" + result)
                dir = dir?.Parent;

            return result;
        }
        
        private void Button_Copy_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog() { ShowNewFolderButton = true, SelectedPath = Options.Current.StartupFolder};
            if (dialog.ShowDialog() != Forms.DialogResult.OK)
                return;

            IsStarted = false;

            var copyStarted = _stopwatch.Elapsed;

            var outputFiles = OutputFiles.Where(file => !file.Copy.HasValue || file.Copy.Value).Memorize();

            StartBatch batch = null;
            var cancelSource = new CancellationTokenSource();
            var task = new Task(() =>
                Application.Current.Dispatcher.Invoke(() =>
                    Log(batch = new StartBatch()
                    {
                        MaxFiles = outputFiles.Count(),
                        Path = new DirectoryInfo(dialog.SelectedPath).FullName
                    }, new Button() {Content = "Tout Arreter"}.Do(btn=> btn.Click+= delegate
                    {
                        cancelSource.Cancel();
                        btn.IsEnabled = false;
                    }))), cancelSource.Token);

            long totalSize = 0L, totalCopiedSize = 0L;

            foreach (var file in outputFiles)
            {
                file.Progress = 0;
                file.IsCopied = true;

                var output = Path.Combine(dialog.SelectedPath, file.NewName);

                var input = new FileInfo(output);
                input.Directory.Create();

                var fileInfo = new FileInfo(Path.Combine(file.OriginalFolder, file.OriginalName));

                var exist = fileInfo.Exists;
                file.IsPrepared = false;
                file.WaitCopy = true;
                Action copyAction = null;

                totalSize += fileInfo.Length;

                copyAction = delegate
                {
                    task.If(t => t.Status == TaskStatus.Created)?.Start();

                    task = task.ContinueWith(t =>
                    {
                        if (cancelSource.IsCancellationRequested)
                            return;

                        TimeSpan start;
                        var doCopy = true;

                        var doContinue = true;
                        ExistantFile existant = null;
                        if (input.Exists)
                        {
                            doContinue = false;
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                                doCopy = false;
                                existant = new ExistantFile()
                                {
                                    FileName = file.NewName
                                };
                                existant.OnAction += (o, action) =>
                                {
                                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                                    if (action.Behavior == ExistantFileBehavior.Ignore)
                                    {
                                        doContinue = true;
                                        batch.MaxFiles--;
                                        file.WaitCopy = false;
                                        totalSize -= fileInfo.Length;
                                    }
                                    else if (action.Behavior == ExistantFileBehavior.Rename)
                                    {
                                        doContinue = true;
                                        file.NewName = action.NewName;

                                        output = Path.Combine(dialog.SelectedPath, file.NewName);
                                        input = new FileInfo(output);
                                        copyAction?.Invoke();
                                    }
                                    else if (action.Behavior == ExistantFileBehavior.Overwrite)
                                    {
                                        input.Delete();
                                        doCopy = true;
                                        doContinue = true;
                                    }
                                    TaskbarItemInfo.ProgressValue = batch.TotalProgress/batch.MaxFiles;
                                };

                                Log(existant, new Button() {Content = "Infos"}.Do(btn => btn.Click += delegate
                                {
                                    fileInfo.Refresh();
                                    input.Refresh();

                                    Log(new FileInfos()
                                    {
                                        ExistingName = input.Name,
                                        NewName = fileInfo.Name,
                                        ExistingSize = Size(input.Length),
                                        NewSize = Size(fileInfo.Length)
                                    }, new Button() {Content = "MD5"}.Do(btnmd5 => btnmd5.Click += delegate
                                    {
                                        Func<FileInfo, string> computeMD5 = f =>
                                        {
                                            using (var md5 = MD5.Create())
                                            using (var stream = f.OpenRead())
                                                return BitConverter.ToString(md5.ComputeHash(stream))
                                                    .Replace("-", "")
                                                    .ToLower();
                                        };

                                        Log(new MD5CheckSum()
                                        {
                                            ExistingName = input.Name,
                                            NewName = fileInfo.Name
                                        }.Do(md5 => md5.OnMD5 += delegate
                                        {
                                            if (Options.Current.SkipFileOnMD5)
                                                Application.Current.Dispatcher.Invoke(() =>
                                                    existant.SetBehavior(ExistantFileBehavior.Ignore));
                                        }).DoAsync(md5Log => md5Log.ExistingMD5 = computeMD5(input))
                                            .DoAsync(md5Log => md5Log.NewMD5 = computeMD5(fileInfo)));
                                    }));
                                }));
                            });
                        }

                        while (!doContinue) ;
                        if (!doCopy)
                            return;
                        file.WaitCopy = false;

                        start = _stopwatch.Elapsed;

                        var copier = new FileCopier(fileInfo, input);
                        var length = 0L;
                        CopyStarted log = null;
                        bool cancel = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            log = new CopyStarted()
                            {
                                FileName = new FileInfo(file.NewName).Name,
                                Speed = "0 Bytes/s",
                                ETA = $"{DateTime.Now:T}"
                            };
                            Log(log, new Button() {Content = "Stop"}.Do(btn => btn.Click += delegate
                            {
                                cancel = true;
                                batch.FileProgress = 0;
                                batch.MaxFiles--;
                                btn.IsEnabled = false;
                                totalSize -= fileInfo.Length;
                            }));
                        });

                        {
                            var last = Tuple.Create(0L, TimeSpan.Zero);
                            copier.OnProgressChanged +=
                                (double percentage, long sizeCopied, ref bool stop) =>
                                {
                                    var current = Tuple.Create(sizeCopied, _stopwatch.Elapsed - start);
                                    if ((current.Item2 - last.Item2).TotalSeconds >= 0.5)
                                    {
                                        log.Speed = Speed(current.Item1, current.Item2);
                                        var speed = SpeedBytes(current.Item1, current.Item2);
                                        log.ETA = ETA(copier.SourceFilePath.Length, sizeCopied,
                                            speed);
                                        last = current;

                                        batch.ETA = ETA(totalSize, totalCopiedSize + sizeCopied, speed);
                                    }

                                    log.Elapsed = _stopwatch.Elapsed - start;

                                    batch.FileProgress = percentage/100;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                                        TaskbarItemInfo.ProgressValue = batch.TotalProgress/batch.MaxFiles;
                                    });
                                    stop = cancel || cancelSource.IsCancellationRequested;
                                    file.Progress = percentage;
                                    if (!cancel)
                                        log.Percentage = percentage;
                                };
                        }
                        if (doCopy)
                            copier.Copy(out length);
                        if (!cancel)
                            batch.Progress++;
                        totalCopiedSize += fileInfo.Length;
                        //var elapsed = _stopwatch.Elapsed - start;
                        //log.AddSpeed(SpeedBytes(fileInfo.Length, elapsed));
                        log.IsRunning = false;

                        if (!file.Copy.HasValue)
                            fileInfo.Delete();

                        file.IsCopied = false;
                    });
                };

                if (exist)
                    copyAction();
                else
                {
                    Log($"Le fichier {fileInfo.Name} a été supprimé, la copie est annulé");

                    file.Progress = 100;
                    file.IsCopied = false;
                }
            }

            Task.WhenAll(new []{task}).ContinueWith(t =>
            {

                while (batch.Progress != batch.MaxFiles && !cancelSource.IsCancellationRequested) ;

                IsStarted = true;
                var elapsed = _stopwatch.Elapsed - copyStarted;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
                    Log(new CopyFinished()
                    {
                        Speed = Speed(totalSize, elapsed),
                        Duration = elapsed.ToString()
                    }.Do(cf => cf.OnReset += delegate
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TaskbarItemInfo.ProgressValue = 0;
                            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                        });
                        foreach (var file in outputFiles)
                        {
                            file.WaitCopy = false;
                            file.IsPrepared = true;
                            file.IsCopied = true;
                            file.Progress = 0;
                        }
                    }));
                });
            });
        }

        private static string ETA(long totalSize, long currentCopied, double speed)
        {
            var remain = totalSize - currentCopied;
            var timeRemain = remain/speed;
            var eta = TimeSpan.FromSeconds(timeRemain);

            return $"{DateTime.Now + eta:T}";
        }

        private static double SpeedBytes(long size, TimeSpan duration)
            => size/duration.TotalSeconds;

        private static string Speed(long size, TimeSpan duration)
        {
            if (duration.TotalSeconds < 0.01)
                return "";

            var newSize = size / duration.TotalSeconds;

            var speedUnits = new[]
            {
                "Bytes/s",
                "KB/s",
                "MB/s",
                "GB/s",
                "TB/s"
            };

            var unit = 0;
            for (; newSize > 1000; newSize /= 1024)
                unit++;

            return $"{newSize:F3} {speedUnits[unit]}";
        }

        private static string Size(long size)
        {
            var speedUnits = new[]
            {
                "Bytes",
                "KB",
                "MB",
                "GB",
                "TB"
            };

            var unit = 0;
            for (; size > 1000; size /= 1024)
                unit++;

            return $"{size:F3} {speedUnits[unit]}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in OutputFiles.Where(file => file.Copy == InPut.IsChecked))
                file.Copy = OutPut.IsChecked;
        }

        private void Button_Addnput_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog()
            { SelectedPath = Options.Current.StartupFolder };
            if (dialog.ShowDialog() != Forms.DialogResult.OK)
                return;

            //PathToInputFolder.Text = dialog.SelectedPath;
            ListFiles(dialog.SelectedPath, false);
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            _inputFiles.Clear();
            OutputFiles.Clear();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var file = (sender as FrameworkElement)?.DataContext as OutputFileInfo;
            if (file == null)
                return;

            OutputFiles.Remove(file);
            Application.Current.Dispatcher.Invoke(
                () =>
                    Log(new RemovedFiles()
                    {
                        Files = new ObservableCollection<OutputFileInfo>(new[] {file}),
                        List = OutputFiles
                    }));
        }

        private void Button_DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            //OutputFiles.RemoveWhen(file => file.Copy == InPut.IsChecked);

            var deleted = new List<OutputFileInfo>(OutputFiles.Where(file => file.Copy == InPut.IsChecked));

            foreach (var file in deleted)
                OutputFiles.Remove(file);
            Application.Current.Dispatcher.Invoke(
                () =>
                    Log(new RemovedFiles()
                    {
                        Files = new ObservableCollection<OutputFileInfo>(deleted),
                        List = OutputFiles
                    }));
        }

        private void MenuItem_OpenOriginalFolder_Click(object sender, RoutedEventArgs e)
        {
            var file = (sender as FrameworkElement)?.DataContext as OutputFileInfo;
            if (file == null)
                return;

            var dir = new DirectoryInfo(Path.Combine(file.OriginalFolder, file.OriginalName));
            Process.Start(new ProcessStartInfo(dir.Parent?.FullName));
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            if (gridSplitter == null)
                return;
            gridSplitter.Visibility = Visibility.Collapsed;
            (sender as DependencyObject)?.SetValue(Panel.ZIndexProperty, -1);
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (gridSplitter == null)
                return;
            gridSplitter.Visibility = Visibility.Visible;
            (sender as DependencyObject)?.SetValue(Panel.ZIndexProperty, 1);
        }
    }
}
