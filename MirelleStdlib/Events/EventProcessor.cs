using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirelleStdlib.Events
{
  public class EventProcessor
  {
    /// <summary>
    /// Processor ID
    /// </summary>
    public int ProcessorID;

    /// <summary>
    /// Current length of the queue
    /// </summary>
    public Queue<Event> Queue = new Queue<Event>();

    /// <summary>
    /// Total number of processed tasks
    /// </summary>
    public int ProcessedTasks = 0;

    /// <summary>
    /// Flag indicating the processor is still busy
    /// </summary>
    public bool Busy = false;

    /// <summary>
    /// Total time processor spent processing tasks
    /// </summary>
    public double WorkDuration = 0;

    /// <summary>
    /// Maximum queue length of the processor
    /// </summary>
    public int MaxQueueLength = 0;

    /// <summary>
    /// Total time all events spent waiting in the queue
    /// </summary>
    public double TotalWaitTime = 0;

    /// <summary>
    /// Maximum waiting time for a task
    /// </summary>
    public double MaxWaitTime = 0;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id">Processor ID</param>
    public EventProcessor(int id)
    {
      ProcessorID = id;
    }

    /// <summary>
    /// Add an event to the queue
    /// </summary>
    /// <param name="evt"></param>
    public void Enqueue(Event evt)
    {
      Simulation.CollectStats();
      Queue.Enqueue(evt);
      MaxQueueLength = Math.Max(Queue.Count, MaxQueueLength);
    }

    /// <summary>
    /// Remove the event from the queue and apply it's properties to statistics
    /// </summary>
    public void Dequeue()
    {
      Simulation.CollectStats();

      var qc = Queue.Count;
      var evt = Queue.Dequeue();

      // collect statistics
      ProcessedTasks++;
      WorkDuration += evt.Duration;
      var waitTime = Simulation.Time - evt.Time;
      TotalWaitTime += waitTime;
      if(waitTime > MaxWaitTime)
        MaxWaitTime = waitTime;

      // free the processor
      Busy = false;
    }

    /// <summary>
    /// Try to process the current event
    /// </summary>
    public void AttemptProcess()
    {
      Simulation.CollectStats();

      if(!Busy && Queue.Count > 0)
      {
        Simulation.CurrentEvent = Queue.Peek();
        Busy = true;
        Simulation.CurrentEvent.ProcessEvent();

        // if event is prolonged and it's allowed, register a FreeEvent
        if (Simulation.AllowTimespans && Simulation.CurrentEvent.Duration > 0)
          Simulation.RegisterEvent(new FreeEvent(ProcessorID, Simulation.CurrentEvent.Duration));

        // or remove the event from the queue immediately
        else
          Dequeue();
      }
    }
  }
}
