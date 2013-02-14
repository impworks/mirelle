using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// The basic abstract class that defines the "action" method of a planner.
  /// Exists to allow stdlib's code in C# interact with custom planner
  /// written in Mirelle
  /// </summary>
  public abstract class Planner
  {
    public Planner() { }

    /// <summary>
    /// The planner action
    /// </summary>
    public abstract Symbol Action(Flow[] flows, Symbol lastSymbol);
  }
}
