using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib
{
  public class IO
  {
    public static string[] Args;

    public static string Read()
    {
      return Console.ReadLine();
    }

    public static string Read(string prompt)
    {
      Console.Write(prompt);
      return Console.ReadLine();
    }

    public static void Wait()
    {
      Console.ReadKey(true);
    }
  }
}
