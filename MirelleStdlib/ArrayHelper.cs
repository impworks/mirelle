using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib
{
  public class ArrayHelper
  {
    /// <summary>
    /// Repeat the array contents several times
    /// </summary>
    /// <param name="arr">Source array</param>
    /// <param name="count">Times to repeat</param>
    /// <returns></returns>
    public static dynamic RepeatArray(dynamic src, int count)
    {
      int length = src.Length;

      // use simple routine for one-element array
      if (length == 1)
      {
        dynamic obj = src[0];
        dynamic arr = Activator.CreateInstance(src.GetType(), count);

        // check if there's no need to populate values?
        if (obj == null) return arr;
        if (obj.GetType() == typeof(int) && (int)obj == 0) return arr;
        if (obj.GetType() == typeof(double) && (double)obj == 0F) return arr;
        if (obj.GetType() == typeof(bool) && (bool)obj == false) return arr;

        for (int idx = 0; idx < count; idx++)
          arr[idx] = obj;
        return arr;
      }

      // or use Array.Copy for longer arrays
      else
      {
        dynamic dst = Activator.CreateInstance(src.GetType(), count * length);

        for (int idx = 0; idx < count; idx++)
          Array.Copy(src, 0, dst, idx * length, length);

        return dst;
      }
    }


    /// <summary>
    /// Create an array with a range of integer values
    /// </summary>
    /// <param name="from">Range start</param>
    /// <param name="to">Range end</param>
    /// <returns></returns>
    public static int[] CreateRangedArray(int from, int to)
    {
      int step = (from < to ? 1 : -1);
      int size = (step == 1 ? to - from : from - to);

      var arr = new int[size+1];
      for (int idx = 0; idx <= size; idx++, from+=step)
        arr[idx] = from;

      return arr;
    }

    /// <summary>
    /// Create an array with a range of floating point values, given a step
    /// </summary>
    /// <param name="from"></param>
    /// <param name="step"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static double[] CreateRangedArray(double from, double to, double step)
    {
      // parameter constrains
      if (from > to || step < 0) return null;

      int count = (int)Math.Ceiling((to - from) / step) + 1;
      var arr = new double[count];

      for(int idx = 0; idx < count; idx++)
      {
        arr[idx] = (from <= to ? from : to);
        from += step;
      }

      return arr;
    }

    /// <summary>
    /// Check if the array contains a value
    /// </summary>
    /// <param name="arr">Haystack</param>
    /// <param name="obj">Needle</param>
    /// <returns></returns>
    public static bool Has(dynamic arr, dynamic obj)
    {
      foreach(dynamic curr in arr)
      {
        if (Compare.Equal(curr, obj))
          return true;
      }

      return false;
    }

    /// <summary>
    /// Add two arrays
    /// </summary>
    /// <param name="arr1">First array</param>
    /// <param name="arr2">Second array</param>
    /// <returns></returns>
    public static dynamic AddArrays(dynamic arr1, dynamic arr2)
    {
      int length = arr1.Length + arr2.Length;

      dynamic arr = Activator.CreateInstance(arr1.GetType(), length);
      Array.Copy(arr1, arr, arr1.Length);
      Array.Copy(arr2, 0, arr, arr1.Length, arr2.Length);

      return arr;
    }
  }
}
