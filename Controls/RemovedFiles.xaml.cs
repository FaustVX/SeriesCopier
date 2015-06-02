using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for RemovedFiles.xaml
    /// </summary>
    public partial class RemovedFiles : UserControl, INotifyPropertyChanged
    {
        public static event Action<RemovedFiles> OnClearList = delegate { };

        private readonly ObservableCollection<OutputFileInfo> _files = new ObservableCollection<OutputFileInfo>();

        public ObservableCollection<OutputFileInfo> Files
        {
            get { return _files; }
            set
            {
                Files.Clear();
                if (value == null)
                    return;

                foreach (var fileInfo in value)
                    Files.Add(fileInfo);
            }
        }

        public IList<OutputFileInfo> List { get; set; }

        public RemovedFiles()
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

        private void Button_RestoreAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var file in Files)
                List.Add(file);

            Files.Clear();
            OnClearList(this);
        }

        private void Button_RestoreOne_Click(object sender, RoutedEventArgs e)
        {
            var file = (sender as FrameworkElement)?.DataContext as OutputFileInfo;
            if (file == null)
                return;

            List.Add(file);
            Files.Remove(file);

            if (!Files.Any())
                OnClearList(this);
        }
    }
}
