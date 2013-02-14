using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Globalization;

namespace MirelleStdlib.Extenders
{
  public class StringExtender
  {
    public static bool ToBool(string str)
    {
      return str == null || str == "" ? false : true;
    }

    /// <summary>
    /// Check if the string contains a valid integer
    /// </summary>
    /// <param name="str">String</param>
    /// <returns></returns>
    public static bool IsInt(string str)
    {
      int result;
      return int.TryParse(str, out result);
    }

    /// <summary>
    /// Retrieve an integer from string or return a 0
    /// </summary>
    /// <param name="str">String</param>
    /// <returns></returns>
    public static int ToInt(string str)
    {
      int result;
      if (int.TryParse(str, out result))
        return result;
      else
        return 0;
    }

    /// <summary>
    /// Check if the string contains a valid float
    /// </summary>
    /// <param name="str">String</param>
    /// <returns></returns>
    public static bool IsFloat(string str)
    {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      double result;
      return double.TryParse(str, out result);
    }

    /// <summary>
    /// Retrieve a float from string or return a 0.0
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static double ToFloat(string str)
    {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
      double result;
      if (double.TryParse(str, out result))
        return result;
      else
        return 0.0;
    }

    /// <summary>
    /// Reverse the string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Reverse(string str)
    {
      var arr = str.ToCharArray();
      Array.Reverse(arr);
      return new string(arr);
    }

    /// <summary>
    /// Get the string's length
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int Size(string str)
    {
      return str.Length;
    }

    /// <summary>
    /// Split the string with a delimiter
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimiter">Delimiter string</param>
    /// <returns></returns>
    public static string[] Split(string str, string delimiter)
    {
      return str.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Split the string with a delimiter to a maximum of N times
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimiter">Delimiter string</param>
    /// <returns></returns>
    public static string[] Split(string str, string delimiter, int count)
    {
      return str.Split(new[] { delimiter }, count, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Join the array of strings with a glue
    /// </summary>
    /// <param name="str">Glue</param>
    /// <param name="values">Strings</param>
    /// <returns></returns>
    public static string Join(string str, string[] values)
    {
      return String.Join(str, values);
    }

    /// <summary>
    /// Repeat the string N times
    /// </summary>
    /// <param name="str">String</param>
    /// <param name="count">Count</param>
    /// <returns></returns>
    public static string Repeat(string str, int count)
    {
      if (count < 0) return str;
      if (count == 0) return "";
      var sb = new StringBuilder(str.Length * count);
      return sb.Insert(0, str, count).ToString();
    }

    /// <summary>
    /// Return a single-character string
    /// </summary>
    /// <param name="str">Source string</param>
    /// <param name="pos">Position</param>
    /// <returns></returns>
    public static string CharAt(string str, int pos)
    {
      if (pos >= 0 && pos < str.Length)
        return str[pos].ToString();
      else
        return "";
    }

    /// <summary>
    /// Return the first char of the string as an integer
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int Ord(string str)
    {
      if (str.Length == 0)
        return 0;
      else
        return (int)str[0];
    }

    /// <summary>
    /// Return a newline character
    /// </summary>
    /// <returns></returns>
    public static string NewLine()
    {
      return "\n";
    }

    /// <summary>
    /// Return a CR LF
    /// </summary>
    /// <returns></returns>
    public static string EndLine()
    {
      return "\r\n";
    }

    /// <summary>
    /// Return a tab character
    /// </summary>
    /// <returns></returns>
    public static string Tab()
    {
      return "\t";
    }

    /// <summary>
    /// Return the quote character
    /// </summary>
    /// <returns></returns>
    public static string Quote()
    {
      return "\"";
    }
  }
}
