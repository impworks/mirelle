using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Extenders
{
  public class IntExtender
  {
    public static bool ToBool(int value)
    {
      return value == 0 ? false : true;
    }

    public static double ToFloat(int value)
    {
      return (double)value;
    }

    public static string ToString(int value)
    {
      return value.ToString();
    }

    /// <summary>
    /// Convert the integer to a character
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Char(int value)
    {
      return new string((char)value, 1);
    }
  }
}
