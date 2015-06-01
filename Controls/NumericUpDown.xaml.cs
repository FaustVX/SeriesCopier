using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        private int _numValue = 0;
        public event Action<int> OnChanged;

        public NumericUpDown()
        {
            DataContext = this;
            InitializeComponent();
            txtNum.Text = _numValue.ToString();
        }

        public int NumValue
        {
            get { return _numValue; }
            set
            {
                if (value < 0)
                    return;
                _numValue = value;
                txtNum.Text = value.ToString();
                OnChanged?.Invoke(NumValue);
            }
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            NumValue += 1024;
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            NumValue -= 1024;
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNum == null)
            {
                return;
            }

            if (!int.TryParse(txtNum.Text, out _numValue))
                txtNum.Text = _numValue.ToString();
        }
    }
}
