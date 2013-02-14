using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.IO;

namespace MirelleStdlib
{
  public static class Initializer
  {
    public static void Initialize()
    {
      Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
    }
  }
}
