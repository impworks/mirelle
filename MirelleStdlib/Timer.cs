using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib
{
  public class Timer
  {
    public static List<DateTime> Ticks = new List<DateTime>();

    /// <summary>
    /// Add a new timer to the tick list
    /// </summary>
    /// <returns></returns>
    public static int Tic()
    {
      Ticks.Add(DateTime.Now);
      return Ticks.Count() - 1;
    }

    /// <summary>
    /// Count the time difference
    /// </summary>
    /// <returns></returns>
    public static double Toc()
    {
      return CompareTimes(Ticks.Last(), DateTime.Now);
    }

    /// <summary>
    /// Count the time difference to a given timer
    /// </summary>
    /// <param name="id">Timer ID</param>
    /// <returns></returns>
    public static double Toc(int id)
    {
      DateTime date = DateTime.Now;
      if (id < Ticks.Count && id >= 0)
        date = Ticks[id];

      return CompareTimes(date, DateTime.Now);
    }

    /// <summary>
    /// Compare the time indicators
    /// </summary>
    /// <param name="from">Tic time</param>
    /// <param name="to">Toc time</param>
    /// <returns></returns>
    static public double CompareTimes(DateTime from, DateTime to)
    {
      var diff = to - from;
      return diff.TotalMilliseconds / 1000;
    }
  }
}
