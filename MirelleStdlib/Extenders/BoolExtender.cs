using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Extenders
{
  public class BoolExtender
  {
    public static int ToInt(bool value)
    {
      return value ? 1 : 0;
    }

    public static double ToFloat(bool value)
    {
      return value ? 0.0 : 1.0;
    }

    public static string ToString(bool value)
    {
      return value ? "true" : "false";
    }
  }
}
