using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MND = MathNet.Numerics.Distributions;

namespace MirelleStdlib.Events
{
  public abstract class EventEmitter
  {
    /// <summary>
    /// Maximum number of events to be emitted
    /// </summary>
    public int Limit;

    /// <summary>
    /// Number of events emitted so far
    /// </summary>
    public int EventCount;

    /// <summary>
    /// Average rate of events arriving
    /// </summary>
    public double Step;

    /// <summary>
    /// Distribution that describes variance
    /// </summary>
    public MND.IContinuousDistribution Distribution;

    /// <summary>
    /// An action to be performed when an event is processed
    /// </summary>
    public abstract void Action();

    /// <summary>
    /// A condition to test whether new events should be emitted.
    /// Is to be overridden in child classes if a condition is specified
    /// </summary>
    /// <returns></returns>
    public virtual bool Condition()
    {
      return false;
    }

    /// <summary>
    /// Emit a new event, if the conditions still allow events to be emitted
    /// </summary>
    /// <returns></returns>
    public GenericEvent Emit()
    {
      // ensure event count limit has not been met
      if (Limit > 0 && EventCount >= Limit)
        return null;

      // ensure the finish condition has not been met
      if (Condition())
        return null;

      EventCount++;
      return new GenericEvent(this);
    }

    public EventEmitter()
    {
      Limit = 0;
      EventCount = 0;
      Step = 0;
      Simulation.EmitterCount++;
    }
  }
}
