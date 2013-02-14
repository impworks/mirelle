using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Events
{
  public class FreeEvent: Event
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="processor">ID of the process to free</param>
    public FreeEvent(int processor, double time)
    {
      Immediate = true;
      ProcessorID = processor;
      Time = Simulation.GetTime() + time;
    }

    /// <summary>
    /// Free the current processor from an event
    /// </summary>
    public override void ProcessEvent()
    {
      Simulation.CollectStats();
      var proc = Simulation.GetProcessor(ProcessorID);
      proc.Dequeue();
      proc.AttemptProcess();
    }

    public override Event GenerateNext()
    {
      return null;
    }
  }
}
