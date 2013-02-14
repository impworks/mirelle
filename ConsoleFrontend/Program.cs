using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleFrontend
{
  class Program
  {
    static void Main(string[] args)
    {
      try
      {
        if (args.Length == 0)
          Console.WriteLine("Mirelle console compiler\nUsage: mirellecc.exe filename.mr");
        else
        {
          var arg = args[0];
          var cpl = new Mirelle.Compiler();
          cpl.Process(arg);

          var fi = new FileInfo(arg);
          cpl.SaveAssembly(fi.Name + ".exe");

          Console.WriteLine("Compiled successfully.");
        }
      }
      catch (Mirelle.CompilerException ex)
      {
        Console.WriteLine(ex.FormattedMessage());
      }
    }
  }
}
