using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Numerics;

namespace MirelleStdlib.Histogram
{
  public class Histogram
  {
    public HistogramWindow Window;
    public bool Ready = false;
    public bool Visible = true;

    /// <summary>
    /// Display window in another thread
    /// </summary>
    private void ShowThread()
    {
      Window = new HistogramWindow(this);
      Window.ShowDialog();
    }

    /// <summary>
    /// Display the window
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
    /// Display the histogram
    /// </summary>
    public Histogram()
    {
      Show();
    }

    /// <summary>
    /// Display the histogram and set caption
    /// </summary>
    /// <param name="caption"></param>
    public Histogram(string caption)
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
    /// Plot data
    /// </summary>
    /// <param name="data">Data as an array of complex values</param>
    public void Plot(Complex[] data)
    {
      Window.Histogram.SetData(data);
    }

    /// <summary>
    /// PLot data
    /// </summary>
    /// <param name="data">Data as an array of floating-point values</param>
    public void Plot(double[] data)
    {
      Window.Histogram.SetData(data);
    }

    /// <summary>
    /// Plot data
    /// </summary>
    /// <param name="captions">Array of bar captions as floats</param>
    /// <param name="data">Array of bar values</param>
    public void Plot(double[] captions, double[] data)
    {
      Window.Histogram.SetData(captions, data);
    }

    /// <summary>
    /// Plot data
    /// </summary>
    /// <param name="captions">Array of bar captions as strings</param>
    /// <param name="data">Array of bar values</param>
    public void Plot(string[] captions, double[] data)
    {
      Window.Histogram.SetData(captions, data);
    }
  }
}
