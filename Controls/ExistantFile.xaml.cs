using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using SeriesCopier.Annotations;

namespace SeriesCopier.Controls
{
    public struct ExistantFileAction
    {
        public ExistantFileBehavior Behavior { get; }
        public string NewName { get; }

        public ExistantFileAction(ExistantFileBehavior behavior, string newName)
        {
            Behavior = behavior;
            NewName = newName;
        }
    }

    public enum ExistantFileBehavior
    {
        Ignore,
        Overwrite,
        Rename
    }

    /// <summary>
    /// Interaction logic for ExistantFile.xaml
    /// </summary>
    public partial class ExistantFile : UserControl, INotifyPropertyChanged
    {
        private string _fileName;
        private string _originalFileName;
        public event EventHandler<ExistantFileAction> OnAction = delegate { };

        public ExistantFileBehavior Behavior { get; private set; }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (value == FileName || value == "")
                    return;
                _fileName = value;
                OnPropertyChanged();
                OriginalFileName = FileName;
            }
        }

        public string OriginalFileName
        {
            get { return _originalFileName; }
            set
            {
                if (!string.IsNullOrEmpty(OriginalFileName))
                    return;
                _originalFileName = value;
                OnPropertyChanged();
            }
        }

        public ExistantFile()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void SetBehavior(ExistantFileBehavior behavior)
        {
            if (!this.IsEnabled)
                return;

            if (behavior != ExistantFileBehavior.Ignore)
                Btn_Ignore.Visibility = Visibility.Collapsed;
            if (behavior != ExistantFileBehavior.Rename)
                Btn_Rename.Visibility = Visibility.Collapsed;
            if (behavior != ExistantFileBehavior.Overwrite)
                Btn_Overwrite.Visibility = Visibility.Collapsed;
            this.IsEnabled = false;
            OnAction(this, new ExistantFileAction(Behavior = behavior, FileName));
        }

        private void Button_Ignore_Click(object sender, RoutedEventArgs e)
            => SetBehavior(ExistantFileBehavior.Ignore);

        private void Button_Owerwrite_Click(object sender, RoutedEventArgs e)
            => SetBehavior(ExistantFileBehavior.Overwrite);

        private void Button_Rename_Click(object sender, RoutedEventArgs e)
            => SetBehavior(ExistantFileBehavior.Rename);

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
