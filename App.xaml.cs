using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SeriesCopier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static new App Current
            => Application.Current as App;

        public static new MainWindow MainWindow
            => Application.Current.MainWindow as MainWindow;
    }
}
