using System.Windows;
using System.Windows.Controls;

namespace SeriesCopier.Controls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public NumericUpDown()
        {
            DataContext = this;
            InitializeComponent();
            txtNum.Text = NumValue.ToString();
        }
        
        public int NumValue
        {
            get { return (int)GetValue(NumValueProperty); }
            set { SetValue(NumValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NumValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumValueProperty =
            DependencyProperty.Register(nameof(NumValue), typeof(int), typeof(NumericUpDown), new PropertyMetadata(0, NumValueChangedCallback));

        private static void NumValueChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var nud = dependencyObject as NumericUpDown;

            if (nud != null)
                nud.txtNum.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
            => NumValue += 1024;

        private void cmdDown_Click(object sender, RoutedEventArgs e)
            => NumValue -= 1024;

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value;

            if (txtNum == null)
                return;

            if (!int.TryParse(txtNum.Text, out value))
                txtNum.Text = value.ToString();

            NumValue = value;
        }
    }
}
