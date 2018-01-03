using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindowsFormsApplication1
{
    /// <summary>
    /// Interaction logic for Kontrol1.xaml
    /// </summary>
    public partial class Kontrol1 : UserControl
    {
     
        public int state_map = 0;
        public Kontrol1()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = ((ComboBox)sender).SelectedItem as ComboBoxItem;

            state_map = ((ComboBox)sender).SelectedIndex;
            //MessageBox.Show(cbi.Content.ToString());
        }
    }
}
