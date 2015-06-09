using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using SeriesCopier.Annotations;

namespace SeriesCopier.Controls
{
    /// <summary>
    /// Logique d'interaction pour FilenameParam.xaml
    /// </summary>
    public partial class FilenameParam : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<KeyValuePair<string, string>> Parameters { get; } =
            new ObservableCollection<KeyValuePair<string, string>>();

        public FilenameParam()
        {
            DataContext = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
