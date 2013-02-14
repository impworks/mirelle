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

namespace MirelleStdlib.Chart
{
  /// <summary>
  /// Interaction logic for ChartWindow.xaml
  /// </summary>
  public partial class ChartWindow : Window
  {
    private Chart Chart;

    public ChartWindow(Chart parent)
    {
      InitializeComponent();
      Chart = parent;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      Chart.Ready = true;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
        Close();
    }
  }
}
