using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MirelleStdlib.Events;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// The planner event that occurs each time when planner should be executed
  /// </summary>
  public class PlannerEvent: Event
  {
    public PlannerEvent()
    {
      // time = block size / data per second
      Time = Simulation.Time + (FlowSimulation.BlockSize * FlowSimulation.BlocksPerSymbol) / ((double)FlowSimulation.Speed);
    }

    public override void ProcessEvent()
    {
      // parse old symbol at the start of current
      if(FlowSimulation.OldSymbol != null)
        FlowSimulation.OldSymbol = FlowSimulation.ProcessSymbol(FlowSimulation.OldSymbol);

      // invoke planner action
      FlowSimulation.OldSymbol = FlowSimulation.Planner.Action(FlowSimulation.Flows, FlowSimulation.OldSymbol);

      // check for finalization
      if (FlowSimulation.OldSymbol == null || Simulation.GetTime() > FlowSimulation.MaxTime)
        Simulation.Stop();
    }

    public override Event GenerateNext()
    {
      return new PlannerEvent();
    }
  }
}
