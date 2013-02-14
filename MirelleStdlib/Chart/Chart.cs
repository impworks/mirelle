using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows.Threading;
using System.Windows.Media;

namespace MirelleStdlib.Chart
{
  public class Chart
  {
    public ChartWindow Window;
    public int SeriesCount = 0;
    public bool Ready = false;

    /// <summary>
    /// Create actual chart window in another thread
    /// </summary>
    private void ShowThread()
    {
      Window = new ChartWindow(this);
      Window.ShowDialog();
    }

    /// <summary>
    /// Show the graph
    /// </summary>
    private void Show()
    {
      var thread = new Thread(new ThreadStart(ShowThread));
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();

      while (!Ready)
        Thread.Sleep(10);
    }
    
    /// <summary>
    /// Display the graph
    /// </summary>
    public Chart()
    {
      Show();
    }

    /// <summary>
    /// Display the graph and set the window caption
    /// </summary>
    /// <param name="caption">Window caption</param>
    public Chart(string caption)
    {
      Show();
      Window.Dispatcher.Invoke((Action)
        delegate()
        {
          Window.Title = caption;
        }
      );
    }

    /// <summary>
    /// Create new series
    /// </summary>
    /// <returns></returns>
    public Series NewSeries()
    {
      SeriesCount++;
      return new Series(Window.Plotter, "Graph " + SeriesCount.ToString(), 1, ColorHelper.CreateRandomHsbColor());
    }

    /// <summary>
    /// Create new series with description
    /// </summary>
    /// <param name="descr">Description</param>
    /// <returns></returns>
    public Series NewSeries(string descr)
    {
      return new Series(Window.Plotter, descr, 1, ColorHelper.CreateRandomHsbColor());
    }

    /// <summary>
    /// Create series with description and color
    /// </summary>
    /// <param name="descr">Description</param>
    /// <param name="color">Color as integer</param>
    /// <returns></returns>
    public Series NewSeries(string descr, double thick)
    {
      return new Series(Window.Plotter, descr, thick, ColorHelper.CreateRandomHsbColor());
    }

    /// <summary>
    /// Create series with description, color and thickness
    /// </summary>
    /// <param name="descr">Description</param>
    /// <param name="color">Color as integer</param>
    /// <param name="thick">Thickness</param>
    /// <returns></returns>
    public Series NewSeries(string descr, double thick, int color)
    {
      return new Series(Window.Plotter, descr, thick, Colors.FromInt(color));
    }
  }
}
