using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MirelleStdlib.Events;

namespace MirelleStdlib.Wireless
{
  /// <summary>
  /// An event at which a new packet arrives to flow queue
  /// </summary>
  public class FlowEvent: Event
  {
    /// <summary>
    /// The flow this event is bound to
    /// </summary>
    public Flow Flow;

    public FlowEvent(Flow flow)
    {
      Flow = flow;

      // time
      // TODO: jitter!
      Time = Simulation.Time + 1.0 / Flow.Speed;
    }

    public override void ProcessEvent()
    {
      // add a packet to the flow queue
      Flow.Data.Add(Simulation.Time, Flow.PacketSize);
    }

    public override Event GenerateNext()
    {
      // create a new event for current flow
      return new FlowEvent(Flow);
    }
  }
}
