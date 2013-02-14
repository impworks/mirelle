using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Editor
{
  public class MirelleCommands
  {
    static public RoutedCommand Compile = new RoutedCommand("Compile", typeof(MirelleCommands));
    static public RoutedCommand Run = new RoutedCommand("Run", typeof(MirelleCommands));
    static public RoutedCommand Info = new RoutedCommand("Info", typeof(MirelleCommands));

    static public RoutedCommand LangRussian = new RoutedCommand("Russian", typeof(MirelleCommands));
    static public RoutedCommand LangEnglish = new RoutedCommand("English", typeof(MirelleCommands));
  }
}
