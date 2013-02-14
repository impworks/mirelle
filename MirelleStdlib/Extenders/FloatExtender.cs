using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Extenders
{
  public class FloatExtender
  {
    public static bool ToBool(double value)
    {
      return value == 0.0F ? false : true;
    }

    public static int ToInt(double value)
    {
      return (int)Math.Round(value);
    }

    public static string ToString(double value)
    {
      return value.ToString();
    }
  }
}
