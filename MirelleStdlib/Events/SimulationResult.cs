using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace MirelleStdlib.Events
{
  public class SimulationResult
  {
    /// <summary>
    /// Number of emitters
    /// </summary>
    public int EmitterCount;

    /// <summary>
    /// Number of processors
    /// </summary>
    public int ProcessorCount;

    /// <summary>
    /// Number of events processed
    /// </summary>
    public int EventCount;

    /// <summary>
    /// Number of events discarded due to queue overflow
    /// </summary>
    public int DiscardedEventCount;

    /// <summary>
    /// Total simulation time
    /// </summary>
    public double Time;

    /// <summary>
    /// Maximal queue length
    /// </summary>
    public int MaxQueue;

    /// <summary>
    /// Average waiting time
    /// </summary>
    public double AvgWait;

    /// <summary>
    /// Maximal waiting time
    /// </summary>
    public double MaxWait;
    
    /// <summary>
    /// Array of points representing number of reserved processors per time
    /// </summary>
    public Complex[] ProcessorGraphPoints;

    /// <summary>
    /// 2D array of points representing queue length per processor per time
    /// </summary>
    public Complex[][] QueueGraphPoints;

    /// <summary>
    /// Display the form for with general results
    /// </summary>
    public void Show()
    {
      // set properties
      var window = new SimulationResultWindow();
      window.AddProperty("Processor count", ProcessorCount);
      window.AddProperty("Simulation time", Time);
      window.AddProperty("Event series count", EmitterCount);
      window.AddProperty("Total event count", EventCount);
      window.AddProperty("Maximum waiting time", MaxWait);
      window.AddProperty("Average waiting time", AvgWait);
      window.AddProperty("Maximum queue length", MaxQueue);

      // display graphs
      window.DisplayGraphs(GetPoints(ProcessorGraphPoints), GetPointsList(QueueGraphPoints));

      window.ShowDialog();
    }

    /// <summary>
    /// Convert array of complex to array of points
    /// </summary>
    /// <param name="data">Array of complex numbers</param>
    /// <returns></returns>
    private IEnumerable<Point> GetPoints(Complex[] data)
    {
      foreach (var curr in data)
        yield return new Point(curr.Real, curr.Imaginary);
    }

    /// <summary>
    /// Convert a nested array of complex to points
    /// </summary>
    /// <param name="data">Nested array of complex numbers</param>
    /// <returns></returns>
    private IEnumerable<IEnumerable<Point>> GetPointsList(Complex[][] data)
    {
      foreach (var curr in data)
        yield return GetPoints(curr);
    }
  }
}
