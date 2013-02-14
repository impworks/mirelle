using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MN = MathNet.Numerics.Distributions;

namespace MirelleStdlib.Events
{
  public class GenericEvent: Event
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="emitter">Parent emitter of the event</param>
    public GenericEvent(EventEmitter emitter)
    {
      Emitter = emitter;

      // calculate time jitter
      double timeJitter = 0;
      if(emitter.Distribution != null)
      {
        // allow user to wobble in both directions
        if(emitter.Distribution is MN.Normal)
          timeJitter = emitter.Distribution.Sample() - emitter.Distribution.Sample();
        else
          timeJitter = emitter.Distribution.Sample();
      }

      var distance = emitter.Step + timeJitter;
      if (distance < 0)
        throw new ArgumentOutOfRangeException("Event cannot be created with negative time offset!");

      Time = Simulation.GetTime() + emitter.Step + timeJitter;
    }

    public override void ProcessEvent()
    {
      // invoke the action
      Emitter.Action();
    }

    public override Event GenerateNext()
    {
      return Emitter.Emit();
    }
  }
}
