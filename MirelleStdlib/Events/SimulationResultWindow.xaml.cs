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

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace MirelleStdlib.Events
{
  /// <summary>
  /// Interaction logic for EventFlowWindow.xaml
  /// </summary>
  public partial class SimulationResultWindow : Window
  {
    public SimulationResultWindow()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Add a property
    /// </summary>
    /// <param name="name">Property name</param>
    /// <param name="value">Property value</param>
    public void AddProperty(string name, double value)
    {
      ptyList.Items.Add(new KeyValuePair<string, string>(name, value.ToString()));
    }

    /// <summary>
    /// Display data for both Processor graph and Queue graph
    /// </summary>
    /// <param name="procStats">Processor usage statistics</param>
    /// <param name="queueStats">Queue usage statistics per processor</param>
    public void DisplayGraphs(IEnumerable<Point> procStats, IEnumerable<IEnumerable<Point>> queueStats)
    {
      var colors = new[]
        {
          "#FFBF00",
          "#E32636",
          "#0000FF",
          "#008000",
          "#00FFFF",
          "#5D8AA8",
          "#4B5320",
          "#B8860B",
          "#6D351A",
          "#BDB76B",
          "#66FF00",
          "#CC5500",
          "#85BB65",
          "#DFFF00",
          "#8B008B"
        };

      // initialize processor usage graph
      var procSource = new ObservableDataSource<Point>();
      procSource.SetXYMapping(pt => pt);
      ProcessorUsageChart.AddLineGraph(procSource, Colors.Blue, 1, "Processor usage");
      procSource.AppendMany(procStats);

      var pidx = 1;
      foreach(var curr in queueStats)
      {
        var queueSource = new ObservableDataSource<Point>();
        procSource.SetXYMapping(pt => pt);
        QueueLengthChart.AddLineGraph(queueSource, (Color)ColorConverter.ConvertFromString(colors[(pidx-1) % colors.Length]), 2, "Processor #" + pidx.ToString() + " queue");
        queueSource.AppendMany(curr);
        pidx++;
      }
    }
  }
}
