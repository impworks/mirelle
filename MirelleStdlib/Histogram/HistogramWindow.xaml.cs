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

namespace MirelleStdlib.Histogram
{
  /// <summary>
  /// Interaction logic for HistogramWindow.xaml
  /// </summary>
  public partial class HistogramWindow : Window
  {
    private Histogram Hist;
    public HistogramWindow(Histogram hist)
    {
      InitializeComponent();
      Hist = hist;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      Hist.Ready = true;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
        Close();
    }

    private void Window_Closing(object sender, EventArgs e)
    {
      Hist.Visible = false;
    }
  }
}
