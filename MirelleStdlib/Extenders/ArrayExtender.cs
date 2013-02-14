using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Extenders
{
  public class ArrayExtender
  {
    /// <summary>
    /// Sort an array using a sort function
    /// </summary>
    /// <typeparam name="T">Array item type</typeparam>
    /// <param name="array">Array</param>
    /// <param name="functor">Sort function</param>
    public static void Sort<T>(T[] array, Func<T, T, bool> functor)
    {
      // dumb case
      if (array.Length < 2) return;

      // traverse items
      for(int idx = 0; idx < array.Length - 1; idx++)
      {
        var max = array[idx];
        var maxid = idx;

        // find a to-be-changed element in the rest of the array
        for(int idx2 = idx + 1; idx2 < array.Length; idx2++)
        {
          if(functor(array[idx2], max))
          {
            max = array[idx2];
            maxid = idx2;
          }
        }

        if(maxid != idx)
        {
          var tmp = array[idx];
          array[idx] = array[maxid];
          array[maxid] = tmp;
        }
      }
    }

    /// <summary>
    /// Find the item in the array that best matches the functor
    /// </summary>
    /// <typeparam name="T">Array item type</typeparam>
    /// <param name="array">Array</param>
    /// <param name="functor">Comparator function</param>
    /// <returns></returns>
    public static T FindBest<T>(IEnumerable<T> array, Func<T, T, bool> functor)
    {
      var en = array.GetEnumerator();

      // make sure there's at least one item in the collection
      if (!en.MoveNext())
        return default(T);

      // traverse all items
      var curr = en.Current;
      while(en.MoveNext())
      {
        if (functor(curr, en.Current))
          curr = en.Current;
      }

      return curr;
    }

    /// <summary>
    /// Sort an integer array
    /// </summary>
    /// <param name="array">Array of integers</param>
    public static void Sort(int[] array)
    {
      Sort(array, (a, b) => a > b);
    }

    /// <summary>
    /// Sort a float array
    /// </summary>
    /// <param name="array">Array of floats</param>
    public static void Sort(double[] array)
    {
      Sort(array, (a, b) => a > b);
    }

    /// <summary>
    /// Sort a string array
    /// </summary>
    /// <param name="array">Array of strings</param>
    public static void Sort(string[] array)
    {
      Sort(array, (a, b) => a.CompareTo(b) > 0);
    }

    /// <summary>
    /// Sort an integer array reverse
    /// </summary>
    /// <param name="array">Array of integers</param>
    public static void Sort(int[] array, bool reverse)
    {
      Sort(array, (a, b) => a < b);
    }

    /// <summary>
    /// Sort a float array
    /// </summary>
    /// <param name="array">Array of floats</param>
    public static void Sort(double[] array, bool reverse)
    {
      Sort(array, (a, b) => a < b);
    }

    /// <summary>
    /// Sort a string array
    /// </summary>
    /// <param name="array">Array of strings</param>
    public static void Sort(string[] array, bool reverse)
    {
      Sort(array, (a, b) => a.CompareTo(b) < 0);
    }

    /// <summary>
    /// Find the minimal item in the array
    /// </summary>
    /// <param name="array">Array</param>
    /// <returns></returns>
    public static int Min(int[] array)
    {
      return FindBest(array, (a, b) => a < b);
    }

    /// <summary>
    /// Find the minimal item in the array
    /// </summary>
    /// <param name="array">Array</param>
    /// <returns></returns>
    public static double Min(double[] array)
    {
      return FindBest(array, (a, b) => a < b);
    }

    /// <summary>
    /// Find the minimal item in the array
    /// </summary>
    /// <param name="array">Array</param>
    /// <returns></returns>
    public static string Min(string[] array)
    {
      return FindBest(array, (a, b) => a.CompareTo(b) < 0);
    }

    /// <summary>
    /// Find the maximal item in the array
    /// </summary>
    /// <param name="array">Array</param>
    /// <returns></returns>
    public static int Max(int[] array)
    {
      return FindBest(array, (a, b) => a > b);
    }

    /// <summary>
    /// Find the maximal item in the array
    /// </summary>
    /// <param name="array">Array</param>
    /// <returns></returns>
    public static double Max(double[] array)
    {
      return FindBest(array, (a, b) => a > b);
    }

    /// <summary>
    /// Find the maximal item in the array
    /// </summary>
    /// <param name="array">Array</param>
    /// <returns></returns>
    public static string Max(string[] array)
    {
      return FindBest(array, (a, b) => a.CompareTo(b) > 0);
    }
  }
}
