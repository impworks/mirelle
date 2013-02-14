using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mirelle
{
  public static class StringExtensions
  {
    /// <summary>
    /// Return a "safe substring": no exceptions when out of bounds
    /// </summary>
    /// <param name="str">Source string</param>
    /// <param name="offset">Offset</param>
    /// <returns></returns>
    public static string SafeSubstring(this string str, int offset)
    {
      if (offset >= str.Length)
        return "";
      else
        return str.Substring(offset);
    }

    /// <summary>
    /// Return a "safe substring": no exceptions when out of bounds
    /// </summary>
    /// <param name="str">Source string</param>
    /// <param name="offset">Offset</param>
    /// <param name="length">Substring length</param>
    /// <returns></returns>
    public static string SafeSubstring(this string str, int offset, int length)
    {
      if (offset >= str.Length)
        return "";
      else
        return str.Substring(offset, Math.Min(str.Length - offset, length));
    }

    /// <summary>
    /// Check if the string is in the given array
    /// </summary>
    /// <param name="str">Needle</param>
    /// <param name="arr">Haystack</param>
    /// <returns></returns>
    public static bool IsAnyOf(this string str, params string[] arr)
    {
      foreach (var curr in arr)
        if (curr == str)
          return true;

      return false;
    }

    /// <summary>
    /// Concatenate an array of strings
    /// </summary>
    /// <param name="arr">Array of strings to be concatenated</param>
    /// <param name="glue">Delimiter</param>
    /// <returns></returns>
    static public string Join(this string[] arr, string glue = "")
    {
      var sb = new StringBuilder();
      for (int idx = 0; idx < arr.Length; idx++)
      {
        if(idx > 0)
          sb.Append(glue);
        sb.Append(arr[idx]);
      }

      return sb.ToString();
    }
  }
}
