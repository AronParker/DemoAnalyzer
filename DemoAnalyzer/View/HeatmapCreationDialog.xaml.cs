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
using System.Windows.Shapes;

namespace DemoAnalyzer.View
{
    /// <summary>
    /// Interaction logic for HeatmapCreationDialog.xaml
    /// </summary>
    public partial class HeatmapCreationDialog : Window
    {
        public int HeatmapFactor { get; private set; }

        public HeatmapCreationDialog()
        {
            InitializeComponent();
        }

        private void SmallButton_Click(object sender, RoutedEventArgs e)
        {
            HeatmapFactor = 64;
            DialogResult = true;
        }

        private void MediumButton_Click(object sender, RoutedEventArgs e)
        {
            HeatmapFactor = 96;
            DialogResult = true;
        }

        private void LargeButton_Click(object sender, RoutedEventArgs e)
        {
            HeatmapFactor = 128;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
