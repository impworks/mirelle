using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Numerics;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace MirelleStdlib.Chart
{
  public class Series
  {
    public LineGraph Graph;
    public ObservableDataSource<Point> DataSource;

    public Series(ChartPlotter plotter, string descr, double thick, Color color)
    {
      DataSource = new ObservableDataSource<Point>();
      DataSource.SetXYMapping(pt => pt);

      plotter.Dispatcher.Invoke((Action)
        delegate()
        {
          Graph = plotter.AddLineGraph(DataSource, color, thick, descr);
        }
      );
    }
    
    /// <summary>
    /// Add a point to the graph
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    public void Plot(double x, double y)
    {
      DataSource.AppendAsync(Graph.Dispatcher, new Point(x, y));
    }

    /// <summary>
    /// Add a point given as a complex number to the graph
    /// </summary>
    /// <param name="point"></param>
    public void Plot(Complex point)
    {
      DataSource.AppendAsync(Graph.Dispatcher, new Point(point.Real, point.Imaginary));
    }


    /// <summary>
    /// Add many points as two arrays
    /// </summary>
    /// <param name="xs">Array of X coordinates</param>
    /// <param name="ys">Array of Y coordinates</param>
    public void Plot(double[] xs, double[] ys)
    {
      var count = Math.Min(xs.Length, ys.Length);
      var pts = new Point[count];
      for(int idx = 0; idx < count; idx++)
        pts[idx] = new Point(xs[idx], ys[idx]);

      Graph.Dispatcher.Invoke((Action)
        delegate()
        {
          DataSource.AppendMany(pts);
        }
      );
    }


    /// <summary>
    /// Add many points as complex numbers
    /// </summary>
    /// <param name="points"></param>
    public void Plot(Complex[] points)
    {
      var pts = new Point[points.Length];
      for (int idx = 0; idx < points.Length; idx++)
        pts[idx] = new Point(points[idx].Real, points[idx].Imaginary);

      Graph.Dispatcher.Invoke((Action)
        delegate()
        {
          DataSource.AppendMany(pts);
        }
      );
    }
  }
}
