using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Events
{
  public abstract class Event
  {
    /// <summary>
    /// When the event occured
    /// </summary>
    public double Time = 0;

    /// <summary>
    /// Check whether event should be processed immediately or put into queue
    /// Only the FreeEvents are immediate
    /// </summary>
    public bool Immediate = false;

    /// <summary>
    /// Desired Processor ID to execute on
    /// -1 for autodetection
    /// </summary>
    public int ProcessorID = -1;

    /// <summary>
    /// Stores the duration of the event if it's been a prolonged one
    /// </summary>
    public double Duration = 0;

    /// <summary>
    /// Event emitter reference
    /// </summary>
    public EventEmitter Emitter;

    /// <summary>
    /// Action to be invoked when event occurs
    /// </summary>
    public abstract void ProcessEvent();

    /// <summary>
    /// Method creating a new 
    /// </summary>
    /// <returns></returns>
    public abstract Event GenerateNext();

    /// <summary>
    /// Compare two events
    /// </summary>
    /// <param name="a">First event</param>
    /// <param name="b">Second event</param>
    /// <returns></returns>
    public static int Compare(Event a, Event b)
    {
      // check for nulls
      if(a == null)
        return b == null ? 0 : -1;
      else if (b == null)
        return 1;

      if (a.Time == b.Time)
        return 0;
      else
        return a.Time > b.Time ? 1 : -1;
    }
  }
}
